using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Seat
{
    public class SeatMapViewModel
    {
        public int TemplateId { get; set; }
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public List<SeatViewModel> Seats { get; set; } = new List<SeatViewModel>();

        // Giá theo loại ghế
        public decimal RegularSeatPrice { get; set; }
        public decimal VipSeatPrice { get; set; }
        public decimal CoupleSeatPrice { get; set; }
    }
}