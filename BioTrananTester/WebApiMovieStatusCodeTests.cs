using BioTrananDomain.DtoModels;
using FluentAssertions;
using BioTransWebApi.Controllers;
using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BioTrananTester;

public class WebApiMovieStatusCodeTests : MongoDbIntegrationTest
{
    private readonly MovieService _movieService;

    private readonly MovieDto _dummyMovieDto;

    public WebApiMovieStatusCodeTests()
    {
        _dummyMovieDto = new MovieDto
        {
            MovieName = "King Kong",
            MovieEmdeddedTrailerUrl = "link",
            MovieShowingsLeft = 2,
            MovieLengthInMinutes = 150
        };

        _movieService = new MovieService(
            mongoDbService: new MongoDBService<Movie>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Movie"));
    }

    // Get Tests
    [Fact]
    public async Task GET_api_movie_should_return_code_notFound_on_bad_id()
    {
        // Arrange
        var sut = new MovieController(_movieService);

        // Act
        var actionResult = await sut.GetMovie(String.Empty);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GET_api_movie_should_return_code_ok_on_success()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        var result = await sut.AddNewMovie(_dummyMovieDto);
        Movie actual = (result.Result as CreatedAtActionResult).Value as Movie;

        // Act
        var actionResult = await sut.GetMovie(actual.Id);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GET_api_all_movies_should_return_code_ok()
    {
        // Arrange
        var sut = new MovieController(_movieService);

        // Act
        var actionResult = await sut.GetAllMovies();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }

    // Post Tests
    [Fact]
    public async Task POST_api_movie_should_return_code_badRequest_on_null_values()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = new MovieDto { MovieName = string.Empty };

        // Act
        var actionResult = await sut.AddNewMovie(movieDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task POST_api_movie_should_return_code_createdAt_on_success()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = _dummyMovieDto;

        // Act
        var actionResult = await sut.AddNewMovie(movieDto);

        // Assert
        actionResult.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    // Put Tests
    [Fact]
    public async Task PUT_api_movie_should_return_code_badRequest_on_null_values()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = new MovieDto { MovieName = string.Empty };

        // Act
        var actionResult = await sut.UpdateMovie(string.Empty, movieDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PUT_api_movie_should_return_code_notFound_on_bad_id()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = _dummyMovieDto;

        // Act
        var actionResult = await sut.UpdateMovie(string.Empty, movieDto);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PUT_api_movie_should_return_code_AcceptedAt_on_success()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = _dummyMovieDto;
        var action = await sut.AddNewMovie(movieDto);
        // Poly-fest
        Movie actual = (action.Result as CreatedAtActionResult).Value as Movie;

        // Act
        var actionResult = await sut.UpdateMovie(actual.Id, movieDto);

        // Assert
        actionResult.Result.Should().BeOfType<AcceptedAtActionResult>();
    }

    // Delete Tests
    [Fact]
    public async Task DELETE_api_movie_should_return_code_notFound_on_wrong_id()
    {
        // Arrange
        var sut = new MovieController(_movieService);

        // Act
        var actionResult = await sut.DeleteMovie(string.Empty);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // Delete Tests
    [Fact]
    public async Task DELETE_api_movie_should_return_code_ok_on_success()
    {
        // Arrange
        var sut = new MovieController(_movieService);
        MovieDto movieDto = _dummyMovieDto;
        var action = await sut.AddNewMovie(movieDto);
        // no idea why this is needed for it to work...
        Movie actual = (action.Result as CreatedAtActionResult).Value as Movie;

        // Act
        var actionResult = await sut.DeleteMovie(actual.Id);

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }
}