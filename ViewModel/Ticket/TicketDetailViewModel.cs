using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class TicketDetailViewModel
    {
        public int TicketID { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MovieTitle { get; set; }
        public TimeSpan ScreeningTime { get; set; }
        public string RoomName { get; set; }
        public string SeatNumber { get; set; }
        public DateTime? BookingTime { get; set; }
        public decimal TotalPrice { get; set; }
        public byte? PaymentStatus { get; set; }


    }
}