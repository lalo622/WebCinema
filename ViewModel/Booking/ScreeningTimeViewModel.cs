using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Booking
{
    public class ScreeningTimeViewModel
    {
        public int ScreeningId { get; set; }
        public TimeSpan StartTime { get; set; }
        public string DisplayTime { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int AvailableSeats { get; set; }
        public decimal BasePrice { get; set; }
    }
}