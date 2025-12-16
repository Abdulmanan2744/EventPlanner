using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Security
{
    public class TokenCleanupService : IHostedService, IDisposable
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenCleanupService> _logger;
        private Timer _timer;
        private readonly string _connectionString;

        public TokenCleanupService(IConfiguration configuration, ILogger<TokenCleanupService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured properly.");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token Cleanup Service is starting.");

            var intervalString = _configuration["TokenCleanupServiceSettings:Interval"];
            if (!TimeSpan.TryParse(intervalString, out var interval))
            {
                _logger.LogWarning("TokenCleanupServiceSettings:Interval is not a valid TimeSpan. Defaulting to 30 minutes.");
                interval = TimeSpan.FromMinutes(30); // Fallback to 30 minutes if not valid
            }

            _timer = new Timer(ExecuteCleanup, null, TimeSpan.Zero, interval);

            return Task.CompletedTask;
        }

        private void ExecuteCleanup(object state)
        {
            try
            {
                RemoveExpiredTokens();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing expired tokens.");
            }
        }

        private void RemoveExpiredTokens()
        {
            var query = "DELETE FROM AuthStore WHERE expiry_date < @CurrentDate";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrentDate", DateTime.UtcNow);

                    try
                    {
                        connection.Open();
                        var affectedRows = command.ExecuteNonQuery();
                        _logger.LogInformation($"Removed {affectedRows} expired tokens.");
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "A SQL error occurred while removing expired tokens.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unexpected error occurred while removing expired tokens.");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Token Cleanup Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
