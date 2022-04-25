using System;
using Newtonsoft.Json;

namespace Ya.D.Helpers
{
    public class ItemTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (existingValue != null)
            {
                return existingValue.ToString() == "dir";
            }
            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((bool)value ? "dir" : "file");
        }
    }
}
