using AutoMapper;
using TicketFlow.BLL.DTOs.Account;
using TicketFlow.BLL.DTOs.Auth;
using TicketFlow.BLL.DTOs.Category;
using TicketFlow.BLL.DTOs.Event;
using TicketFlow.BLL.DTOs.Place;
using TicketFlow.BLL.DTOs.Reservation;
using TicketFlow.DAL.Models;

namespace TicketFlow.BLL.Mapping
{
    public class MappingProfile : Profile 
    {
        public MappingProfile()
        {
            // 1. RegisterDto -> AppUser
            CreateMap<RegisterRequestDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

            // 2. AppUser -> UserProfileDto
            CreateMap<AppUser, UserProfileDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Handled manually in service from UserManager

            // 3. UpdateProfileDto -> AppUser
            CreateMap<UpdateProfileDto, AppUser>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();

            // Place Mappings
            CreateMap<Place, PlaceDto>();
            CreateMap<CreatePlaceDto, Place>();

            // Event Mappings
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.PlaceName, opt => opt.MapFrom(src => src.Place.Name))
                .ForMember(dest => dest.OrganizerName, opt => opt.MapFrom(src => src.Organizer.FullName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => EventStatusEnum.Draft));

            CreateMap<EventSeat, EventSeatDto>()
                .ForMember(dest => dest.Section, opt => opt.MapFrom(src => src.Seat.Section))
                .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Seat.Row))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.Seat.SeatNumber.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.ReservationItems.Sum(ri => ri.Price)))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.ReservationItems));

            CreateMap<ReservationItem, ReservationItemDto>()
                .ForMember(dest => dest.Section, opt => opt.MapFrom(src => src.EventSeat.Seat.Section))
                .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.EventSeat.Seat.Row))
                .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.EventSeat.Seat.SeatNumber));
        }
    }
}