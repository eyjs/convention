using LocalRAG.Data;
using LocalRAG.DTOs.PersonalTrip;
using LocalRAG.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalRAG.Services.PersonalTrip
{
    public class PersonalTripService : IPersonalTripService
    {
        private readonly ConventionDbContext _context;

        public PersonalTripService(ConventionDbContext context)
        {
            _context = context;
        }

        #region PersonalTrip CRUD

        public async Task<List<PersonalTripDto>> GetUserTripsAsync(int userId)
        {
            var trips = await _context.PersonalTrips
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .Include(t => t.ItineraryItems)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return trips.Select(MapToPersonalTripDto).ToList();
        }

        public async Task<PersonalTripDto?> GetTripByIdAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips
                .AsNoTracking()
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .Include(t => t.ItineraryItems)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            return trip == null ? null : MapToPersonalTripDto(trip);
        }

        public async Task<PersonalTripDto> CreateTripAsync(CreatePersonalTripDto dto, int userId)
        {
            if (!DateOnly.TryParseExact(dto.StartDate, "yyyy-MM-dd", out var startDate) || !DateOnly.TryParseExact(dto.EndDate, "yyyy-MM-dd", out var endDate))
                throw new ArgumentException("날짜 형식이 올바르지 않습니다. (yyyy-MM-dd)");

            if (endDate < startDate)
                throw new ArgumentException("종료일은 시작일보다 이후여야 합니다.");

            var trip = new Entities.PersonalTrip.PersonalTrip
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = startDate,
                EndDate = endDate,
                Destination = dto.Destination,
                City = dto.City,
                CountryCode = dto.CountryCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PersonalTrips.Add(trip);
            await _context.SaveChangesAsync();

            return MapToPersonalTripDto(trip);
        }

        public async Task<PersonalTripDto?> UpdateTripAsync(int tripId, UpdatePersonalTripDto dto, int userId)
        {
            var trip = await _context.PersonalTrips
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .Include(t => t.ItineraryItems)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null) return null;

            if (!DateOnly.TryParseExact(dto.StartDate, "yyyy-MM-dd", out var startDate) || !DateOnly.TryParseExact(dto.EndDate, "yyyy-MM-dd", out var endDate))
                throw new ArgumentException("날짜 형식이 올바르지 않습니다. (yyyy-MM-dd)");

            if (endDate < startDate)
                throw new ArgumentException("종료일은 시작일보다 이후여야 합니다.");

            trip.Title = dto.Title;
            trip.Description = dto.Description;
            trip.StartDate = startDate;
            trip.EndDate = endDate;
            trip.Destination = dto.Destination;
            trip.City = dto.City;
            trip.CountryCode = dto.CountryCode;
            trip.Latitude = dto.Latitude;
            trip.Longitude = dto.Longitude;
            trip.CoverImageUrl = dto.CoverImageUrl;
            trip.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPersonalTripDto(trip);
        }

        public async Task<bool> DeleteTripAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) return false;

            _context.PersonalTrips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Flight CRUD

        public async Task<FlightDto> AddFlightAsync(int tripId, CreateFlightDto dto, int userId)
        {
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없습니다.");

            var flight = new Entities.PersonalTrip.Flight
            {
                PersonalTripId = tripId,
                Category = dto.Category,
                ItineraryItemId = dto.ItineraryItemId,
                Amount = dto.Amount,
                TollFee = dto.TollFee,
                FuelCost = dto.FuelCost,
                ParkingFee = dto.ParkingFee,
                RentalCost = dto.RentalCost,
                Airline = dto.Airline,
                FlightNumber = dto.FlightNumber,
                DepartureLocation = dto.DepartureLocation,
                ArrivalLocation = dto.ArrivalLocation,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                BookingReference = dto.BookingReference,
                SeatNumber = dto.SeatNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            return MapToFlightDto(flight);
        }

        public async Task<FlightDto?> UpdateFlightAsync(int flightId, CreateFlightDto dto, int userId)
        {
            var flight = await _context.Flights.Include(f => f.PersonalTrip).FirstOrDefaultAsync(f => f.Id == flightId && f.PersonalTrip.UserId == userId);
            if (flight == null) return null;

            flight.Category = dto.Category;
            flight.ItineraryItemId = dto.ItineraryItemId;
            flight.Amount = dto.Amount;
            flight.TollFee = dto.TollFee;
            flight.FuelCost = dto.FuelCost;
            flight.ParkingFee = dto.ParkingFee;
            flight.RentalCost = dto.RentalCost;
            flight.Airline = dto.Airline;
            flight.FlightNumber = dto.FlightNumber;
            flight.DepartureLocation = dto.DepartureLocation;
            flight.ArrivalLocation = dto.ArrivalLocation;
            flight.DepartureTime = dto.DepartureTime;
            flight.ArrivalTime = dto.ArrivalTime;
            flight.BookingReference = dto.BookingReference;
            flight.SeatNumber = dto.SeatNumber;
            flight.Notes = dto.Notes;
            flight.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToFlightDto(flight);
        }

        public async Task<bool> DeleteFlightAsync(int flightId, int userId)
        {
            var flight = await _context.Flights.Include(f => f.PersonalTrip).FirstOrDefaultAsync(f => f.Id == flightId && f.PersonalTrip.UserId == userId);
            if (flight == null) return false;

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Accommodation CRUD

        public async Task<AccommodationDto> AddAccommodationAsync(int tripId, CreateAccommodationDto dto, int userId)
        {
            var trip = await _context.PersonalTrips
                .Include(t => t.ItineraryItems) // Include ItineraryItems for auto-generation logic
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없습니다.");

            var accommodation = new Entities.PersonalTrip.Accommodation
            {
                PersonalTripId = tripId,
                Name = dto.Name,
                Type = dto.Type,
                Address = dto.Address,
                PostalCode = dto.PostalCode,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                BookingReference = dto.BookingReference,
                ContactNumber = dto.ContactNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync(); // Save accommodation first to get its ID

            // Auto-generate itinerary items for check-in/out
            await AddOrUpdateAutoGeneratedItineraryItems(trip, accommodation);

            return MapToAccommodationDto(accommodation);
        }

        public async Task<AccommodationDto?> UpdateAccommodationAsync(int accommodationId, CreateAccommodationDto dto, int userId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.PersonalTrip)
                .ThenInclude(t => t.ItineraryItems) // Include ItineraryItems for auto-generation logic
                .FirstOrDefaultAsync(a => a.Id == accommodationId && a.PersonalTrip.UserId == userId);
            if (accommodation == null) return null;

            // Store old check-in/out times to compare
            var oldCheckInTime = accommodation.CheckInTime;
            var oldCheckOutTime = accommodation.CheckOutTime;

            accommodation.Name = dto.Name;
            accommodation.Type = dto.Type;
            accommodation.Address = dto.Address;
            accommodation.PostalCode = dto.PostalCode;
            accommodation.Latitude = dto.Latitude;
            accommodation.Longitude = dto.Longitude;
            accommodation.CheckInTime = dto.CheckInTime;
            accommodation.CheckOutTime = dto.CheckOutTime;
            accommodation.BookingReference = dto.BookingReference;
            accommodation.ContactNumber = dto.ContactNumber;
            accommodation.Notes = dto.Notes;
            accommodation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update auto-generated itinerary items for check-in/out if times changed
            if (oldCheckInTime != accommodation.CheckInTime || oldCheckOutTime != accommodation.CheckOutTime)
            {
                await AddOrUpdateAutoGeneratedItineraryItems(accommodation.PersonalTrip, accommodation);
            }

            return MapToAccommodationDto(accommodation);
        }

        public async Task<bool> DeleteAccommodationAsync(int accommodationId, int userId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.PersonalTrip)
                .ThenInclude(t => t.ItineraryItems) // Include ItineraryItems to delete auto-generated ones
                .FirstOrDefaultAsync(a => a.Id == accommodationId && a.PersonalTrip.UserId == userId);
            if (accommodation == null) return false;

            // Delete associated auto-generated itinerary items
            var autoGeneratedItems = accommodation.PersonalTrip.ItineraryItems
                .Where(i => i.IsAutoGenerated && i.GooglePlaceId == $"accommodation-{accommodation.Id}") // Using GooglePlaceId to link
                .ToList();
            _context.ItineraryItems.RemoveRange(autoGeneratedItems);

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region ItineraryItem CRUD

        public async Task<List<ItineraryItemDto>> GetItineraryItemsAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없거나 접근 권한이 없습니다.");

            var items = await _context.ItineraryItems
                .AsNoTracking()
                .Where(i => i.PersonalTripId == tripId)
                .OrderBy(i => i.DayNumber)
                .ThenBy(i => i.OrderNum)
                .ThenBy(i => i.StartTime)
                .ToListAsync();

            return items.Select(MapToItineraryItemDto).ToList();
        }

        public async Task<ItineraryItemDto> AddItineraryItemAsync(int tripId, CreateItineraryItemDto dto, int userId)
        {
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없거나 접근 권한이 없습니다.");

            var newItem = new Entities.PersonalTrip.ItineraryItem
            {
                PersonalTripId = tripId,
                DayNumber = dto.DayNumber,
                LocationName = dto.LocationName,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                GooglePlaceId = dto.GooglePlaceId,
                StartTime = dto.StartTime != null && TimeOnly.TryParse(dto.StartTime, out var st) ? st : null,
                EndTime = dto.EndTime != null && TimeOnly.TryParse(dto.EndTime, out var et) ? et : null,
                Notes = dto.Notes,
                Category = dto.Category,
                PhoneNumber = dto.PhoneNumber,
                KakaoPlaceUrl = dto.KakaoPlaceUrl,
                ExpenseAmount = dto.ExpenseAmount,
                IsAutoGenerated = false // User-added item
            };

            _context.ItineraryItems.Add(newItem);
            await _context.SaveChangesAsync();
            return MapToItineraryItemDto(newItem);
        }

        public async Task<ItineraryItemDto?> UpdateItineraryItemAsync(int itemId, UpdateItineraryItemDto dto, int userId)
        {
            var item = await _context.ItineraryItems.Include(i => i.PersonalTrip).FirstOrDefaultAsync(i => i.Id == itemId && i.PersonalTrip.UserId == userId);
            if (item == null) return null;

            // Prevent updating auto-generated items directly through this endpoint
            if (item.IsAutoGenerated)
            {
                throw new ArgumentException("자동 생성된 일정 항목은 직접 수정할 수 없습니다.");
            }

            item.DayNumber = dto.DayNumber;
            item.LocationName = dto.LocationName;
            item.Address = dto.Address;
            item.Latitude = dto.Latitude;
            item.Longitude = dto.Longitude;
            item.GooglePlaceId = dto.GooglePlaceId;
            item.StartTime = dto.StartTime != null && TimeOnly.TryParse(dto.StartTime, out var st) ? st : null;
            item.EndTime = dto.EndTime != null && TimeOnly.TryParse(dto.EndTime, out var et) ? et : null;
            item.Notes = dto.Notes;
            item.Category = dto.Category;
            item.PhoneNumber = dto.PhoneNumber;
            item.KakaoPlaceUrl = dto.KakaoPlaceUrl;
            item.ExpenseAmount = dto.ExpenseAmount;

            await _context.SaveChangesAsync();
            return MapToItineraryItemDto(item);
        }

        public async Task<bool> DeleteItineraryItemAsync(int itemId, int userId)
        {
            var item = await _context.ItineraryItems.Include(i => i.PersonalTrip).FirstOrDefaultAsync(i => i.Id == itemId && i.PersonalTrip.UserId == userId);
            if (item == null) return false;

            // Prevent deleting auto-generated items directly through this endpoint
            if (item.IsAutoGenerated)
            {
                throw new ArgumentException("자동 생성된 일정 항목은 직접 삭제할 수 없습니다.");
            }

            _context.ItineraryItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteItineraryItemsAsync(int tripId, List<int> itemIds, int userId)
        {
            var trip = await _context.PersonalTrips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없거나 접근 권한이 없습니다.");

            var itemsToDelete = await _context.ItineraryItems
                .Where(i => i.PersonalTripId == tripId && itemIds.Contains(i.Id))
                .ToListAsync();

            if (itemsToDelete.Count != itemIds.Count)
            {
                throw new ArgumentException("제공된 일부 일정 항목 ID가 유효하지 않거나 해당 여행에 속하지 않습니다.");
            }

            // Prevent deleting auto-generated items directly through this endpoint
            if (itemsToDelete.Any(i => i.IsAutoGenerated))
            {
                throw new ArgumentException("자동 생성된 일정 항목은 대량 삭제할 수 없습니다.");
            }

            _context.ItineraryItems.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkUpdateItineraryItemsDayAsync(int tripId, List<UpdateItemDayDto> updates, int userId)
        {
            var trip = await _context.PersonalTrips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) throw new ArgumentException("여행을 찾을 수 없거나 접근 권한이 없습니다.");

            var itemIdsToUpdate = updates.Select(u => u.ItemId).ToList();
            var items = await _context.ItineraryItems
                .Where(i => i.PersonalTripId == tripId && itemIdsToUpdate.Contains(i.Id))
                .ToListAsync();

            if (items.Count != itemIdsToUpdate.Count)
            {
                throw new ArgumentException("제공된 일부 일정 항목 ID가 유효하지 않거나 해당 여행에 속하지 않습니다.");
            }

            // Prevent updating auto-generated items directly through this endpoint
            if (items.Any(i => i.IsAutoGenerated))
            {
                throw new ArgumentException("자동 생성된 일정 항목은 대량 수정할 수 없습니다.");
            }

            foreach (var item in items)
            {
                var update = updates.FirstOrDefault(u => u.ItemId == item.Id);
                if (update != null)
                {
                    item.DayNumber = update.NewDayNumber;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ReorderItineraryItemsAsync(int tripId, List<Controllers.PersonalTrip.ReorderItemDto> items, int userId)
        {
            // 여행이 사용자의 것인지 확인
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null)
                throw new ArgumentException("여행을 찾을 수 없습니다.");

            // 각 항목의 순서 업데이트
            foreach (var itemDto in items)
            {
                var item = await _context.ItineraryItems
                    .Include(i => i.PersonalTrip)
                    .FirstOrDefaultAsync(i => i.Id == itemDto.Id && i.PersonalTrip.UserId == userId);

                if (item != null)
                {
                    // Prevent reordering auto-generated items directly through this endpoint
                    if (item.IsAutoGenerated)
                    {
                        throw new ArgumentException("자동 생성된 일정 항목은 순서를 직접 변경할 수 없습니다.");
                    }
                    item.OrderNum = itemDto.OrderNum;
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Search

        public async Task<IEnumerable<PersonalTripDto>> SearchTripsByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new List<PersonalTripDto>();
            }

            var lowerCaseUserName = userName.ToLower();

            // Find users whose first name, last name, or full name contains the search term
            var userIds = await _context.Users
                .AsNoTracking()
                .Where(u => (u.FirstName != null && u.FirstName.ToLower().Contains(lowerCaseUserName)) ||
                            (u.LastName != null && u.LastName.ToLower().Contains(lowerCaseUserName)) ||
                            (u.Name != null && u.Name.ToLower().Contains(lowerCaseUserName)))
                .Select(u => u.Id)
                .ToListAsync();

            if (!userIds.Any())
            {
                return new List<PersonalTripDto>();
            }

            // Get all personal trips for the found users
            var trips = await _context.PersonalTrips
                .AsNoTracking()
                .Where(t => userIds.Contains(t.UserId))
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .Include(t => t.ItineraryItems)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return trips.Select(MapToPersonalTripDto).ToList();
        }

        #endregion

        #region Sharing

        public async Task<string> GenerateShareTokenAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null)
                throw new ArgumentException("여행을 찾을 수 없거나 권한이 없습니다.");

            if (string.IsNullOrEmpty(trip.ShareToken))
            {
                trip.ShareToken = Guid.NewGuid().ToString();
            }

            trip.IsShared = true;
            trip.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return trip.ShareToken;
        }

        public async Task DisableSharingAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null)
                throw new ArgumentException("여행을 찾을 수 없거나 권한이 없습니다.");

            trip.IsShared = false;
            trip.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<PersonalTripDto?> GetPublicTripByTokenAsync(string token)
        {
            var trip = await _context.PersonalTrips
                .AsNoTracking()
                .Where(t => t.ShareToken == token && t.IsShared)
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .Include(t => t.ItineraryItems)
                .FirstOrDefaultAsync();

            return trip == null ? null : MapToPersonalTripDto(trip);
        }

        #endregion

        #region Mappers

        private PersonalTripDto MapToPersonalTripDto(Entities.PersonalTrip.PersonalTrip trip)
        {
            return new PersonalTripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                StartDate = trip.StartDate.ToString("yyyy-MM-dd"),
                EndDate = trip.EndDate.ToString("yyyy-MM-dd"),
                Destination = trip.Destination,
                City = trip.City,
                CountryCode = trip.CountryCode,
                Latitude = trip.Latitude,
                Longitude = trip.Longitude,
                CoverImageUrl = trip.CoverImageUrl,
                UserId = trip.UserId,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                IsShared = trip.IsShared,
                ShareToken = trip.ShareToken,
                Flights = trip.Flights?.Select(MapToFlightDto).OrderBy(f => f.DepartureTime).ToList() ?? new List<FlightDto>(),
                Accommodations = trip.Accommodations?.Select(MapToAccommodationDto).OrderBy(a => a.CheckInTime).ToList() ?? new List<AccommodationDto>(),
                ItineraryItems = trip.ItineraryItems?.Select(MapToItineraryItemDto).OrderBy(i => i.DayNumber).ThenBy(i => i.StartTime).ToList() ?? new List<ItineraryItemDto>()
            };
        }

        private FlightDto MapToFlightDto(Entities.PersonalTrip.Flight flight)
        {
            return new FlightDto
            {
                Id = flight.Id,
                PersonalTripId = flight.PersonalTripId,
                Category = flight.Category,
                ItineraryItemId = flight.ItineraryItemId,
                Airline = flight.Airline,
                FlightNumber = flight.FlightNumber,
                DepartureLocation = flight.DepartureLocation,
                ArrivalLocation = flight.ArrivalLocation,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BookingReference = flight.BookingReference,
                SeatNumber = flight.SeatNumber,
                Amount = flight.Amount,
                TollFee = flight.TollFee,
                FuelCost = flight.FuelCost,
                ParkingFee = flight.ParkingFee,
                RentalCost = flight.RentalCost,
                Notes = flight.Notes,
                CreatedAt = flight.CreatedAt,
                UpdatedAt = flight.UpdatedAt
            };
        }

        private AccommodationDto MapToAccommodationDto(Entities.PersonalTrip.Accommodation accommodation)
        {
            return new AccommodationDto
            {
                Id = accommodation.Id,
                PersonalTripId = accommodation.PersonalTripId,
                Name = accommodation.Name,
                Type = accommodation.Type,
                Address = accommodation.Address,
                PostalCode = accommodation.PostalCode,
                Latitude = accommodation.Latitude,
                Longitude = accommodation.Longitude,
                CheckInTime = accommodation.CheckInTime,
                CheckOutTime = accommodation.CheckOutTime,
                BookingReference = accommodation.BookingReference,
                ContactNumber = accommodation.ContactNumber,
                Notes = accommodation.Notes,
                ExpenseAmount = accommodation.ExpenseAmount,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt
            };
        }

        private ItineraryItemDto MapToItineraryItemDto(Entities.PersonalTrip.ItineraryItem item)
        {
            return new ItineraryItemDto
            {
                Id = item.Id,
                DayNumber = item.DayNumber,
                LocationName = item.LocationName,
                Address = item.Address,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                GooglePlaceId = item.GooglePlaceId,
                StartTime = item.StartTime.HasValue ? item.StartTime.Value.ToString("HH:mm") : null,
                EndTime = item.EndTime.HasValue ? item.EndTime.Value.ToString("HH:mm") : null,
                OrderNum = item.OrderNum,
                Notes = item.Notes,
                Category = item.Category,
                PhoneNumber = item.PhoneNumber,
                KakaoPlaceUrl = item.KakaoPlaceUrl,
                ExpenseAmount = item.ExpenseAmount,
                IsAutoGenerated = item.IsAutoGenerated
            };
        }

        private async Task AddOrUpdateAutoGeneratedItineraryItems(Entities.PersonalTrip.PersonalTrip trip, Entities.PersonalTrip.Accommodation accommodation)
        {
            // Remove existing auto-generated items for this accommodation
            var existingAutoItems = trip.ItineraryItems
                .Where(i => i.IsAutoGenerated && i.GooglePlaceId == $"accommodation-{accommodation.Id}")
                .ToList();
            _context.ItineraryItems.RemoveRange(existingAutoItems);

            if (accommodation.CheckInTime.HasValue && accommodation.CheckOutTime.HasValue)
            {
                // Calculate DayNumber for check-in
                var checkInDate = new DateOnly(accommodation.CheckInTime.Value.Year, accommodation.CheckInTime.Value.Month, accommodation.CheckInTime.Value.Day);
                var checkInDayNumber = (int)(checkInDate.ToDateTime(TimeOnly.MinValue) - trip.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                // Create check-in item
                var checkInItem = new Entities.PersonalTrip.ItineraryItem
                {
                    PersonalTripId = trip.Id,
                    DayNumber = checkInDayNumber,
                    LocationName = $"{accommodation.Name} 체크인",
                    Address = accommodation.Address,
                    Latitude = accommodation.Latitude,
                    Longitude = accommodation.Longitude,
                    GooglePlaceId = $"accommodation-{accommodation.Id}", // Link to accommodation
                    StartTime = TimeOnly.FromDateTime(accommodation.CheckInTime.Value),
                    EndTime = TimeOnly.FromDateTime(accommodation.CheckInTime.Value.AddHours(1)), // Assume 1 hour for check-in
                    Notes = accommodation.Notes,
                    Category = "숙소 체크인",
                    IsAutoGenerated = true,
                    OrderNum = 0 // Will be reordered by the system
                };
                _context.ItineraryItems.Add(checkInItem);

                // Calculate DayNumber for check-out
                var checkOutDate = new DateOnly(accommodation.CheckOutTime.Value.Year, accommodation.CheckOutTime.Value.Month, accommodation.CheckOutTime.Value.Day);
                var checkOutDayNumber = (int)(checkOutDate.ToDateTime(TimeOnly.MinValue) - trip.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                // Create check-out item
                var checkOutItem = new Entities.PersonalTrip.ItineraryItem
                {
                    PersonalTripId = trip.Id,
                    DayNumber = checkOutDayNumber,
                    LocationName = $"{accommodation.Name} 체크아웃",
                    Address = accommodation.Address,
                    Latitude = accommodation.Latitude,
                    Longitude = accommodation.Longitude,
                    GooglePlaceId = $"accommodation-{accommodation.Id}", // Link to accommodation
                    StartTime = TimeOnly.FromDateTime(accommodation.CheckOutTime.Value.AddHours(-1)), // Assume 1 hour for check-out
                    EndTime = TimeOnly.FromDateTime(accommodation.CheckOutTime.Value),
                    Notes = accommodation.Notes,
                    Category = "숙소 체크아웃",
                    IsAutoGenerated = true,
                    OrderNum = 0 // Will be reordered by the system
                };
                _context.ItineraryItems.Add(checkOutItem);
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
