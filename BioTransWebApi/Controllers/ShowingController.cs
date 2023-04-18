using Microsoft.AspNetCore.Mvc;
using BioTransWebApi.Services;
using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowingController : Controller
    {
        private readonly ShowingService _showingService;

        public ShowingController(ShowingService showingService)
        {
            _showingService = showingService;
        }

        // GET: api/Showing/GetUpcomingShowings
        [HttpGet("GetUpcomingShowings")]
        public async Task<ActionResult<List<Showing>>> GetUpcomingShowings()
        {
            return Ok(await _showingService.GetAllUpcomingShowingsFromDbAsync());
        }

        // GET: api/Showing/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Showing>> GetShowing(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound("No match on Id");

            Showing showing = await _showingService.GetShowingFromDbAsync(id);

            if (showing == null) return NotFound("No match on Id");
            return Ok(showing);
        }

        // POST: api/Showing
        [HttpPost]
        public async Task<ActionResult<Showing>> AddShowing([FromBody] ShowingDto showingDto,
            bool isSeatsRestricted = false)
        {
            if (_showingService.ErrorHandleShowingPost(showingDto))
                return BadRequest("Null or bad values found");

            Showing showing = await _showingService.TryToSetupShowing(showingDto, isSeatsRestricted);
            if (showing == null) return BadRequest("Values break against rules!");

            await _showingService.AddShowingToDbAsync(showing);

            return CreatedAtAction(nameof(GetUpcomingShowings), new { id = showing.Id }, showing);
        }

        // // PUT: api/Showing/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Showing>> UpdateShowing(string id, [FromBody] ShowingDto showingDto)
        {
            if (_showingService.ErrorHandleShowingPost(showingDto))
                return BadRequest("Error Message: Null values found");

            Showing updatedShowing = await _showingService.UpdateShowingInDbAsync(showingDto, id);

            if (updatedShowing == null) return NotFound("No match on Id");
            return AcceptedAtAction(nameof(GetUpcomingShowings), new { id = updatedShowing.Id }, updatedShowing);
        }
    }
}