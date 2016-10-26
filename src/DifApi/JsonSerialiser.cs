using LinqInfer.Data;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DifApi
{
    public class JsonSerialiser : IObjectSerialiser
    {
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
            var obj = new JsonSerializer().Deserialize<T>(new JsonTextReader(reader));
            return Task.FromResult(obj);
        }

        public Task Serialise<T>(T obj, Encoding encoding, string mimeType, Stream output)
        {
            using (var writer = new StreamWriter(output, encoding, 1024, true))
            {
                new JsonSerializer().Serialize(writer, obj, typeof(T));
            }
            return Task.FromResult(0);
        }
    }
}