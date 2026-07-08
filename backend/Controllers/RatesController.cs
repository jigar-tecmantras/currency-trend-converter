using CurrencyTrendConverter.Models;
using CurrencyTrendConverter.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyTrendConverter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatesController : ControllerBase
{
    private readonly IExchangeRateService _rateService;
    private readonly ILogger<RatesController> _logger;

    public RatesController(IExchangeRateService rateService, ILogger<RatesController> logger)
    {
        _rateService = rateService;
        _logger = logger;
    }

    [HttpGet("currencies")]
    public ActionResult<IEnumerable<string>> GetCurrencies()
    {
        return Ok(_rateService.SupportedCurrencies.OrderBy(x => x));
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestAsync(
        [FromQuery] string baseCurrency = "USD",
        [FromQuery] string targetCurrency = "EUR",
        [FromQuery] decimal amount = 1m)
    {
        try
        {
            var result = await _rateService.GetLatestRateAsync(baseCurrency, targetCurrency, amount);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid currency pair requested: {Base}-{Target}", baseCurrency, targetCurrency);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve latest rate for {Base}-{Target}", baseCurrency, targetCurrency);
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to reach the exchange rate provider.");
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] string baseCurrency = "USD",
        [FromQuery] string targetCurrency = "EUR",
        [FromQuery] int days = 7)
    {
        try
        {
            var result = await _rateService.GetHistoricalRatesAsync(baseCurrency, targetCurrency, days);
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid days window requested: {Days}", days);
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid currency pair requested for history: {Base}-{Target}", baseCurrency, targetCurrency);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve history for {Base}-{Target}", baseCurrency, targetCurrency);
            return StatusCode(StatusCodes.Status502BadGateway, "Failed to reach the exchange rate provider.");
        }
    }
}
