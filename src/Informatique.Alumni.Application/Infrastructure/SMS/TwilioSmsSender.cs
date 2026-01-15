using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Sms;
using Informatique.Alumni.Infrastructure.SMS; // Keep utilizing the explicit namespace for the custom interface

namespace Informatique.Alumni.Infrastructure.SMS;

public class TwilioSmsSender : ISmsSender, ITransientDependency
{
    private readonly IDistributedEventBus _eventBus;
    private readonly IConfiguration _configuration;
    
    // Circuit Breaker Policy: Breaks after 3 failures, waits 1 minute before retrying
    private static readonly AsyncCircuitBreakerPolicy _circuitBreaker = Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 3, 
            durationOfBreak: TimeSpan.FromMinutes(1)
        );

    public TwilioSmsSender(
        IDistributedEventBus eventBus,
        IConfiguration configuration)
    {
        _eventBus = eventBus;
        _configuration = configuration;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        bool isSuccess = false;
        string? errorMessage = null;

        try
        {
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromNumber = _configuration["Twilio:FromNumber"];

                if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
                {
                    throw new InvalidOperationException("Twilio credentials are not configured.");
                }

                TwilioClient.Init(accountSid, authToken);

                await MessageResource.CreateAsync(
                    to: new PhoneNumber(phoneNumber),
                    from: !string.IsNullOrWhiteSpace(fromNumber) ? new PhoneNumber(fromNumber) : null,
                    body: message
                );
            });

            isSuccess = true;
        }
        catch (BrokenCircuitException)
        {
            errorMessage = "SMS Provider Circuit is Open (Too many failures). Dispatching aborted.";
            // In a real system, you might fallback to another provider here
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            // Log the attempt (Success or Failure)
            await _eventBus.PublishAsync(new SmsSentEto(
                phoneNumber,
                message,
                "Twilio",
                isSuccess,
                errorMessage
            ));
        }
    }
}
