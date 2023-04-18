using Microsoft.AspNetCore.Mvc;
using BioTransWebApi.Services;
using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Controllers;

[Controller]
[Route("api/[controller]")]
public class MovieController : Controller
{
    private readonly MovieService _movieService;

    public MovieController(MovieService movieService)
    {
        _movieService = movieService;
    }

    // GET: api/Movie/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(string id)
    {
        Movie movie = await _movieService.GetMovieFromDbAsync(id);

        if (movie == null) return NotFound("No id match");
        return Ok(movie);
    }

    // GET: api/Movie
    [HttpGet]
    public async Task<ActionResult<List<Movie>>> GetAllMovies()
    {
        return Ok(await _movieService.GetMoviesFromDbAsync());
    }

    // POST: api/Movie
    [HttpPost]
    public async Task<ActionResult<Movie>> AddNewMovie([FromBody] MovieDto movieDto)
    {
        if (_movieService.ErrorHandleMoviePost(movieDto)) return BadRequest("Error Message: Null values found");

        Movie movie = await _movieService.AddMovieToDbAsync(movieDto);

        return CreatedAtAction(nameof(GetAllMovies), new { id = movie.Id }, movie);
    }

    // PUT: api/Movie/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<Movie>> UpdateMovie(string id, [FromBody] MovieDto movieDto)
    {
        if (_movieService.ErrorHandleMoviePost(movieDto)) return BadRequest("Error Message: Null values found");

        Movie movie = await _movieService.UpdateMovieInDbAsync(movieDto, id);

        if (movie == null) return NotFound("No match on Id");
        return AcceptedAtAction(nameof(GetAllMovies), new { id = movie.Id }, movie);
    }

    // DELETE: api/Movie/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<string>> DeleteMovie(string id)
    {
        bool isFound = await _movieService.DeleteMovieInDbAsync(id);

        if (!isFound) return NotFound("No match on Id");
        return Ok($"item with id: {id}, was removed!");
    }
}