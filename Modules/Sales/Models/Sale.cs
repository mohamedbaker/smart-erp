using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smart_ERP.Modules.Products.Models;

namespace Smart_ERP.Modules.Sales.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    }
}
