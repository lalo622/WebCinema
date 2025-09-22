using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel.Payment
{
    public class FoodComboVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; } // "food" hoặc "combo"
        public string ImageUrl { get; set; }
    }
}