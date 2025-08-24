using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Http;
using WhatsAppWaha.Core.Configuration;
using System.ComponentModel.DataAnnotations;

namespace WhatsAppWaha.Core.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Adds all WhatsApp WAHA framework services to the DI container.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="configuration">The configuration.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddWhatsAppWahaFramework(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // Add and validate configuration
    services.AddConfigurationWithValidation(configuration);

    // Add HTTP clients with retry policies
    services.AddHttpClientsWithRetryPolicies();

    // Add core services (will be implemented in future tasks)
    services.AddApplicationServices();

    return services;
  }

  /// <summary>
  /// Adds configuration models with validation to the DI container.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <param name="configuration">The configuration.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddConfigurationWithValidation(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    // Configure and validate WahaSettings
    services.Configure<WahaSettings>(configuration.GetSection(WahaSettings.SectionName));
    services.AddSingleton<IValidateOptions<WahaSettings>, ValidateOptionsWithDataAnnotations<WahaSettings>>();

    // Configure and validate NtfySettings
    services.Configure<NtfySettings>(configuration.GetSection(NtfySettings.SectionName));
    services.AddSingleton<IValidateOptions<NtfySettings>, ValidateOptionsWithDataAnnotations<NtfySettings>>();

    // Configure and validate AppSettings
    services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
    services.AddSingleton<IValidateOptions<AppSettings>, ValidateOptionsWithDataAnnotations<AppSettings>>();

    // Configure and validate LoggingSettings
    services.Configure<LoggingSettings>(configuration.GetSection(LoggingSettings.SectionName));
    services.AddSingleton<IValidateOptions<LoggingSettings>, ValidateOptionsWithDataAnnotations<LoggingSettings>>();

    return services;
  }

  /// <summary>
  /// Adds HTTP clients with Polly retry policies.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddHttpClientsWithRetryPolicies(this IServiceCollection services)
  {
    // Add WAHA HTTP client with retry policy
    services.AddHttpClient("WahaClient", (serviceProvider, client) =>
    {
      var wahaSettings = serviceProvider.GetRequiredService<IOptions<WahaSettings>>().Value;
      client.BaseAddress = new Uri(wahaSettings.BaseUrl);
      client.Timeout = TimeSpan.FromSeconds(wahaSettings.TimeoutSeconds);

      if (!string.IsNullOrEmpty(wahaSettings.ApiKey))
      {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {wahaSettings.ApiKey}");
      }
    })
    .AddPolicyHandler((serviceProvider, request) =>
    {
      var wahaSettings = serviceProvider.GetRequiredService<IOptions<WahaSettings>>().Value;
      return CreateRetryPolicy(wahaSettings.MaxRetryAttempts, wahaSettings.RetryDelayMs);
    });

    // Add ntfy HTTP client with retry policy
    services.AddHttpClient("NtfyClient", (serviceProvider, client) =>
    {
      var ntfySettings = serviceProvider.GetRequiredService<IOptions<NtfySettings>>().Value;
      client.BaseAddress = new Uri(ntfySettings.BaseUrl);
      client.Timeout = TimeSpan.FromSeconds(ntfySettings.TimeoutSeconds);

      if (!string.IsNullOrEmpty(ntfySettings.AuthToken))
      {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ntfySettings.AuthToken}");
      }
    })
    .AddPolicyHandler(CreateRetryPolicy(3, 1000)); // Standard retry for ntfy

    return services;
  }

  /// <summary>
  /// Adds application-specific services.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    // Placeholder for future service registrations
    // Will be implemented in subsequent tasks:
    // - IWahaService
    // - INtfyService
    // - IMessageProcessor
    // etc.

    return services;
  }

  /// <summary>
  /// Creates a retry policy with exponential backoff.
  /// </summary>
  /// <param name="maxRetryAttempts">Maximum number of retry attempts.</param>
  /// <param name="baseDelayMs">Base delay in milliseconds.</param>
  /// <returns>The retry policy.</returns>
  private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(int maxRetryAttempts, int baseDelayMs)
  {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: maxRetryAttempts,
            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                baseDelayMs * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
            onRetry: (outcome, timespan, retryCount, context) =>
            {
              // This will be enhanced with structured logging in the next part
              Console.WriteLine($"Retry {retryCount} after {timespan}ms delay");
            });
  }
}

/// <summary>
/// Generic validator for options using data annotations.
/// </summary>
/// <typeparam name="TOptions">The options type to validate.</typeparam>
public class ValidateOptionsWithDataAnnotations<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
  /// <summary>
  /// Validates the options using data annotations.
  /// </summary>
  /// <param name="name">The options name.</param>
  /// <param name="options">The options instance.</param>
  /// <returns>The validation result.</returns>
  public ValidateOptionsResult Validate(string? name, TOptions options)
  {
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(options, serviceProvider: null, items: null);

    if (Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true))
    {
      return ValidateOptionsResult.Success;
    }

    var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown validation error").ToList();
    return ValidateOptionsResult.Fail(errors);
  }
}
