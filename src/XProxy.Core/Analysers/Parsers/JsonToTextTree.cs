using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace XProxy.Core.Analysers.Parsers
{
    class JsonToTextTree
    {
        public static bool CanHandle(string mime)
        {
            return mime.Contains("json");
        }

        public static TextTree Read(Stream data)
        {
            var tree = new TextTree();

            using (var reader = new JsonTextReader(new StreamReader(data)))
            {
                var json = JObject.ReadFrom(reader);

                Read(tree, json);
            }

            return tree;
        }

        private static TextTree Read(TextTree parent, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    return ReadArray(parent, (JArray)token);
                case JTokenType.Object:
                    return ReadObject(parent, (JObject)token);
                default:
                    return parent;
            }
        }

        private static TextTree ReadObject(TextTree parent, JObject json)
        {
            foreach (var prop in json.Properties())
            {
                switch (prop.Type)
                {
                    case JTokenType.Object:
                        {
                            var obj = ReadObject(new TextTree(), ((JObject)prop.Value));
                            parent.Children[prop.Name] = obj;
                            break;
                        }
                    case JTokenType.Array:
                        {
                            var obj = ReadArray(new TextTree(), ((JArray)prop.Value));
                            parent.Children[prop.Name] = obj;
                            break;
                        }
                    default:
                        parent.Properties[prop.Name] = prop.Value.ToString();
                        break;
                }
            }

            return parent;
        }

        private static TextTree ReadArray(TextTree parent, JArray jarray)
        {
            int i = 0;

            foreach (var item in jarray)
            {
                if (item.Type == JTokenType.Object || item.Type == JTokenType.Array)
                {
                    parent.Children[string.Format("[{0}]", i++)] = Read(new TextTree(), item);
                }
                else
                {
                    parent.Properties[string.Format("[{0}]", i++)] = item.ToString();
                }
            }

            return parent;
        }
    }
}