using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TicketFlow.BLL.DTOs.Account;
using TicketFlow.BLL.DTOs.Auth;
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
        }
    }
}