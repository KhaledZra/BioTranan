using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Services;

public class MovieService
{
    private readonly MongoDBService<Movie> _mongoDbService;

    public MovieService(MongoDBService<Movie> mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    // Public Methods
    public async Task<Movie> GetMovieFromDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);
        if (!isFound) return null;

        return await _mongoDbService.GetItemAsync(id);
    }

    public async Task<List<Movie>> GetMoviesFromDbAsync()
    {
        return await _mongoDbService.GetItemsAsync();
    }

    public async Task<Movie> AddMovieToDbAsync(MovieDto movieDto)
    {
        Movie movie = DtoToMovie(movieDto);

        await _mongoDbService.CreateItemAsync(movie);

        return movie;
    }

    public async Task<Movie> UpdateMovieInDbAsync(MovieDto movieDto, string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);

        if (!isFound) return null; // Early return

        Movie movie = DtoToMovie(movieDto, id);

        await _mongoDbService.ReplaceItemAsync(movie, id);

        return movie;
    }

    public async Task<Movie> UpdateMovieInDbAsync(Movie movie)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(movie.Id);

        if (!isFound) return null; // Early return

        await _mongoDbService.ReplaceItemAsync(movie, movie.Id);

        return movie;
    }

    public async Task<bool> DeleteMovieInDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);

        if (!isFound) return false; // Early return

        await _mongoDbService.DeleteItemAsync(id);

        return true;
    }

    public bool ErrorHandleMoviePost(MovieDto potentialMovie)
    {
        if (string.IsNullOrWhiteSpace(potentialMovie.MovieName)) return true;
        if (string.IsNullOrWhiteSpace(potentialMovie.MovieEmdeddedTrailerUrl)) return true;
        if (potentialMovie.MovieShowingsLeft <= 0) return true;
        if (potentialMovie.MovieLengthInMinutes <= 0) return true;

        return false; // all checks are clear
    }

    // Private Methods
    private Movie DtoToMovie(MovieDto movieDto, string id = null!)
    {
        return new Movie
        {
            MovieName = movieDto.MovieName,
            MovieShowingsLeft = movieDto.MovieShowingsLeft,
            MovieEmbeddedTrailerUrl = movieDto.MovieEmdeddedTrailerUrl,
            MovieLengthInMinutes = movieDto.MovieLengthInMinutes,
            Id = id // mongodb will assign it's new value later, if it's null
        };
    }
}