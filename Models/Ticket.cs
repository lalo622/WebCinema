//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebCinema.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ticket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Ticket()
        {
            this.TicketFoods = new HashSet<TicketFood>();
        }
    
        public int TicketID { get; set; }
        public int ScreeningID { get; set; }
        public int UserID { get; set; }
        public string SeatNumber { get; set; }
        public Nullable<System.DateTime> BookingTime { get; set; }
        public decimal TotalPrice { get; set; }
        public Nullable<byte> PaymentStatus { get; set; }
    
        public virtual Screening Screening { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketFood> TicketFoods { get; set; }
    }
}
