using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.Models;

namespace WebCinema.ViewModel
{
    public class TicketPricingViewModel
    {
        // Ghế
        public List<SeatTypePrice> SeatPrices { get; set; }

        // Phụ thu phòng
        public List<RoomType> RoomSurcharges { get; set; }

    }
}