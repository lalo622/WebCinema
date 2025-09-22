using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class RevenueStatisticViewModel
    {
        public decimal RevenueToday { get; set; }
        public decimal RevenueYesterday { get; set; }
        public decimal PercentToday { get; set; }

        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueLastWeek { get; set; }
        public decimal PercentThisWeek { get; set; }

        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal PercentThisMonth { get; set; }

        public decimal RevenueThisYear { get; set; }
        public decimal RevenueLastYear { get; set; }
        public decimal PercentThisYear { get; set; }
        public List<MovieRevenueViewModel> TopMovies { get; set; }
    }
}