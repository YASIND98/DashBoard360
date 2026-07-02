using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DashboardTsy.Api.Services;

public sealed class ScoreCardTokenService : IScoreCardTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ServiceBusOptions _options;

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ScoreCardTokenService(HttpClient httpClient, IOptions<ServiceBusOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken != null && DateTimeOffset.UtcNow < _expiresAt)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // double-check after acquiring lock
            if (_cachedToken != null && DateTimeOffset.UtcNow < _expiresAt)
                return _cachedToken;

            var body = new
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret,
                audience = _options.Audience,
                grant_type = "client_credentials"
            };

            using var response = await _httpClient
                .PostAsJsonAsync(_options.TokenUrl, body, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, _jsonOptions)
                ?? throw new InvalidOperationException("ServiceBus token response was empty.");

            _cachedToken = tokenResponse.AccessToken;
            // Refresh 60 seconds before actual expiry to avoid edge cases
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
