using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mapapi
{
    class IPAddressConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return IPAddress.Parse(reader.GetString());
            //return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString()); 
        }
    }
}
