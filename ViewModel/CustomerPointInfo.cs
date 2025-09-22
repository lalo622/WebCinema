using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.Models;

namespace WebCinema.ViewModel
{
    public class CustomerPointInfo
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public int CurrentPoints { get; set; }
        public decimal TotalSpending { get; set; }
        public MemberLevel CurrentLevel { get; set; }
        public MemberLevel NextLevel { get; set; }
        public decimal SpendingToNextLevel { get; set; }
    }
}