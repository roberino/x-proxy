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

            if (context.OwinContext.Response.HasContent)
            {
                try
                {
                    if (JsonToTextTree.CanHandle(context.OwinContext.Response.Header.ContentMimeType))
                    {
                        response.Children["body"] = JsonToTextTree.Read(context.CreateContentReader());
                    }
                    else
                    {
                        if (HtmlToTextTree.CanHandle(context.OwinContext.Response.Header.ContentMimeType))
                        {
                            response.Children["body"] = HtmlToTextTree.Read(context.CreateContentReader());
                        }
                        else
                        {
                            if (PlainTextToTextTree.CanHandle(context.OwinContext.Response.Header.ContentMimeType))
                            {
                                response.Children["body"] = PlainTextToTextTree.Read(context.CreateContentReader());
                            }
                        }
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