using System.Collections.Generic;

namespace AspnetRunBasics.Models
{
    public class BasketModel
    {
        public string UserName { get; set; }
        public List<BasketModelItem> Items { get; set; } = new List<BasketModelItem>();

        public decimal TotalPrice { get; set; }
    }
}
