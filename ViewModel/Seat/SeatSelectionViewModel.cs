using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Seat
{
    public class SeatSelectionViewModel
    {
        public int ScreeningId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaName { get; set; }
        public string RoomName { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }

        public SeatMapViewModel SeatMap { get; set; }
        public List<string> SelectedSeats { get; set; } = new List<string>();
        public decimal TotalPrice { get; set; }

        // Thời gian đặt vé (15 phút)
        public DateTime BookingStartTime { get; set; }
        public DateTime BookingExpireTime { get; set; }
    }
}