using System.Text.Json;
using System.Text.Json.Serialization;

namespace PulumiSharp;

internal class PulumiProfileJsonConverter : JsonConverter<PulumiProfile>
{
    public override bool CanConvert(Type type)
    {
        return type.IsAssignableFrom(typeof(PulumiProfile));
    }

    public override PulumiProfile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out var doc))
        {
            if (doc.RootElement.TryGetProperty("type", out var type))
            {
                var typeValue = type.GetInt32();
                var rootElement = doc.RootElement.GetRawText();

                return typeValue switch
                {
                    1 => JsonSerializer.Deserialize<AzurePulumiProfile>(rootElement, options),
                    _ => throw new JsonException($"{typeValue} has not been mapped to a custom type yet!")
                };
            }
            else
            {
                var rootElement = doc.RootElement.GetRawText();

                return JsonSerializer.Deserialize<AzurePulumiProfile>(rootElement, options);
            }
        }

        throw new JsonException("Failed to parse JsonDocument");
    }

    public override void Write(Utf8JsonWriter writer, PulumiProfile value, JsonSerializerOptions options)
    {
        if (value is AzurePulumiProfile azurePulumiProfile)
        {
            writer.WriteStartObject();
            writer.WriteNumber(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.Type)) ?? nameof(azurePulumiProfile.Type), azurePulumiProfile.Type);
            writer.WriteString(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.Organization)) ?? nameof(azurePulumiProfile.Organization), azurePulumiProfile.Organization);
            writer.WriteString(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.StorageAccountName)) ?? nameof(azurePulumiProfile.StorageAccountName), azurePulumiProfile.StorageAccountName);
            writer.WriteString(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.KeyVaultName)) ?? nameof(azurePulumiProfile.KeyVaultName), azurePulumiProfile.KeyVaultName);
            writer.WriteString(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.TenantId)) ?? nameof(azurePulumiProfile.TenantId), azurePulumiProfile.TenantId);
            writer.WriteString(options.PropertyNamingPolicy?.ConvertName(nameof(azurePulumiProfile.SubscriptionId)) ?? nameof(azurePulumiProfile.SubscriptionId), azurePulumiProfile.SubscriptionId);
            writer.WriteEndObject();
        }

        throw new JsonException("Failed to write JsonDocument");
    }
}