using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.API.BackgroundServices
{
    public class ExpiredReservationSweeper : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredReservationSweeper> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public ExpiredReservationSweeper(
            IServiceProvider serviceProvider,
            ILogger<ExpiredReservationSweeper> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                    var expiredCount = await reservationService.ProcessExpiredReservationsAsync(stoppingToken);

                    if (expiredCount > 0)
                    {
                        _logger.LogInformation("Successfully expired {Count} overdue reservations.", expiredCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sweeping expired reservations.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}