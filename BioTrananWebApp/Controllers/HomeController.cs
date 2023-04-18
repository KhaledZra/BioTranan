using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BioTrananDomain.Models;
using BioTrananWebApp.Models;

namespace BioTrananWebApp.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: Home/Index
    public async Task<IActionResult> Index()
    {
        HttpClient httpClient;
        List<Showing> showings;

        try
        {
            httpClient = _httpClientFactory.CreateClient("WebApi");
            showings = await httpClient.GetFromJsonAsync<List<Showing>>("api/Showing/GetUpcomingShowings");
        }
        catch (Exception e)
        {
            showings = new List<Showing>();
        }

        try
        {
            httpClient = _httpClientFactory.CreateClient("ChuckApi");
            ChuckJoke chuckJoke = await httpClient.GetFromJsonAsync<ChuckJoke>("jokes/random");
            ViewBag.Joke = chuckJoke.Value;
        }
        catch (Exception e)
        {
            ViewBag.Joke = "No Chuck Jokes today :(";
        }

        return View(showings);
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}