using BioTrananDomain.DtoModels;
using FluentAssertions;
using BioTransWebApi.Controllers;
using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BioTrananTester.ControllerTests;

public class WebApiShowingControllerTests : MongoDbIntegrationTest
{
    private readonly ShowingService _showingService;
    private readonly MovieService _movieService;
    private readonly SalonService _salonService;
    private readonly MovieDto _dummyMovieDto;


    public WebApiShowingControllerTests()
    {
        _dummyMovieDto = new MovieDto
        {
            MovieName = "King Kong",
            MovieEmdeddedTrailerUrl = "link",
            MovieShowingsLeft = 5,
            MovieLengthInMinutes = 187
        };

        _movieService = new MovieService(
            mongoDbService: new MongoDBService<Movie>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Movies"));

        _salonService = new SalonService(
            mongoDbService: new MongoDBService<Salon>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Salons"));

        _showingService = new ShowingService(
            movieService: _movieService,
            salonService: _salonService,
            mongoDbService: new MongoDBService<Showing>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Showings"));
    }

    // Showing Api Unit tests
    [Fact]
    public async Task POST_api_showing_should_add_showing_on_success()
    {
        // Arrange
        ShowingController sut = new ShowingController(_showingService);

        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto
            { SalonName = "Room A", SalonSeatAmount = 50 });

        ActionResult<Showing> result = await sut.AddShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(10),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        Showing addedShowing = (result.Result as CreatedAtActionResult).Value as Showing;

        // Act
        var action = await sut.GetUpcomingShowings();
        var actionResult = action.Result as OkObjectResult;
        var foundShowing = (actionResult?.Value as List<Showing>).FirstOrDefault();

        // Assert
        foundShowing?.Id.Should().Be(addedShowing?.Id);
    }

    [Fact]
    public async Task GET_api_showing_should_get_upcoming_showings()
    {
        // Arrange
        ShowingController sut = new ShowingController(_showingService);

        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto
            { SalonName = "Room A", SalonSeatAmount = 50 });

        for (int i = 1; i < 5; i++)
        {
            await sut.AddShowing(new ShowingDto
            {
                ShowingDateAndTime = DateTime.Now.AddDays(i),
                ShowingMovieId = movie.Id,
                ShowingSalonId = salon.Id
            });
        }

        // Act
        var action = await sut.GetUpcomingShowings();
        var actionResult = action.Result as OkObjectResult;
        var foundShowings = actionResult?.Value as List<Showing>;

        // Assert
        foundShowings?.Should().HaveCount(4);
    }

    [Fact]
    public async Task POST_api_showing_should_give_bad_request_if_movie_showing_limit_is_reached()
    {
        // Arrange
        ShowingController sut = new ShowingController(_showingService);

        _dummyMovieDto.MovieShowingsLeft = 0;
        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto
            { SalonName = "Room A", SalonSeatAmount = 50 });

        // Act
        var actionResult = await sut.AddShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(3),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}