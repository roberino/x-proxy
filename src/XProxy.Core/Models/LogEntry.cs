using LinqInfer.Data.Remoting;
using System;
using System.Linq;
using System.Xml;

namespace XProxy.Core.Models
{
    public class LogEntry
    {
        public LogEntry SetOrder(float order)
        {
            Order = order;
            return this;
        }

        public float Order { get; private set; }
        public string RemoteAddress { get; private set; }
        public string Id { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan Elapsed { get; private set; }
        public string HttpVerb { get; private set; }
        public string MimeType { get; private set; }
        public int Status { get; private set; }
        public Uri RefererUrl { get; private set; }
        public Uri OriginUrl { get; private set; }
        public string OriginPath
        {
            get
            {
                if (OriginUrl != null)
                {
                    return OriginUrl.PathAndQuery.Split('?').FirstOrDefault();
                }
                return null;
            }
        }
        public string OriginHost
        {
            get
            {
                if (OriginUrl != null)
                {
                    return OriginUrl.Host;
                }
                return null;
            }
        }
        public long RequestSize { get; private set; }
        public long ResponseSize { get; private set; }
        public double ElapsedMilliseconds { get { return Elapsed.TotalMilliseconds; } }

        public static LogEntry Parse(string recordData)
        {
            var parts = recordData.Split('\t');

            return new LogEntry()
            {
                RemoteAddress = parts[0],
                Id = parts[1],
                Date = XmlConvert.ToDateTime(parts[2], XmlDateTimeSerializationMode.Utc),
                Elapsed = TimeSpan.Parse(parts[3]),
                HttpVerb = parts[4],
                Status = int.Parse(parts[5]),
                OriginUrl = new Uri(parts[6]),
                RequestSize = long.Parse(parts[7]),
                ResponseSize = long.Parse(parts[8]),
                MimeType = parts.Length > 9 ? parts[9] : null,
                RefererUrl = parts.Length > 10 ? TryParseUri(parts[10]) : null
            };
        }

        public static string Format(RequestContext requestContext)
        {
            var head = requestContext.OwinContext.Request.Header;
            var res = requestContext.OwinContext.Response;

            string[] remoteAddr;

            if (!head.Headers.TryGetValue("REMOTE_ADDR", out remoteAddr))
            {
                remoteAddr = new string[0];
            }

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                remoteAddr.FirstOrDefault(),
                requestContext.Id,
                XmlConvert.ToString(res.Header.Date, XmlDateTimeSerializationMode.Utc),
                requestContext.Elapsed,
                head.HttpVerb,
                res.Header.StatusCode.GetValueOrDefault(0),
                requestContext.OriginUrl,
                head.ContentLength,
                GetContentLength(requestContext),
                GetMime(requestContext),
                GetReferer(requestContext));
        }

        private static string GetContentLength(RequestContext requestContext)
        {
            string[] val;
            if (requestContext.OwinContext.Response.Header.Headers.TryGetValue("Content-Length", out val) && val.Length > 0) return val.First();
            return "0";
        }

        private static Uri TryParseUri(string uri)
        {
            Uri u;

            if (!string.IsNullOrEmpty(uri) && Uri.TryCreate(uri, UriKind.Absolute, out u)) return u;

            return null;
        }

        private static string GetMime(RequestContext requestContext)
        {
            var rh = requestContext.OwinContext.Response.Header;
            var mimeType = rh.ContentMimeType;

            if (mimeType != null) return mimeType;

            string[] mimeTypeHeader;

            if (rh.Headers.TryGetValue("Content-Type", out mimeTypeHeader))
            {
                mimeType = mimeTypeHeader.FirstOrDefault();

                if (mimeType != null) return mimeType.Split(';').FirstOrDefault().Trim();
            }

            return null;
        }

        private static Uri GetReferer(RequestContext requestContext)
        {
            string[] referer;

            if (requestContext.OwinContext.Request.Header.Headers.TryGetValue("Referer", out referer))
            {
                var referer1 = referer.FirstOrDefault();

                if (referer1 != null)
                {
                    try
                    {
                        if (!referer1.Contains(UriHelper.SchemeDelimiter))
                        {
                            return new Uri(requestContext.OriginUrl, referer1);
                        }
                        else
                        {
                            return new Uri(referer1);
                        }
                    }
                    catch { }
                }
            }

            return null;
        }
    }
}