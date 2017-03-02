using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace XProxy.Core.Analysers.Parsers
{
    class JsonToTextTree
    {
        public static bool CanHandle(string mime)
        {
            return mime.Contains("json");
        }

        public static TextTree Read(Stream data, Encoding encoding)
        {
            var tree = new TextTree();

            using (var reader = new StreamReader(data, encoding, true, 1024, true))
            {
                {
                    int i = 0;

                    while (true)
                    {
                        var line = reader.ReadLine();

                        if (line == null) return tree;

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            i++;
                        }

                        if (i > 1) // expecting 2 request + 2 response header lines
                        {
                            bool startOfJsonFound = false;

                            while (!reader.EndOfStream)
                            {
                                var nextChar = reader.Peek();

                                if (nextChar == '[' || nextChar == '{')
                                {
                                    startOfJsonFound = true;
                                    break;
                                }
                            }

                            if (!startOfJsonFound) return tree;

                            break;
                        }
                    }
                }

                //var jsonData = reader.ReadToEnd();

                using (var jsonReader = new JsonTextReader(reader))
                {
                    try
                    {
                        //var json = JObject.Parse(jsonData);

                        var json = JObject.Load(jsonReader);

                        Read(tree, json);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);

                        var remaining = reader.ReadToEnd();

                        Console.WriteLine(remaining);
                    }
                }
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
                switch (prop.Value.Type)
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