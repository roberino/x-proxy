using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LinqInfer.Data.Remoting;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core
{
    public class SessionStore : IHasHttpInterface
    {
        private readonly DirectoryInfo _baseDir;
        private readonly string _id;

        private SessionStore(DirectoryInfo baseDir, string id)
        {
            Contract.Assert(id != null);

            _id = id;
            _baseDir = new DirectoryInfo(Path.Combine(baseDir.FullName, _id));
            Label = _baseDir.Name.Substring(2);

            if (!_baseDir.Exists) _baseDir.Create();
        }

        private SessionStore(DirectoryInfo dir)
        {
            _baseDir = dir;
            _id = dir.Name;
            Label = _baseDir.Name.Substring(2);

            if (!_baseDir.Exists) _baseDir.Create();
        }

        public static IEnumerable<SessionStore> ListAllSessions(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists) return Enumerable.Empty<SessionStore>();
            var sessionDirs = baseDir.GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).Where(d => d.Name.StartsWith("x_")).ToList();
            return sessionDirs.Select(d => new SessionStore(d));
        }

        public static SessionStore CreateSessionStore(DirectoryInfo baseDir, bool createNew = false)
        {
            return createNew ? CreateNewSessionStore(baseDir) : FindLastSessionOrCreateNew(baseDir);
        }

        private static SessionStore CreateNewSessionStore(DirectoryInfo baseDir)
        {
            var mainDir = GetSessionFolderName(DateTime.UtcNow);
            var pattern = new Regex(mainDir.Replace("_0", "_(\\d+)"));

            var lastDir = baseDir.Exists ? baseDir.GetDirectories().Where(d => pattern.IsMatch(d.Name)).OrderByDescending(d => d.Name).FirstOrDefault() : null;

            if (lastDir != null)
            {
                var i = int.Parse(pattern.Match(lastDir.Name).Groups[1].Value) + 1;

                return new SessionStore(baseDir, mainDir.Replace("_0", "_" + i.ToString()));
            }

            return new SessionStore(baseDir, mainDir);
        }

        private static SessionStore FindLastSessionOrCreateNew(DirectoryInfo baseDir)
        {
            var lastSession = ListAllSessions(baseDir).FirstOrDefault();

            if (lastSession == null) return CreateSessionStore(baseDir, true);

            return lastSession;
        }

        public DateTime? LastWriteTime
        {
            get
            {
                var dir = _baseDir.GetDirectories().Where(d => !d.Name.StartsWith("_")).ToList();

                if (!dir.Any()) return null;

                return dir.Max(d => d.LastWriteTimeUtc);
            }
        }

        public DirectoryInfo BaseStorageDirectory { get { return _baseDir; } }

        public string Label { get; private set; }

        private static string GetSessionFolderName(DateTime timestamp, int version = 0)
        {
            return "x_" + timestamp.ToString("yyyy-MM-dd") + "_" + version;
        }

        public void Register(IHttpApi api)
        {
            api.Bind("/sessions").To(false, _ => Task.FromResult(new ResourceList<string>(ListAllSessions(_baseDir.Parent).Select(s => s.Label))));
        }
    }
}