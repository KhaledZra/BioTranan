using System.Net.Http.Headers;
using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BioTrananWebApp.Controllers;

public class BookingController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BookingController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    // GET: Booking/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(string id)
    {
        var httpClient = _httpClientFactory.CreateClient("WebApi");
        Showing showing;

        try
        {
            showing = await httpClient.GetFromJsonAsync<Showing>($"api/Showing/{id}");
        }
        catch (Exception e)
        {
            return View(null);
        }


        return View(showing);
    }

    // POST: Booking/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> PostBooking(string id)
    {
        var httpClient = _httpClientFactory.CreateClient("WebApi");

        if (string.IsNullOrWhiteSpace(Request.Form["Email"])) return View(null);
        if (!int.TryParse(Request.Form["Seats"], out int seats)) return View(null);

        HttpResponseMessage result;
        Reservation reservation;
        ReservationDto reservationDto = new ReservationDto
        {
            ReservationShowingId = id,
            Email = Request.Form["Email"]!,
            AmountOfSeats = seats,
        };

        var serializedData = JsonSerializer.Serialize(reservationDto);
        var buffer = System.Text.Encoding.UTF8.GetBytes(serializedData);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        try
        {
            result = await httpClient.PostAsync("/api/Reservation", byteContent);
            reservation = await result.Content.ReadFromJsonAsync<Reservation>();
        }
        catch (Exception e)
        {
            return View(null);
        }

        return View(reservation);
    }
}