namespace TicketFlow.BLL;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.BLL.Mapping;
using TicketFlow.BLL.Services.Classes;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.BLL.Settings;
using TicketFlow.DAL;

public static class BLLServiceRegister
{
    public static IServiceCollection AddBLLServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDALServices(configuration);
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        services.AddAutoMapper(_ => { }, typeof(MappingProfile).Assembly);

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPlaceService, PlaceService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IPhoneVerificationService, PhoneVerificationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        return services;
    }
}