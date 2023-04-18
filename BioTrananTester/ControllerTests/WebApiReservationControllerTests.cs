using BioTrananDomain.DtoModels;
using FluentAssertions;
using BioTransWebApi.Controllers;
using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BioTrananTester.ControllerTests;

public class WebApiReservationControllerTests : MongoDbIntegrationTest
{
    private readonly ReservationService _reservationService;
    private readonly ShowingService _showingService;
    private readonly MovieService _movieService;
    private readonly SalonService _salonService;
    private readonly MovieDto _dummyMovieDto;

    public WebApiReservationControllerTests()
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

        _reservationService = new ReservationService(
            mongoDbService: new MongoDBService<Reservation>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Reservations"),
            showingService: _showingService);
    }

    // Rservation Api Unit tests
    [Fact]
    public async Task POST_api_reservation_should_add_reservation()
    {
        // Arrange
        ReservationController sut = new ReservationController(_reservationService);
        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto { SalonName = "Room A", SalonSeatAmount = 10 });
        var showing = await _showingService.TryToSetupShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(2),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        await _showingService.AddShowingToDbAsync(showing);

        ActionResult<Reservation> result = await sut.AddReservation(new ReservationDto
        {
            ReservationShowingId = showing.Id,
            Email = "test@gmail.com",
            AmountOfSeats = 1
        });
        Reservation addedReservation = (result.Result as CreatedAtActionResult).Value as Reservation;

        // Act
        var action = await sut.GetAllReservations();
        var actionResult = action.Result as OkObjectResult;
        var foundReservation = (actionResult?.Value as List<Reservation>).FirstOrDefault();

        // Assert
        foundReservation.Should().BeEquivalentTo(addedReservation);
    }

    // Rservation Api Unit tests
    [Fact]
    public async Task GET_api_reservation_should_get_reservations()
    {
        // Arrange
        ReservationController sut = new ReservationController(_reservationService);
        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto { SalonName = "Room A", SalonSeatAmount = 10 });
        var showing = await _showingService.TryToSetupShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(2),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        await _showingService.AddShowingToDbAsync(showing);


        for (int i = 1; i < 6; i++)
        {
            await sut.AddReservation(new ReservationDto
            {
                ReservationShowingId = showing.Id,
                Email = "test@gmail.com",
                AmountOfSeats = 1
            });
        }

        // Act
        var action = await sut.GetAllReservations();
        var actionResult = action.Result as OkObjectResult;
        var foundReservation = actionResult?.Value as List<Reservation>;

        // Assert
        foundReservation.Should().HaveCount(5);
        foundReservation?.ForEach(r => r.ReservationShowing.Id.Should().BeEquivalentTo(showing.Id));
    }

    // Rservation Api Unit tests
    [Fact]
    public async Task GET_api_reservation_should_get_reservations_with_showing_filter()
    {
        // Arrange
        ReservationController sut = new ReservationController(_reservationService);
        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto { SalonName = "Room A", SalonSeatAmount = 10 });
        var showing = await _showingService.TryToSetupShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(2),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        await _showingService.AddShowingToDbAsync(showing);
        var showing2 = await _showingService.TryToSetupShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(5),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        await _showingService.AddShowingToDbAsync(showing2);

        for (int i = 1; i < 6; i++)
        {
            await sut.AddReservation(new ReservationDto
            {
                ReservationShowingId = showing.Id,
                Email = "test@gmail.com",
                AmountOfSeats = 1
            });
        }

        // Act
        var action = await sut.GetReservationsWithShowingFilter(showing2.Id);
        var actionResult = action.Result as OkObjectResult;
        var foundReservation = actionResult?.Value as List<Reservation>;

        // Assert
        foundReservation.Should().HaveCount(0);
    }

    [Fact]
    public async Task POST_api_reservation_should_return_bad_request_when_seats_are_sold_out()
    {
        // Arrange
        ReservationController sut = new ReservationController(_reservationService);
        var movie = await _movieService.AddMovieToDbAsync(_dummyMovieDto);
        var salon = await _salonService.AddSalonToDbAsync(new SalonDto { SalonName = "Room A", SalonSeatAmount = 0 });
        var showing = await _showingService.TryToSetupShowing(new ShowingDto
        {
            ShowingDateAndTime = DateTime.Now.AddDays(2),
            ShowingMovieId = movie.Id,
            ShowingSalonId = salon.Id
        });
        await _showingService.AddShowingToDbAsync(showing);

        // Act
        var actionResult = await sut.AddReservation(new ReservationDto
        {
            ReservationShowingId = showing.Id,
            Email = "test@gmail.com",
            AmountOfSeats = 1
        });

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}