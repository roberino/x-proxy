using LinqInfer.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using XProxy.Core.Models;

namespace XProxy.Core.Converters
{
    class HtmlToTextTree
    {
        public static bool CanHandle(string mime)
        {
            return mime.Contains("html");
        }

        public static TextTree Read(TextReader reader)
        {
            var data = ((StreamReader)reader).BaseStream;
            var xnodes = TextExtensions.OpenAsHtml(data, ((StreamReader)reader).CurrentEncoding);
            var tree = new TextTree();

            AppendNodes(tree, xnodes);

            return tree;
        }

        private static void AppendNodes(TextTree tree, IEnumerable<XNode> xnodes)
        {
            int i = 0;

            foreach (var xnode in xnodes)
            {
                switch (xnode.NodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        {

                            foreach (var attr in ((XElement)xnode).Attributes())
                            {
                                tree.Properties[attr.Name.LocalName] = attr.Value;
                            }
                        }
                        break;
                    case System.Xml.XmlNodeType.Text:
                        {
                            tree.Properties["#text" + (i++)] = ((XText)xnode).Value;
                        }
                        break;
                }
            }
        }
    }
}