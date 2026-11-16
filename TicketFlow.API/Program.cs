using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using TicketFlow.API.BackgroundServices;
using TicketFlow.API.Hubs;
using TicketFlow.API.Middlewares;
using TicketFlow.API.Services.Classes;
using TicketFlow.BLL;
using TicketFlow.BLL.Services;
using TicketFlow.BLL.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBLLServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<ISeatNotificationService, SeatNotificationService>();
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured in appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

            var userIdStr = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenStamp = context.Principal?.FindFirstValue("SecurityStamp");

            if (Guid.TryParse(userIdStr, out var userId) && !string.IsNullOrEmpty(tokenStamp))
            {
                var isValid = await authService.ValidateSecurityStampAsync(userId, tokenStamp);
                if (!isValid)
                {
                    context.Fail("This token has been revoked or user session expired.");
                }
            }
            else
            {
                context.Fail("Invalid token claim structure.");
            }
        }
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Or allow set origins
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 👈 Required for SignalR WebSockets/Server-Sent Events
    });
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketFlow API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your_token}"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});
builder.Services.AddHostedService<ExpiredReservationSweeper>();
var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("SignalRCors");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.SeedIdentityDataAsync();
app.MapHub<SeatHub>("/hubs/seats").RequireCors("SignalRCors");
app.Run();