using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Infrastructure.SMS;

public class TwilioSmsSender : ISmsSender, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsSender> _logger;

    // Circuit Breaker Policy: Breaks after 3 failures, waits 1 minute before retrying
    private static readonly AsyncCircuitBreakerPolicy _circuitBreaker = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromMinutes(1)
        );

    public TwilioSmsSender(
        IConfiguration configuration,
        ILogger<TwilioSmsSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken  = _configuration["Twilio:AuthToken"];
        var fromNumber = _configuration["Twilio:FromNumber"];

        if (string.IsNullOrWhiteSpace(accountSid) ||
            accountSid.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(authToken)   ||
            authToken.StartsWith("REPLACE",  StringComparison.OrdinalIgnoreCase))
        {
            var msg = "Twilio credentials are not configured in appsettings.json. SMS cannot be sent until AccountSid, AuthToken, and FromNumber are set.";
            _logger.LogError(msg);
            throw new InvalidOperationException(msg);
        }

        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                TwilioClient.Init(accountSid, authToken);

                var result = await MessageResource.CreateAsync(
                    to:   new PhoneNumber(phoneNumber),
                    from: !string.IsNullOrWhiteSpace(fromNumber) ? new PhoneNumber(fromNumber) : null,
                    body: message
                );

                _logger.LogInformation("SMS sent to {Phone} — Twilio SID: {Sid}, Status: {Status}",
                    phoneNumber, result.Sid, result.Status);
            });
        }
        catch (BrokenCircuitException bce)
        {
            _logger.LogError(bce, "SMS Circuit Breaker is OPEN — too many consecutive Twilio failures.");
            throw; // Re-throw so the job logs Failed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone} via Twilio.", phoneNumber);
            throw; // Re-throw so the job logs Failed instead of Success
        }
    }
}
