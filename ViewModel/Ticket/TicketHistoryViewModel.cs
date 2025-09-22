using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class TicketHistoryViewModel
    {
        public int TicketID { get; set; }
        public string MovieTitle { get; set; }
        public string MovieImageUrl { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan ScreeningTime { get; set; }
        public string RoomName { get; set; }
        public string SeatNumber { get; set; }
        public decimal TotalPrice { get; set; }
    }

}