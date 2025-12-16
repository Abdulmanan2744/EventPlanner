using Microsoft.Data.SqlClient;

namespace UserSideAPI.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;
        private readonly string _connectionString;

        public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured properly.");
            }
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Extract token from Authorization header
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                // Validate token
                if (!await ValidateTokenAsync(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return;
                }
            }

            // Continue processing the request
            await _next(context);
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            var query = "SELECT COUNT(*) FROM AuthStore WHERE auth_token = @Token AND expiry_date >= @CurrentDate";

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@CurrentDate", DateTime.UtcNow);

                    try
                    {
                        await connection.OpenAsync();
                        var count = (int)await command.ExecuteScalarAsync();
                        return count > 0;
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "A SQL error occurred while validating the token.");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An unexpected error occurred while validating the token.");
                        return false;
                    }
                }
            }
        }
    }
}
