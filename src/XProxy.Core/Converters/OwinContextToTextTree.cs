using System;
using System.Diagnostics;
using XProxy.Core.Models;

namespace XProxy.Core.Converters
{
    public static class OwinContextToTextTree
    {
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

            if (context.OwinContext.Response.HasContent && JsonToTextTree.CanHandle(context.OwinContext.Response.Header.ContentMimeType))
            {
                try
                {
                    if (context.RequestBlob.CanRead && context.RequestBlob.CanSeek)
                    {
                        context.RequestBlob.Position = 0;

                        response.Children["body"] = JsonToTextTree.Read(context.RequestBlob, context.OwinContext.Response.Header.TextEncoding);

                        context.RequestBlob.Position = 0;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            return root;
        }
    }
}