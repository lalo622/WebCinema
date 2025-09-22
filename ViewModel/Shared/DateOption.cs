using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class DateOption
    {
        public DateTime Date { get; set; }
        public string DisplayText { get; set; }
        public string DayOfWeek { get; set; }
        public bool IsToday { get; set; }
        public bool IsSelected { get; set; }
    }
}