document.addEventListener("DOMContentLoaded", function () {
    const unavailableRanges = window.unavailableRanges;
    const pricePerNight = window.pricePerNight;

    flatpickr("#date-range", {
        mode: "range",
        minDate: "today",
        dateFormat: "Y-m-d",
        disable: unavailableRanges.map(b => ({
            from: b.start,
            to: b.end
        })),
        onChange: function (selectedDates) {
            if (selectedDates.length === 2) {
                const formatDate = (d) => {
                    const year = d.getFullYear();
                    const month = String(d.getMonth() + 1).padStart(2, '0');
                    const day = String(d.getDate()).padStart(2, '0');
                    return `${year}-${month}-${day}`;
                };

                document.getElementById("StartDate").value = formatDate(selectedDates[0]);
                document.getElementById("EndDate").value = formatDate(selectedDates[1]);
            }

            updateBookingSummary(selectedDates);
        }
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/bookingHub")
        .build();

    connection.on("ReceiveNewBooking", function (data) {
        console.log("SignalR reçu :", data);

        const currentPropertyId = parseInt(document.querySelector("input[name='PropertyId']").value);

        if (data.propertyId !== currentPropertyId) return;

        window.unavailableRanges.push({
            start: data.start,
            end: data.end
        });

        refreshCalendar();
    });

    connection.start().catch(e => console.error("SignalR error", e));

    function updateBookingSummary(selectedDates) {
        const summaryDiv = document.getElementById('booking-summary');
        const submitBtn = document.getElementById('stripe-booking');

        if (selectedDates.length === 2) {
            const startDate = selectedDates[0];
            const endDate = selectedDates[1];
            const nights = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));
            const totalPrice = nights * pricePerNight;

            document.getElementById('checkin-date').textContent = startDate.toLocaleDateString();
            document.getElementById('checkout-date').textContent = endDate.toLocaleDateString();
            document.getElementById('total-nights').textContent = nights;
            document.getElementById('total-price').textContent = totalPrice;
            document.getElementById('amount').value = Math.round(totalPrice * 100);

            summaryDiv.classList.remove('hidden');
            submitBtn.disabled = false;

            summaryDiv.style.opacity = '0';
            summaryDiv.style.transform = 'translateY(20px)';
            setTimeout(() => {
                summaryDiv.style.transition = 'all 0.3s ease';
                summaryDiv.style.opacity = '1';
                summaryDiv.style.transform = 'translateY(0)';
            }, 10);
        } else {
            summaryDiv.classList.add('hidden');
            submitBtn.disabled = true;
        }
    }

    function refreshCalendar() {
        const calendarEl = document.querySelector("#date-range");
        if (calendarEl._flatpickr) {
            calendarEl._flatpickr.destroy();
        }

        flatpickr("#date-range", {
            mode: "range",
            minDate: "today",
            dateFormat: "Y-m-d",
            disable: window.unavailableRanges.map(b => ({
                from: b.start,
                to: b.end
            })),
            onChange: function (selectedDates) {
                if (selectedDates.length === 2) {
                    const formatDate = (d) => {
                        const year = d.getFullYear();
                        const month = String(d.getMonth() + 1).padStart(2, '0');
                        const day = String(d.getDate()).padStart(2, '0');
                        return `${year}-${month}-${day}`;
                    };

                    document.getElementById("StartDate").value = formatDate(selectedDates[0]);
                    document.getElementById("EndDate").value = formatDate(selectedDates[1]);
                }

                updateBookingSummary(selectedDates);
            }
        });
    }

    // Star rating
    let currentRating = 0;

    function setRating(rating) {
        currentRating = rating;
        document.getElementById('rating-value').value = rating;

        const stars = document.querySelectorAll('.star-icon');
        stars.forEach((star, index) => {
            if (index < rating) {
                star.classList.remove('text-gray-300');
                star.classList.add('text-yellow-400');
            } else {
                star.classList.remove('text-yellow-400');
                star.classList.add('text-gray-300');
            }
        });
    }

    window.setRating = setRating;

    document.querySelectorAll('.star-icon').forEach((star, index) => {
        star.addEventListener('click', function () {
            setRating(index + 1);
        });

        star.addEventListener('mouseenter', function () {
            const hoverRating = index + 1;
            const stars = document.querySelectorAll('.star-icon');

            stars.forEach((s, i) => {
                if (i < hoverRating) {
                    s.classList.remove('text-gray-300');
                    s.classList.add('text-yellow-500');
                } else {
                    s.classList.remove('text-yellow-500');
                    s.classList.add('text-gray-300');
                }
            });
        });

        star.addEventListener('mouseleave', function () {
            const stars = document.querySelectorAll('.star-icon');
            stars.forEach((s, i) => {
                if (i < currentRating) {
                    s.classList.remove('text-gray-300', 'text-yellow-500');
                    s.classList.add('text-yellow-400');
                } else {
                    s.classList.remove('text-yellow-400', 'text-yellow-500');
                    s.classList.add('text-gray-300');
                }
            });
        });
    });

    // Flash messages
    setTimeout(() => {
        document.querySelectorAll('.flash-message').forEach(el => {
            el.classList.add('opacity-0');
            setTimeout(() => el.remove(), 500);
        });
    }, 5000);
}
);