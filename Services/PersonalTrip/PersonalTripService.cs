using LocalRAG.Data;
using LocalRAG.DTOs.PersonalTrip;
using LocalRAG.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return trips.Select(t => new PersonalTripDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                StartDate = t.StartDate.ToString("yyyy-MM-dd"),
                EndDate = t.EndDate.ToString("yyyy-MM-dd"),
                Destination = t.Destination,
                City = t.City,
                UserId = t.UserId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Flights = t.Flights.Select(f => new FlightDto
                {
                    Id = f.Id,
                    PersonalTripId = f.PersonalTripId,
                    Airline = f.Airline,
                    FlightNumber = f.FlightNumber,
                    DepartureLocation = f.DepartureLocation,
                    ArrivalLocation = f.ArrivalLocation,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    BookingReference = f.BookingReference,
                    SeatNumber = f.SeatNumber,
                    Notes = f.Notes,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).OrderBy(f => f.DepartureTime).ToList(),
                Accommodations = t.Accommodations.Select(a => new AccommodationDto
                {
                    Id = a.Id,
                    PersonalTripId = a.PersonalTripId,
                    Name = a.Name,
                    Type = a.Type,
                    Address = a.Address,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    BookingReference = a.BookingReference,
                    ContactNumber = a.ContactNumber,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).OrderBy(a => a.CheckInTime).ToList()
            }).ToList();
        }

        public async Task<PersonalTripDto?> GetTripByIdAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips
                .AsNoTracking()
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
                return null;

            return new PersonalTripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                StartDate = trip.StartDate.ToString("yyyy-MM-dd"),
                EndDate = trip.EndDate.ToString("yyyy-MM-dd"),
                Destination = trip.Destination,
                City = trip.City,
                UserId = trip.UserId,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                Flights = trip.Flights.Select(f => new FlightDto
                {
                    Id = f.Id,
                    PersonalTripId = f.PersonalTripId,
                    Airline = f.Airline,
                    FlightNumber = f.FlightNumber,
                    DepartureLocation = f.DepartureLocation,
                    ArrivalLocation = f.ArrivalLocation,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    BookingReference = f.BookingReference,
                    SeatNumber = f.SeatNumber,
                    Notes = f.Notes,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).OrderBy(f => f.DepartureTime).ToList(),
                Accommodations = trip.Accommodations.Select(a => new AccommodationDto
                {
                    Id = a.Id,
                    PersonalTripId = a.PersonalTripId,
                    Name = a.Name,
                    Type = a.Type,
                    Address = a.Address,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    BookingReference = a.BookingReference,
                    ContactNumber = a.ContactNumber,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).OrderBy(a => a.CheckInTime).ToList()
            };
        }

        public async Task<PersonalTripDto> CreateTripAsync(CreatePersonalTripDto dto, int userId)
        {
            if (!DateOnly.TryParseExact(dto.StartDate, "yyyy-MM-dd", out var startDate))
                throw new ArgumentException("시작일 형식이 올바르지 않습니다. (yyyy-MM-dd)");

            if (!DateOnly.TryParseExact(dto.EndDate, "yyyy-MM-dd", out var endDate))
                throw new ArgumentException("종료일 형식이 올바르지 않습니다. (yyyy-MM-dd)");

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
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PersonalTrips.Add(trip);
            await _context.SaveChangesAsync();

            return new PersonalTripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                StartDate = trip.StartDate.ToString("yyyy-MM-dd"),
                EndDate = trip.EndDate.ToString("yyyy-MM-dd"),
                Destination = trip.Destination,
                City = trip.City,
                UserId = trip.UserId,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                Flights = new List<FlightDto>(),
                Accommodations = new List<AccommodationDto>()
            };
        }

        public async Task<PersonalTripDto?> UpdateTripAsync(int tripId, UpdatePersonalTripDto dto, int userId)
        {
            var trip = await _context.PersonalTrips
                .Include(t => t.Flights)
                .Include(t => t.Accommodations)
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
                return null;

            if (!DateOnly.TryParseExact(dto.StartDate, "yyyy-MM-dd", out var startDate))
                throw new ArgumentException("시작일 형식이 올바르지 않습니다. (yyyy-MM-dd)");

            if (!DateOnly.TryParseExact(dto.EndDate, "yyyy-MM-dd", out var endDate))
                throw new ArgumentException("종료일 형식이 올바르지 않습니다. (yyyy-MM-dd)");

            if (endDate < startDate)
                throw new ArgumentException("종료일은 시작일보다 이후여야 합니다.");

            trip.Title = dto.Title;
            trip.Description = dto.Description;
            trip.StartDate = startDate;
            trip.EndDate = endDate;
            trip.Destination = dto.Destination;
            trip.City = dto.City;
            trip.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PersonalTripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                StartDate = trip.StartDate.ToString("yyyy-MM-dd"),
                EndDate = trip.EndDate.ToString("yyyy-MM-dd"),
                Destination = trip.Destination,
                City = trip.City,
                UserId = trip.UserId,
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                Flights = trip.Flights.Select(f => new FlightDto
                {
                    Id = f.Id,
                    PersonalTripId = f.PersonalTripId,
                    Airline = f.Airline,
                    FlightNumber = f.FlightNumber,
                    DepartureLocation = f.DepartureLocation,
                    ArrivalLocation = f.ArrivalLocation,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    BookingReference = f.BookingReference,
                    SeatNumber = f.SeatNumber,
                    Notes = f.Notes,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).OrderBy(f => f.DepartureTime).ToList(),
                Accommodations = trip.Accommodations.Select(a => new AccommodationDto
                {
                    Id = a.Id,
                    PersonalTripId = a.PersonalTripId,
                    Name = a.Name,
                    Type = a.Type,
                    Address = a.Address,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    BookingReference = a.BookingReference,
                    ContactNumber = a.ContactNumber,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).OrderBy(a => a.CheckInTime).ToList()
            };
        }

        public async Task<bool> DeleteTripAsync(int tripId, int userId)
        {
            var trip = await _context.PersonalTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
                return false;

            _context.PersonalTrips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Flight CRUD

        public async Task<FlightDto> AddFlightAsync(int tripId, CreateFlightDto dto, int userId)
        {
            var trip = await _context.PersonalTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
                throw new ArgumentException("여행을 찾을 수 없습니다.");

            var flight = new Entities.PersonalTrip.Flight
            {
                PersonalTripId = tripId,
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

            return new FlightDto
            {
                Id = flight.Id,
                PersonalTripId = flight.PersonalTripId,
                Airline = flight.Airline,
                FlightNumber = flight.FlightNumber,
                DepartureLocation = flight.DepartureLocation,
                ArrivalLocation = flight.ArrivalLocation,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BookingReference = flight.BookingReference,
                SeatNumber = flight.SeatNumber,
                Notes = flight.Notes,
                CreatedAt = flight.CreatedAt,
                UpdatedAt = flight.UpdatedAt
            };
        }

        public async Task<FlightDto?> UpdateFlightAsync(int flightId, CreateFlightDto dto, int userId)
        {
            var flight = await _context.Flights
                .Include(f => f.PersonalTrip)
                .FirstOrDefaultAsync(f => f.Id == flightId && f.PersonalTrip.UserId == userId);

            if (flight == null)
                return null;

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

            return new FlightDto
            {
                Id = flight.Id,
                PersonalTripId = flight.PersonalTripId,
                Airline = flight.Airline,
                FlightNumber = flight.FlightNumber,
                DepartureLocation = flight.DepartureLocation,
                ArrivalLocation = flight.ArrivalLocation,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                BookingReference = flight.BookingReference,
                SeatNumber = flight.SeatNumber,
                Notes = flight.Notes,
                CreatedAt = flight.CreatedAt,
                UpdatedAt = flight.UpdatedAt
            };
        }

        public async Task<bool> DeleteFlightAsync(int flightId, int userId)
        {
            var flight = await _context.Flights
                .Include(f => f.PersonalTrip)
                .FirstOrDefaultAsync(f => f.Id == flightId && f.PersonalTrip.UserId == userId);

            if (flight == null)
                return false;

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Accommodation CRUD

        public async Task<AccommodationDto> AddAccommodationAsync(int tripId, CreateAccommodationDto dto, int userId)
        {
            var trip = await _context.PersonalTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
                throw new ArgumentException("여행을 찾을 수 없습니다.");

            var accommodation = new Entities.PersonalTrip.Accommodation
            {
                PersonalTripId = tripId,
                Name = dto.Name,
                Type = dto.Type,
                Address = dto.Address,
                CheckInTime = dto.CheckInTime,
                CheckOutTime = dto.CheckOutTime,
                BookingReference = dto.BookingReference,
                ContactNumber = dto.ContactNumber,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync();

            return new AccommodationDto
            {
                Id = accommodation.Id,
                PersonalTripId = accommodation.PersonalTripId,
                Name = accommodation.Name,
                Type = accommodation.Type,
                Address = accommodation.Address,
                CheckInTime = accommodation.CheckInTime,
                CheckOutTime = accommodation.CheckOutTime,
                BookingReference = accommodation.BookingReference,
                ContactNumber = accommodation.ContactNumber,
                Notes = accommodation.Notes,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt
            };
        }

        public async Task<AccommodationDto?> UpdateAccommodationAsync(int accommodationId, CreateAccommodationDto dto, int userId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.PersonalTrip)
                .FirstOrDefaultAsync(a => a.Id == accommodationId && a.PersonalTrip.UserId == userId);

            if (accommodation == null)
                return null;

            accommodation.Name = dto.Name;
            accommodation.Type = dto.Type;
            accommodation.Address = dto.Address;
            accommodation.CheckInTime = dto.CheckInTime;
            accommodation.CheckOutTime = dto.CheckOutTime;
            accommodation.BookingReference = dto.BookingReference;
            accommodation.ContactNumber = dto.ContactNumber;
            accommodation.Notes = dto.Notes;
            accommodation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AccommodationDto
            {
                Id = accommodation.Id,
                PersonalTripId = accommodation.PersonalTripId,
                Name = accommodation.Name,
                Type = accommodation.Type,
                Address = accommodation.Address,
                CheckInTime = accommodation.CheckInTime,
                CheckOutTime = accommodation.CheckOutTime,
                BookingReference = accommodation.BookingReference,
                ContactNumber = accommodation.ContactNumber,
                Notes = accommodation.Notes,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt
            };
        }

        public async Task<bool> DeleteAccommodationAsync(int accommodationId, int userId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.PersonalTrip)
                .FirstOrDefaultAsync(a => a.Id == accommodationId && a.PersonalTrip.UserId == userId);

            if (accommodation == null)
                return false;

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
