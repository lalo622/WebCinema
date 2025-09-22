using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.Models;

namespace WebCinema.ViewModel.Profile
{
    public class MembershipInfoViewModel
    {
        public MemberLevel MemberLevel { get; set; }
        public int CurrentPoints { get; set; }
        public decimal TotalSpending { get; set; }
        public List<PointHistoryViewModel> PointHistory { get; set; }
    }
}