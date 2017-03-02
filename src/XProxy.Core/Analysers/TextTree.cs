using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XProxy.Core.Analysers
{
    public class TextTree
    {
        public TextTree()
        {
            Properties = new Dictionary<string, string>();
            Children = new Dictionary<string, TextTree>();
        }

        public IDictionary<string, string> Properties { get; private set; }

        public IDictionary<string, TextTree> Children { get; private set; }

        public async Task Write(Stream output)
        {
            var json = JsonConvert.SerializeObject(this);

            using (var writer = new StreamWriter(output))
            {
                await writer.WriteAsync(json);
            }
        }

        public static async Task<TextTree> ReadAsync(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var json = await reader.ReadToEndAsync();

                return JsonConvert.DeserializeObject<TextTree>(json);
            }
        }
    }
}