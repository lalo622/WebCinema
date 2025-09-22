using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Seat
{
    public class SeatViewModel
    {
        public string SeatNumber { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public SeatType SeatType { get; set; }
        
        public decimal Price { get; set; }
        public string DisplayName { get; set; }
        public bool IsBooked { get; set; }
        public bool IsSelected { get; set; }
    }
}