using LinqInfer.Data.Remoting;
using System;
using System.Linq;
using System.Reflection;

namespace XProxy.Core
{
    public class WebPortal : HttpAppBase
    {
        private readonly Assembly _asm;

        public WebPortal(Uri host) : base(host)
        {
            _asm = Assembly.GetExecutingAssembly();
        }

        protected override void Setup(IOwinApplication host)
        {
            base.Setup(host);

            host.AddComponent(async c =>
            {
                var relPath = c.RequestUri.PathAndQuery == "/" ? ".index.html" : c.RequestUri.PathAndQuery.Replace('/', '.');
                var path = GetType().Namespace + ".wwwroot.app" + relPath;
                var names = _asm.GetManifestResourceNames();
                var name = names.FirstOrDefault(n => DashAgnosticMatch(n, path));

                using (var resource = _asm.GetManifestResourceStream(name ?? path))
                {
                    if (resource == null)
                    {
                        c.Response.CreateStatusResponse(404);
                    }
                    else
                    {
                        var ext = relPath.Split('.').Last().Split('?').First();

                        c.Response.Header.Headers["Cache-Control"] = new [] { "private, max-age=15000" };
                        c.Response.Header.ContentMimeType = MapMime(ext);

                        await resource.CopyToAsync(c.Response.Content);
                    }
                }
            });
        }

        private bool DashAgnosticMatch(string s1, string s2)
        {
            if (s1 == null || s2 == null || s1.Length != s2.Length) return false;
            return s1.Replace('_', '-').Equals(s2.Replace('_', '-'));
        }

        private string MapMime(string ext)
        {
            switch (ext)
            {
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";
                case "json":
                    return "application/json";
                case "html":
                    return "text/html";
                case "woff":
                case "woff2":
                    return "application/font-woff";
                case "ttf":
                    return "application/font-sfnt";
                default:
                    return "text/plain";
            }
        }
    }
}