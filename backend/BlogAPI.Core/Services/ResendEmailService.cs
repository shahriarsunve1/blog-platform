using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlogAPI.Core.Services;

/// <summary>
/// Sends transactional email via the Resend API (https://resend.com). No-ops
/// with a log line when no API key is configured, so local dev/tests never
/// need real credentials.
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var apiKey = _configuration["Email:ResendApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogInformation("Email skipped (no Resend API key configured): {Subject} to {To}", subject, to);
            return;
        }

        try
        {
            var fromAddress = _configuration["Email:FromAddress"] ?? "onboarding@resend.dev";

            var request = new HttpRequestMessage(HttpMethod.Post, "emails")
            {
                Content = JsonContent.Create(new
                {
                    from = fromAddress,
                    to = new[] { to },
                    subject,
                    html = htmlBody
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Email send failed ({StatusCode}) to {To}: {Body}", response.StatusCode, to, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email send threw an exception for {To}", to);
        }
    }
}
