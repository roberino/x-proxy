using LinqInfer.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DifApi
{
    public class JsonSerialiser : IObjectSerialiser
    {
        private readonly JsonSerializer _serialiser;

        public JsonSerialiser()
        {
            _serialiser = new JsonSerializer();

            _serialiser.Formatting = Formatting.Indented;
            _serialiser.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _serialiser.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            _serialiser.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }

        public string[] SupportedMimeTypes
        {
            get
            {
                return new string[] { "application/json" };
            }
        }

        public Task<T> Deserialise<T>(Stream input, Encoding encoding, string mimeType)
        {
            var reader = new StreamReader(input, encoding);
            var obj = _serialiser.Deserialize<T>(new JsonTextReader(reader));
            return Task.FromResult(obj);
        }

        public async Task Serialise<T>(T obj, Encoding encoding, string mimeType, Stream output)
        {
            using (var writer = new StreamWriter(output, encoding, 1024, true))
            {
                _serialiser.Serialize(writer, obj, typeof(T));

                await writer.FlushAsync();
            }
        }
    }
}