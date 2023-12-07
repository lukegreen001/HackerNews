using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection.Metadata.Ecma335;

namespace HackerNewsApi.Models
{
    public record Story
    {        
        public string title { get; init; } = "";

        [JsonProperty("url")]
        public string uri { get; init; } = "";

        [JsonProperty("by")]
        public string postedBy { get; init; } = "";

        [JsonConverter(typeof(MyDateTimeConverter))]        
        public DateTime  time { get; init; }
        
        public int  score { get; init; }

        [JsonProperty("descendants")]
        public int commentCount { get; init; }
    }

    internal class MyDateTimeConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long t;

            if (reader.Value.GetType() != typeof(long))
                t = long.Parse((string)reader.Value);
            else
                t = (long)reader.Value;
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(t);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
