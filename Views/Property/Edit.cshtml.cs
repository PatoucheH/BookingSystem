using BookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingSystem.Models;
using BookingSystem.Services; 

namespace BookingSystem.Pages.Properties
{
    public class EditModel : PageModel
    {
        private readonly IPropertyService _propertyService;

        public EditModel(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [BindProperty]
        public Property Property { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Property = await _propertyService.GetPropertyById(id);

            if (Property == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existingProperty = await _propertyService.GetPropertyById(Property.Id);
            if (existingProperty == null)
                return NotFound();

            existingProperty.Title = Property.Title;
            existingProperty.Description = Property.Description;
            existingProperty.Price = Property.Price;
            existingProperty.GuestNbr = Property.GuestNbr;

            await _propertyService.UpdateAsync(existingProperty);

            return RedirectToPage("/Property/Details"); 
        }
    }
}
