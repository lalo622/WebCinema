using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Profile
{
    public class PointHistoryViewModel
    {
        public string Description { get; set; }
        public int PointChange { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPositive => PointChange > 0;
    }
}