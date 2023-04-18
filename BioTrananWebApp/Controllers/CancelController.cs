using Microsoft.AspNetCore.Mvc;

namespace BioTrananWebApp.Controllers;

public class CancelController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CancelController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    // GET: Cancel/Reservation
    [HttpGet]
    public IActionResult Reservation()
    {
        return View();
    }

    // POST: Cancel/Reservation
    [HttpPost("Cancel/Reservation")]
    public async Task<IActionResult> CancelResult()
    {
        var httpClient = _httpClientFactory.CreateClient("WebApi");


        if (string.IsNullOrWhiteSpace(Request.Form["Id"])) return View(false);

        try
        {
            var response = await httpClient.DeleteAsync($"/api/reservation/{Request.Form["Id"]}");
            if (!response.IsSuccessStatusCode)
            {
                return View(false);
            }
        }
        catch (Exception e)
        {
            return View(false);
        }

        return View(true);
    }
}