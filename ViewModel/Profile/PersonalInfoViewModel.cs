using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebCinema.Models;

namespace WebCinema.ViewModel.Profile
{
    public class PersonalInfoViewModel
    {
        public Customer Customer { get; set; }
        public List<TicketHistoryViewModel> TicketHistory { get; set; }
        public MembershipInfoViewModel MembershipInfo { get; set; }
    }
}