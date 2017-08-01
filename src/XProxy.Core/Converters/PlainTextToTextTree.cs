using LinqInfer.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using XProxy.Core.Models;

namespace XProxy.Core.Converters
{
    class PlainTextToTextTree
    {
        public static bool CanHandle(string mime)
        {
            return mime.Contains("text/");
        }

        public static TextTree Read(TextReader reader)
        {
            var tree = new TextTree();

            int i = 0;

            while (true)
            {
                var next = reader.ReadLine();
                if (next == null) break;

                tree.Properties["#line" + (i++)] = next;
            }

            return tree;
        }
    }
}