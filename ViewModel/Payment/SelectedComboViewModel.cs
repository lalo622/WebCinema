using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Payment
{
    public class SelectedComboViewModel
    {
        public int ComboID { get; set; }
        public string Name { get; set; }
        public decimal SalePrice { get; set; }
        public int Quantity { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
    }
}   