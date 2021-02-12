using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample
{
    public class HostedService : IHostedService
    {
        private readonly ILogger<HostedService> _logger;

        public HostedService(ILogger<HostedService> logger)
        {
            _logger = logger;            
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            await Task.Delay(1000);
            _logger.LogInformation("Started");
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            await Task.Delay(1000);
            _logger.LogInformation("Stopped");
        }
    }
}
