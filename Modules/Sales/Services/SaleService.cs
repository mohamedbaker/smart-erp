
using Microsoft.EntityFrameworkCore;
using Smart_ERP.Data;
using Smart_ERP.Modules.Sales.DTOs;
using Smart_ERP.Modules.Sales.Models;

namespace Smart_ERP.Modules.Sales.Services
{
    public class SaleService
    {
        private readonly ERPDbContext _context;

        public SaleService(ERPDbContext context) => _context = context;

        public async Task<List<Sale>> GetAllAsync() =>
            await _context.Sales.Include(s => s.Product).ToListAsync();

        public async Task<Sale> CreateAsync(CreateSaleDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null || product.Stock < dto.Quantity)
                throw new Exception("Product not available or insufficient stock");

            product.Stock -= dto.Quantity;

            var sale = new Sale
            {
                ProductId = product.Id,
                Quantity = dto.Quantity,
                TotalPrice = product.Price * dto.Quantity,
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return sale;
        }
    }
}
