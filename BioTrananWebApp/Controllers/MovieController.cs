using BioTrananDomain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BioTrananWebApp.Controllers;

public class MovieController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MovieController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    // GET: Movie/GetMovie/{movieId}
    [HttpGet("Movie/GetMovie/{movieId}")]
    public async Task<IActionResult> GetMovie(string movieId)
    {
        var httpClient = _httpClientFactory.CreateClient("WebApi");
        Movie movie;

        try
        {
            movie = await httpClient.GetFromJsonAsync<Movie>($"api/Movie/{movieId}");
        }
        catch (Exception e)
        {
            return View(null);
        }

        return View(movie);
    }
}