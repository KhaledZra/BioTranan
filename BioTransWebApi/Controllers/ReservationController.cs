using Microsoft.AspNetCore.Mvc;
using BioTransWebApi.Services;
using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : Controller
    {
        private readonly ReservationService _reservationService;

        public ReservationController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<List<Reservation>>> GetAllReservations()
        {
            return Ok(await _reservationService.GetAllReservationsFromDbAsync());
        }

        // GET: api/Reservation/ShowingFilter
        [HttpGet("ShowingFilter/{showingId}")]
        public async Task<ActionResult<List<Reservation>>> GetReservationsWithShowingFilter(string showingId)
        {
            var reservations = await _reservationService.GetAllReservationsForShowingFromDbAsync(showingId);

            if (reservations == null!) return NotFound();
            return Ok(reservations);
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> AddReservation([FromBody] ReservationDto reservationDto)
        {
            if (_reservationService.ErrorHandleReservationPost(reservationDto))
                return BadRequest("Bad values");

            var reservation = await _reservationService.AddReservationToDbAsync(reservationDto);

            if (reservation == null!) return BadRequest("No match on Id or seats full");
            return CreatedAtAction(nameof(GetAllReservations), new { id = reservation.Id }, reservation);
        }

        // DELETE: api/Reservation/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteReservation(string id)
        {
            bool isFound = await _reservationService.DeleteReservationInDbAsync(id);

            if (!isFound) return NotFound("No match on Id");
            return Ok($"item with id: {id}, was removed!");
        }
    }
}