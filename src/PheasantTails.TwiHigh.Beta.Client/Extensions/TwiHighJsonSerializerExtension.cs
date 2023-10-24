namespace PheasantTails.TwiHigh.Beta.Client.Extensions;

using PheasantTails.TwiHigh.Data.Model.Tweets;
using PheasantTails.TwiHigh.Interface;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class TwiHighJsonSerializerExtension
{
    private static JsonSerializerOptions Options => new()
    {
        Converters = {
            new InterfaceConverter<ITweet, ResponseTweetContext>()
        },
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T?> TwiHighReadFromJsonAsync<T>(this HttpContent content, CancellationToken cancellationToken = default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        using var contentStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(contentStream, Options, cancellationToken);
    }

    private class InterfaceConverter<TInterface, TImplement> : JsonConverter<TInterface> where TImplement : TInterface
    {
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (TInterface?)JsonSerializer.Deserialize(ref reader, typeof(TImplement), options);
        }

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(TImplement), options);
        }
    }
}
