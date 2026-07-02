using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DashboardTsy.Api.Services;

public sealed class ScoreCardTokenService : IScoreCardTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ServiceBusOptions _options;
    private readonly ILogger<ScoreCardTokenService> _logger;

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ScoreCardTokenService(HttpClient httpClient, IOptions<ServiceBusOptions> options, ILogger<ScoreCardTokenService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken != null && DateTimeOffset.UtcNow < _expiresAt)
        {
            _logger.LogInformation("[TokenService] Cache'den token döndürüldü, geçerlilik: {ExpiresAt}", _expiresAt);
            return _cachedToken;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken != null && DateTimeOffset.UtcNow < _expiresAt)
            {
                _logger.LogInformation("[TokenService] Cache'den token döndürüldü (double-check), geçerlilik: {ExpiresAt}", _expiresAt);
                return _cachedToken;
            }

            _logger.LogInformation("[TokenService] Yeni token isteniyor. URL: {TokenUrl}, ClientId: {ClientId}", _options.TokenUrl, _options.ClientId);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string, string>("audience", _options.Audience)
            });

            using var response = await _httpClient
                .PostAsync(_options.TokenUrl, formData, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("[TokenService] ServiceBus yanıtı: {StatusCode}", (int)response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[TokenService] ServiceBus hata döndü: {StatusCode} {Body}", (int)response.StatusCode, responseBody);
                response.EnsureSuccessStatusCode();
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody, _jsonOptions)
                ?? throw new InvalidOperationException("ServiceBus token response was empty.");

            _cachedToken = tokenResponse.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            _logger.LogInformation("[TokenService] Token alındı, ExpiresIn: {ExpiresIn}s, geçerlilik: {ExpiresAt}", tokenResponse.ExpiresIn, _expiresAt);

            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TokenService] Token alınırken hata oluştu");
            throw;
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
