using Microsoft.AspNetCore.Mvc;
using POS.DTOs.Sales;
using POS.Services.Sales;
using System.ComponentModel.DataAnnotations;

namespace POS.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int take = 20)
    {
        var sales = await _saleService.GetRecentAsync(take);
        return Ok(sales);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            var sale = await _saleService.CheckoutAsync(request);
            return CreatedAtAction(nameof(GetRecent), new { id = sale.Id }, sale);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
