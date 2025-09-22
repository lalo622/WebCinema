using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.ViewModel.Booking;

namespace WebCinema.ViewModel
{
    public class MovieBookingViewModel
    {
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MoviePoster { get; set; }
        public string MovieDuration { get; set; }

        public DateTime SelectedDate { get; set; }
        public List<DateOption> AvailableDates { get; set; } = new List<DateOption>();
        public List<CinemaScreeningViewModel> CinemaScreenings { get; set; } = new List<CinemaScreeningViewModel>();
    }
}