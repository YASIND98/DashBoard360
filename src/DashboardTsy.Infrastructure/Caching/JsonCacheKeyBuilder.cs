using System.Text.Json;
using System.Text.Json.Serialization;
using DashboardTsy.Application.Caching;

namespace DashboardTsy.Infrastructure.Caching;

/// <summary>
/// Argümanları System.Text.Json ile deterministik biçimde serialize edip key üretir.
/// SessionId ve session bazlı benzer alanlar cache isabet oranını düşürmemek için elenir.
/// Key formatı: "{prefix}::{methodName}::{jsonPayload}"
/// </summary>
public sealed class JsonCacheKeyBuilder : ICacheKeyBuilder
{
    // Case-insensitive; sessionId, sessionID, SessionId → hepsi elenir.
    private static readonly HashSet<string> IgnoredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "SessionId"
    };

    private static readonly JsonSerializerOptions Options = new()
    {
        // Property'leri deterministik sırada yazmak için custom converter kullanıyoruz.
        // Default converter, tanımlanma sırasında yazar — reflection order refactoring'de değişebilir.
        Converters = { new DeterministicObjectConverterFactory(IgnoredProperties) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public string Build(string prefix, string methodName, object? arguments)
    {
        if (arguments is null)
            return $"{prefix}::{methodName}";

        var payload = JsonSerializer.Serialize(arguments, Options);
        return $"{prefix}::{methodName}::{payload}";
    }

    /// <summary>
    /// Karmaşık nesneleri property adına göre alfabetik yazan, belirtilen property'leri atlayan converter fabrikası.
    /// Primitive tipler (string, int, DateTime, ...) default serializer'a bırakılır.
    /// </summary>
    private sealed class DeterministicObjectConverterFactory : JsonConverterFactory
    {
        private readonly HashSet<string> _ignored;
        public DeterministicObjectConverterFactory(HashSet<string> ignored) => _ignored = ignored;

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(string)) return false;
            if (typeToConvert.IsPrimitive) return false;
            if (typeToConvert.IsEnum) return false;
            if (Nullable.GetUnderlyingType(typeToConvert) != null) return false;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(typeToConvert)) return false;

            // Sadece POCO / request DTO'lar için etkin ol; DateTime, decimal vb. default kalır.
            return typeToConvert.IsClass;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(
                typeof(DeterministicObjectConverter<>).MakeGenericType(typeToConvert),
                _ignored)!;
    }

    private sealed class DeterministicObjectConverter<T> : JsonConverter<T>
    {
        private readonly HashSet<string> _ignored;
        public DeterministicObjectConverter(HashSet<string> ignored) => _ignored = ignored;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Cache key builder is write-only.");

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            var properties = value.GetType().GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && !_ignored.Contains(p.Name))
                .OrderBy(p => p.Name, StringComparer.Ordinal);

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(value);
                if (propValue is null) continue; // WhenWritingNull semantic'i manuel uygula

                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, LeafOptions);
            }

            writer.WriteEndObject();
        }

        // Inner serialize'da sonsuz döngüye girmemek için factory'siz temiz seçenekler.
        // Alt seviyedeki karmaşık nesneleri default serializer yazar; SessionId gibi filtreler yalnızca üst seviyede.
        private static readonly JsonSerializerOptions LeafOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }
}
