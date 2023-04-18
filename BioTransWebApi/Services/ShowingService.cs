using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;
using MongoDB.Bson;

namespace BioTransWebApi.Services;

public class ShowingService
{
    private readonly MongoDBService<Showing> _mongoDbService;
    private readonly MovieService _movieService;
    private readonly SalonService _salonService;

    public ShowingService(MongoDBService<Showing> mongoDbService, MovieService movieService, SalonService salonService)
    {
        _mongoDbService = mongoDbService;
        _movieService = movieService;
        _salonService = salonService;
    }

    // Service methods that use Db service
    public async Task<Showing> GetShowingFromDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);
        if (!isFound) return null!;

        return await _mongoDbService.GetItemAsync(id);
    }

    public async Task<List<Showing>> GetAllUpcomingShowingsFromDbAsync()
    {
        // Adding two hours to match utc with gmt+2 for mongodb
        var upcomingShowings = await _mongoDbService
            .GetItemsWithCustomFilterAsync("ShowingDateAndTime",
                new BsonDateTime(DateTime.Now.AddHours(2)), MongoFilters.GreaterThan);

        return upcomingShowings;
    }

    public async Task<Showing>
        TryToSetupShowing(ShowingDto showingDto, bool isRestrictMode = false)
    {
        bool isFailed = false;
        Showing showing = DtoToShowing(showingDto);

        isFailed = await NullChecker(showing);
        if (isFailed) return null!;

        if (DateCrashChecker(await _mongoDbService.GetItemsAsync(), showing)) return null!;

        // Restriction Handler - Sets up seats left
        if (isRestrictMode) showing.SeatsLeft = showing.ShowingSalon!.SalonSeatAmount / 2;
        else showing.SeatsLeft = showing.ShowingSalon!.SalonSeatAmount;

        // Updates Showings left
        if (showing.ShowingMovie!.MovieShowingsLeft < 1) return null!;
        showing.ShowingMovie.MovieShowingsLeft--;
        await _movieService.UpdateMovieInDbAsync(showing.ShowingMovie);

        return showing;
    }

    public async Task AddShowingToDbAsync(Showing showing) =>
        await _mongoDbService.CreateItemAsync(showing);

    public async Task<Showing> UpdateShowingInDbAsync(ShowingDto showingDto, string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);

        if (!isFound) return null; // Early return

        Showing showing = DtoToShowing(showingDto, id);

        await _mongoDbService.ReplaceItemAsync(showing, id);

        return showing;
    }

    public async Task<Showing> UpdateShowingInDbAsync(Showing showing)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(showing.Id);

        if (!isFound) return null; // Early return

        await _mongoDbService.ReplaceItemAsync(showing, showing.Id);

        return showing;
    }

    public async Task<bool> ConfirmMovieId(string id) =>
        await _mongoDbService.ConfirmItemIdAsync(id);

    public bool ErrorHandleShowingPost(ShowingDto potentialShowing)
    {
        if (!DateTime.TryParse(potentialShowing.ShowingDateAndTime.ToString(), out DateTime _)) return true;
        if (potentialShowing.ShowingDateAndTime < DateTime.Now) return true;
        if (string.IsNullOrWhiteSpace(potentialShowing.ShowingMovieId)) return true;
        if (string.IsNullOrWhiteSpace(potentialShowing.ShowingSalonId)) return true;

        return false; // all checks are clear
    }

    // Private Methods
    private Showing DtoToShowing(ShowingDto showingDto, string id = null!)
    {
        return new Showing
        {
            ShowingDateAndTime = showingDto.ShowingDateAndTime,
            ShowingMovie = new Movie { Id = showingDto.ShowingMovieId },
            ShowingSalon = new Salon { Id = showingDto.ShowingSalonId },
            Id = id // mongodb will assign it's new value later, if it's null
        };
    }

    private async Task<bool> NullChecker(Showing showing)
    {
        showing.ShowingMovie = await _movieService.GetMovieFromDbAsync(showing.ShowingMovie?.Id!);
        if (showing.ShowingMovie == null) return true;

        showing.ShowingSalon = await _salonService.GetSalonFromDbAsync(showing.ShowingSalon?.Id!);
        if (showing.ShowingSalon == null) return true;

        return false;
    }

    private bool DateCrashChecker(List<Showing> showings, Showing potentialShowing)
    {
        bool isShowingTimeAccepted = false;
        int cleaningTime = 15;

        showings.ForEach(b =>
        {
            // (dateB < dateA && dateA < dateC) Algorithm
            // This checks if the movie start time crashes with another show
            if (potentialShowing.ShowingDateAndTime >= b.ShowingDateAndTime)
            {
                if (potentialShowing.ShowingDateAndTime <= b.ShowingDateAndTime.AddMinutes(
                        b.ShowingMovie.MovieLengthInMinutes + cleaningTime))
                {
                    isShowingTimeAccepted = true;
                }
            }

            // This checks if the movie run time crashes with another show
            if (potentialShowing.ShowingDateAndTime.AddMinutes(potentialShowing.ShowingMovie.MovieLengthInMinutes +
                                                               cleaningTime)
                >= b.ShowingDateAndTime)
            {
                if (potentialShowing.ShowingDateAndTime.AddMinutes(potentialShowing.ShowingMovie.MovieLengthInMinutes +
                                                                   cleaningTime)
                    <= b.ShowingDateAndTime.AddMinutes(b.ShowingMovie.MovieLengthInMinutes + cleaningTime))
                {
                    isShowingTimeAccepted = true;
                }
            }
        });

        return isShowingTimeAccepted;
    }
}