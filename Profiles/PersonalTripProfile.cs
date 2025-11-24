using AutoMapper;
using LocalRAG.DTOs.PersonalTrip;
using LocalRAG.Entities.PersonalTrip;

namespace LocalRAG.Profiles
{
    public class PersonalTripProfile : Profile
    {
        public PersonalTripProfile()
        {
            // Flight mappings
            CreateMap<Flight, FlightDto>().ReverseMap();
            CreateMap<CreateFlightDto, Flight>();
        }
    }
}
