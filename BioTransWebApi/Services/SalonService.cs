using BioTrananDomain.DtoModels;
using BioTrananDomain.Models;

namespace BioTransWebApi.Services;

public class SalonService
{
    private readonly MongoDBService<Salon> _mongoDbService;

    public SalonService(MongoDBService<Salon> mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    // Public Methods
    public async Task<Salon> GetSalonFromDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);
        if (!isFound) return null;

        return await _mongoDbService.GetItemAsync(id);
    }

    public async Task<List<Salon>> GetSalonsFromDbAsync()
    {
        return await _mongoDbService.GetItemsAsync();
    }

    public async Task<Salon> AddSalonToDbAsync(SalonDto salonDto)
    {
        Salon salon = DtoToSalon(salonDto);

        await _mongoDbService.CreateItemAsync(salon);

        return salon;
    }

    public async Task<Salon> UpdateSalonInDbAsync(SalonDto salonDto, string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);

        if (!isFound) return null; // Early return

        Salon salon = DtoToSalon(salonDto, id);

        await _mongoDbService.ReplaceItemAsync(salon, id);

        return salon;
    }

    public async Task<bool> DeleteSalonInDbAsync(string id)
    {
        bool isFound = await _mongoDbService.ConfirmItemIdAsync(id);

        if (!isFound) return false; // Early return

        await _mongoDbService.DeleteItemAsync(id);

        return true;
    }

    public bool ErrorHandleSalonPost(SalonDto potentialSalon)
    {
        if (string.IsNullOrWhiteSpace(potentialSalon.SalonName)) return true;
        if (potentialSalon.SalonSeatAmount == 0) return true;

        return false; // all checks are clear
    }

    // Private Methods
    private Salon DtoToSalon(SalonDto salonDto, string id = null!)
    {
        return new Salon
        {
            SalonName = salonDto.SalonName,
            SalonSeatAmount = salonDto.SalonSeatAmount,
            Id = id // mongodb will assign it's new value later, if it's null
        };
    }
}