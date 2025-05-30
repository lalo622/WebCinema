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
    
    public partial class Combo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Combo()
        {
            this.ComboFoods = new HashSet<ComboFood>();
            this.TicketFoods = new HashSet<TicketFood>();
        }
    
        public int ComboID { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public decimal SalePrice { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ComboFood> ComboFoods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketFood> TicketFoods { get; set; }
    }
}
