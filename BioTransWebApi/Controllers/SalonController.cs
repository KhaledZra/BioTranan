using Microsoft.AspNetCore.Mvc;
using BioTransWebApi.Services;
using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalonController : Controller
    {
        private readonly SalonService _salonService;

        public SalonController(SalonService salonService)
        {
            _salonService = salonService;
        }

        // GET: api/Salon/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Salon>>> GetSalon(string id)
        {
            Salon salon = await _salonService.GetSalonFromDbAsync(id);

            if (salon == null) return NotFound("No id match");
            return Ok(salon);
        }

        // GET: api/Salon
        [HttpGet]
        public async Task<ActionResult<List<Salon>>> GetAllSalons()
        {
            return Ok(await _salonService.GetSalonsFromDbAsync());
        }

        // POST: api/Salon
        [HttpPost]
        public async Task<ActionResult<Salon>> AddSalon([FromBody] SalonDto salonDto)
        {
            if (_salonService.ErrorHandleSalonPost(salonDto)) return BadRequest("Error Message: Null values found");

            Salon salon = await _salonService.AddSalonToDbAsync(salonDto);

            return CreatedAtAction(nameof(GetAllSalons), new { id = salon.Id }, salon);
        }

        // PUT: api/Salon/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Salon>> UpdateSalon(string id, [FromBody] SalonDto salonDto)
        {
            if (_salonService.ErrorHandleSalonPost(salonDto)) return BadRequest("Error Message: Null values found");

            Salon salon = await _salonService.UpdateSalonInDbAsync(salonDto, id);

            if (salon == null) return NotFound("No match on Id");
            return AcceptedAtAction(nameof(GetAllSalons), new { id = salon.Id }, salon);
        }

        // DELETE: api/Salon/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteSalon(string id)
        {
            bool isFound = await _salonService.DeleteSalonInDbAsync(id);

            if (!isFound) return NotFound("No match on Id");
            return Ok($"item with id: {id}, was removed!");
        }
    }
}