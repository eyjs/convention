using LocalRAG.DTOs.PersonalTrip;

namespace LocalRAG.Interfaces
{
    public interface IPersonalTripService
    {
        // PersonalTrip CRUD
        Task<List<PersonalTripDto>> GetUserTripsAsync(int userId);
        Task<PersonalTripDto?> GetTripByIdAsync(int tripId, int userId);
        Task<PersonalTripDto> CreateTripAsync(CreatePersonalTripDto dto, int userId);
        Task<PersonalTripDto?> UpdateTripAsync(int tripId, UpdatePersonalTripDto dto, int userId);
        Task<bool> DeleteTripAsync(int tripId, int userId);

        // Flight CRUD
        Task<FlightDto> AddFlightAsync(int tripId, CreateFlightDto dto, int userId);
        Task<FlightDto?> UpdateFlightAsync(int flightId, CreateFlightDto dto, int userId);
        Task<bool> DeleteFlightAsync(int flightId, int userId);

        // Accommodation CRUD
        Task<AccommodationDto> AddAccommodationAsync(int tripId, CreateAccommodationDto dto, int userId);
        Task<AccommodationDto?> UpdateAccommodationAsync(int accommodationId, CreateAccommodationDto dto, int userId);
        Task<bool> DeleteAccommodationAsync(int accommodationId, int userId);

        // ItineraryItem CRUD
        Task<List<ItineraryItemDto>> GetItineraryItemsAsync(int tripId, int userId);
        Task<ItineraryItemDto> AddItineraryItemAsync(int tripId, CreateItineraryItemDto dto, int userId);
        Task<ItineraryItemDto?> UpdateItineraryItemAsync(int itemId, UpdateItineraryItemDto dto, int userId);
        Task<bool> DeleteItineraryItemAsync(int itemId, int userId);
        Task<bool> BulkDeleteItineraryItemsAsync(int tripId, List<int> itemIds, int userId);
        Task<bool> BulkUpdateItineraryItemsDayAsync(int tripId, List<UpdateItemDayDto> updates, int userId);
        Task ReorderItineraryItemsAsync(int tripId, List<Controllers.PersonalTrip.ReorderItemDto> items, int userId);

        // Search
        Task<IEnumerable<PersonalTripDto>> SearchTripsByUserNameAsync(string userName);

        // Sharing
        Task<string> GenerateShareTokenAsync(int tripId, int userId);
        Task DisableSharingAsync(int tripId, int userId);
        Task<PersonalTripDto?> GetPublicTripByTokenAsync(string token);
    }
}
