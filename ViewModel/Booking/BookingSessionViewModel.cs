using System;
using WebCinema.ViewModel.Payment;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Booking
{
    public class BookingSessionViewModel
    {
        public string SessionId { get; set; }
        public int ScreeningId { get; set; }
        public List<string> SelectedSeats { get; set; }
        public DateTime BookingStartTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public decimal TotalPrice { get; set; }

        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }

        public List<SelectedFoodViewModel> SelectedFoods { get; set; } = new List<SelectedFoodViewModel>();
        public List<SelectedComboViewModel> SelectedCombos { get; set; } = new List<SelectedComboViewModel>();
        public decimal? FoodPrice { get; set; }
        public decimal? ComboPrice { get; set; }
        public int? PointsUsed { get; set; } = 0;
        public decimal? PointDiscount { get; set; } = 0;
        public decimal FinalPrice => TotalPrice - (PointDiscount ?? 0);
    }

}