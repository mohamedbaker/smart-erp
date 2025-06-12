using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Smart_ERP.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminEndpoint()
    {
        return Ok("Hello Admin!");
    }

    [HttpGet("user-only")]
    [Authorize(Roles = "User")]
    public IActionResult UserEndpoint()
    {
        return Ok("Hello User!");
    }
}
}