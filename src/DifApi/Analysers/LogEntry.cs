using System;
using System.Linq;
using System.Xml;

namespace DifApi.Analysers
{
    public class LogEntry
    {
        public string RemoteAddress { get; private set; }
        public string Id { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan Elapsed { get; private set; }
        public string HttpVerb { get; private set; }
        public int Status { get; private set; }
        public Uri OriginUrl { get; private set; }
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
                ResponseSize = long.Parse(parts[8])
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

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                remoteAddr.FirstOrDefault(),
                requestContext.Id,
                XmlConvert.ToString(res.Header.Date, XmlDateTimeSerializationMode.Utc),
                requestContext.Elapsed,
                head.HttpVerb,
                res.Header.StatusCode.GetValueOrDefault(0),
                requestContext.OriginUrl,
                head.ContentLength,
                res.Header.Headers["Content-Length"].FirstOrDefault());
        }
    }
}
