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

        public static TextTree Create(RequestContext context)
        {
            var response = new TextTree();
            var request = new TextTree();

            foreach (var header in context.OwinContext.Request.Header.Headers)
            {
                request.Properties[header.Key] = string.Join(",", header.Value);
            }

            foreach (var header in context.OwinContext.Response.Header.Headers)
            {
                response.Properties[header.Key] = string.Join(",", header.Value);
            }

            var root = new TextTree();

            root.Children["request"] = request;
            root.Children["response"] = response;

            return root;
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