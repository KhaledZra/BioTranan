using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Services;

public class ReservationService
{
    private readonly MongoDBService<Reservation> _mongoDbService;
    private readonly ShowingService _showingService;

    public ReservationService(MongoDBService<Reservation> mongoDbService, ShowingService showingService)
    {
        _mongoDbService = mongoDbService;
        _showingService = showingService;
    }

    // Public Methods
    public async Task<List<Reservation>> GetAllReservationsFromDbAsync()
    {
        return await _mongoDbService.GetItemsAsync();
    }

    public async Task<List<Reservation>> GetAllReservationsForShowingFromDbAsync(string showingId)
    {
        bool isFound = await _showingService.ConfirmMovieId(showingId);
        if (!isFound) return null!;

        var reservations = await _mongoDbService
            .GetItemsWithCustomFilterAsync("ReservationShowing", showingId, MongoFilters.Equal);

        return reservations;
    }

    public async Task<Reservation> AddReservationToDbAsync(ReservationDto reservationDto)
    {
        Reservation reservation = DtoToReservation(reservationDto);

        reservation.ReservationShowing = await _showingService
            .GetShowingFromDbAsync(reservation.ReservationShowing?.Id!);

        if (reservation.ReservationShowing == null) return null!;

        // Sets new seat value and updates that in Db
        int seatCalculation = reservation.ReservationShowing.SeatsLeft - reservation.AmountOfSeats;
        if (seatCalculation <= -1) return null;
        reservation.ReservationShowing.SeatsLeft = seatCalculation;
        reservation.ReservationShowing = await _showingService.UpdateShowingInDbAsync(reservation.ReservationShowing);

        await _mongoDbService.CreateItemWithExpireDateAsync(reservation);

        return reservation;
    }

    public async Task<bool> DeleteReservationInDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);
        if (!isFound) return false;

        // add back the seats and update it in db
        var reservation = await _mongoDbService.GetItemAsync(id);
        reservation.ReservationShowing!.SeatsLeft += reservation.AmountOfSeats;
        await _showingService.UpdateShowingInDbAsync(reservation.ReservationShowing);

        await _mongoDbService.DeleteItemAsync(id);
        return true;
    }

    public bool ErrorHandleReservationPost(ReservationDto potentialReservation)
    {
        if (string.IsNullOrWhiteSpace(potentialReservation.ReservationShowingId)) return true;
        if (string.IsNullOrWhiteSpace(potentialReservation.Email)) return true;
        if (potentialReservation.AmountOfSeats <= 0) return true;

        return false; // all checks are clear
    }

    // Private Methods
    private Reservation DtoToReservation(ReservationDto reservationDto, string id = null!)
    {
        return new Reservation
        {
            ReservationShowing = new Showing { Id = reservationDto.ReservationShowingId },
            Email = reservationDto.Email,
            AmountOfSeats = reservationDto.AmountOfSeats,
            Id = id // mongodb will assign it's new value later, if it's null
        };
    }
}