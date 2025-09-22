using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCinema.ViewModel
{
    public class ComboFoodItemVM
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ComboViewModel
    {
        public int ComboID { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public decimal SalePrice { get; set; }
        public decimal OriginalPrice => ComboFoods.Sum(f => f.Price * f.Quantity);
        public List<ComboFoodItemVM> ComboFoods { get; set; } = new List<ComboFoodItemVM>();
    }
}