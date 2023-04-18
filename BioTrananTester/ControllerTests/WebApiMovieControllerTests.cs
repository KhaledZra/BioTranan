using System.Net;
using BioTrananDomain.DtoModels;
using FluentAssertions;
using BioTransWebApi.Controllers;
using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BioTrananTester.ControllerTests;

public class WebApiMovieControllerTests : MongoDbIntegrationTest
{
    private readonly MovieService _movieService;
    private readonly MovieDto _dummyMovieDto;

    public WebApiMovieControllerTests()
    {
        _dummyMovieDto = new MovieDto
        {
            MovieName = "King Kong",
            MovieShowingsLeft = 5,
            MovieEmdeddedTrailerUrl = "link",
            MovieLengthInMinutes = 150
        };
        _movieService = new MovieService(
            mongoDbService: new MongoDBService<Movie>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Movie"));
    }

    // Movie Api Unit tests
    [Fact]
    public async Task POST_api_movie_should_add_movie()
    {
        // Arrange
        MovieController sut = new MovieController(_movieService);
        ActionResult<Movie> result = await sut.AddNewMovie(_dummyMovieDto);
        Movie addedMovie = (result.Result as CreatedAtActionResult).Value as Movie;

        // Act
        var action = await sut.GetMovie(addedMovie?.Id);
        var actionResult = action.Result as OkObjectResult;
        var foundMovie = actionResult?.Value as Movie;

        // Assert
        foundMovie?.Should().BeEquivalentTo(addedMovie);
    }

    [Fact]
    public async Task DELETE_api_movie_should_remove_movie()
    {
        // Arrange
        MovieController sut = new MovieController(_movieService);
        ActionResult<Movie> result = await sut.AddNewMovie(_dummyMovieDto);
        Movie addedMovie = (result.Result as CreatedAtActionResult).Value as Movie;

        // Act
        var action = await sut.DeleteMovie(addedMovie.Id);
        var actionResult = action.Result as NotFoundObjectResult;
        var movieValue = actionResult?.Value as Movie;

        // Assert
        movieValue.Should().BeNull();
    }

    [Fact]
    public async Task GET_api_all_movies_should_return_list()
    {
        // Arrange
        MovieController sut = new MovieController(_movieService);
        await sut.AddNewMovie(_dummyMovieDto);
        await sut.AddNewMovie(_dummyMovieDto);

        // Act
        var action = await sut.GetAllMovies();
        var actionResult = action.Result as OkObjectResult;
        var moviesListValue = actionResult?.Value as List<Movie>;

        // Assert
        moviesListValue.Should().HaveCount(2);
        moviesListValue?[0].MovieName.Should().Be("King Kong");
        moviesListValue?[1].MovieName.Should().Be("King Kong");
    }
}