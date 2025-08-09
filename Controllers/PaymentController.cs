using BookingSystem.Data;
using BookingSystem.HubSignalR;
using BookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace BookingSystem.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<BookingHub> hubContext,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession(
            [FromForm] string productName,
            [FromForm] long amount,
            [FromForm] int propertyId,
            [FromForm] DateTime startDate,
            [FromForm] DateTime endDate)
        {
            Console.WriteLine("Call the method");
            try
            {
                if (startDate.Kind == DateTimeKind.Unspecified)
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                else if (startDate.Kind == DateTimeKind.Local)
                    startDate = startDate.ToUniversalTime();

                if (endDate.Kind == DateTimeKind.Unspecified)
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                else if (endDate.Kind == DateTimeKind.Local)
                    endDate = endDate.ToUniversalTime();


                _logger.LogInformation($"Stripe API Key: {Stripe.StripeConfiguration.ApiKey?.Substring(0, 15)}...");

                if (string.IsNullOrEmpty(productName) || amount <= 0 || propertyId <= 0)
                {
                    _logger.LogWarning("Validation failed !");
                    return BadRequest("Data not good");
                }

                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null)
                {
                    _logger.LogWarning($"Property not found: {propertyId}");
                    return NotFound("Property not find ");
                }

                if (startDate >= endDate || startDate < DateTime.UtcNow.Date)
                {
                    _logger.LogWarning($"Invalid dates: {startDate} - {endDate}");
                    return BadRequest("Dates unavailable");
                }

                var isAvailable = await CheckAvailability(propertyId, startDate, endDate);
                if (!isAvailable)
                {
                    _logger.LogWarning($"Property {propertyId} not available for {startDate} - {endDate}");
                    return BadRequest("Dates not available");
                }

                HttpContext.Session.SetInt32("PropertyId", propertyId);
                HttpContext.Session.SetString("StartDate", startDate.ToString("o"));
                HttpContext.Session.SetString("EndDate", endDate.ToString("o"));

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = amount,
                                Currency = "eur",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = productName,
                                    Description = $"Reservation between {startDate:dd/MM/yyyy} and {endDate:dd/MM/yyyy}",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/payment/success?session_id={{{{CHECKOUT_SESSION_ID}}}}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/payment/cancel",
                    Metadata = new Dictionary<string, string>
                    {
                        ["PropertyId"] = propertyId.ToString(),
                        ["StartDate"] = startDate.ToString("yyyy-MM-dd"),
                        ["EndDate"] = endDate.ToString("yyyy-MM-dd")
                    }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                _logger.LogInformation($"Stripe session created: {session.Id} for property {propertyId}");
                ViewBag.StripePublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout session");
                Console.WriteLine("Stripe error: " + ex.Message);
                return StatusCode(500, $"Error in the payment's session creating {ex.ToString()}");
            }
        }

        [HttpGet("success")]
        public async Task<IActionResult> Success()
        {
            try
            {
                int? propertyId = HttpContext.Session.GetInt32("PropertyId");
                string? startStr = HttpContext.Session.GetString("StartDate");
                string? endStr = HttpContext.Session.GetString("EndDate");

                if (propertyId == null || startStr == null || endStr == null)
                {
                    _logger.LogWarning("Missing session data for booking confirmation");
                    TempData["Error"] = "Impossible to confirm reservation.";
                    return RedirectToAction("Index", "Home");
                }

                var startDate = DateTime.SpecifyKind(DateTime.Parse(startStr).Date.AddHours(16), DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(DateTime.Parse(endStr).Date.AddHours(11), DateTimeKind.Utc);

                var isStillAvailable = await CheckAvailability(propertyId.Value, startDate, endDate);
                if (!isStillAvailable)
                {
                    _logger.LogWarning($"Property {propertyId} no longer available after payment");
                    TempData["Error"] = "the date are not available yet.";
                    return RedirectToAction("Details", "Property", new { id = propertyId });
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated for booking");
                    TempData["Error"] = "Utilisateur no authentificate.";
                    return RedirectToAction("Index", "Home");
                }

                var booking = new Booking
                {
                    PropertyId = propertyId.Value,
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId,
                    CreateAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveNewBooking", new
                {
                    propertyId = propertyId,
                    start = startDate.ToString("yyyy-MM-dd"),
                    end = endDate.ToString("yyyy-MM-dd")
                });

                HttpContext.Session.Remove("PropertyId");
                HttpContext.Session.Remove("StartDate");
                HttpContext.Session.Remove("EndDate");

                _logger.LogInformation($"Booking created successfully: {booking.Id}");

                TempData["Success"] = "Payment success. Reservation confirmed.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing successful payment");
                TempData["Error"] = $"Error during the reservation. { ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            HttpContext.Session.Remove("PropertyId");
            HttpContext.Session.Remove("StartDate");
            HttpContext.Session.Remove("EndDate");

            TempData["Info"] = "Payment canceled.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<bool> CheckAvailability(int propertyId, DateTime startDate, DateTime endDate)
        {
            var overlapping = await _context.Bookings
                .Where(b => b.PropertyId == propertyId &&
                    ((startDate >= b.StartDate && startDate < b.EndDate) ||
                     (endDate > b.StartDate && endDate <= b.EndDate) ||
                     (startDate <= b.StartDate && endDate >= b.EndDate)))
                .AnyAsync();

            return !overlapping;
        }
    }
}