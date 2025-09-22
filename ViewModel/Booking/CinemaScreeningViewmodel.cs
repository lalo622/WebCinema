using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Booking
{
    public class CinemaScreeningViewModel
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
        public string CinemaAddress { get; set; }
        public List<ScreeningTimeViewModel> ScreeningTimes { get; set; } = new List<ScreeningTimeViewModel>();
    }
}