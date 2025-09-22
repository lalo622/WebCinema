using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class MovieRevenueViewModel
    {
        public int MovieID { get; set; }
        public string Title { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}