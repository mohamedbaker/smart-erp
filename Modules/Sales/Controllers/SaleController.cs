using Microsoft.AspNetCore.Mvc;
using Smart_ERP.Modules.Sales.DTOs;
using Smart_ERP.Modules.Sales.Services;

namespace Smart_ERP.Modules.Sales.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SaleController : ControllerBase
    {
        private readonly SaleService _service;

        public SaleController(SaleService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
        {
            try
            {
                var sale = await _service.CreateAsync(dto);
                return Ok(sale);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
