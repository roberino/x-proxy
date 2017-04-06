using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XProxy.Core.Models
{
    public class TextTree : ICanPersist
    {
        public TextTree()
        {
            Properties = new Dictionary<string, string>();
            Children = new Dictionary<string, TextTree>();
        }

        public IDictionary<string, string> Properties { get; private set; }

        public IDictionary<string, TextTree> Children { get; private set; }

        public async Task WriteAsync(Stream output)
        {
            var json = JsonConvert.SerializeObject(this);

            using (var writer = new StreamWriter(output))
            {
                await writer.WriteAsync(json);
            }
        }

        public async Task ReadAsync(Stream input)
        {
            var tree = await LoadAsync(input);

            Properties = tree.Properties;
            Children = tree.Children;
        }

        public static async Task<TextTree> LoadAsync(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var json = await reader.ReadToEndAsync();

                return JsonConvert.DeserializeObject<TextTree>(json);
            }
        }
    }
}