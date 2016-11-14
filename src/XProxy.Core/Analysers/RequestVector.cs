using System;

namespace XProxy.Core.Analysers
{
    public class RequestVector
    {
        public Uri Url { get; set; }
        public bool IsJson { get; set; }
        public bool IsHtml { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCacheable { get; set; }
        public double SemanticTokenRatio { get; set; }
        public double NumericTokenRatio { get; set; }
        public int TokenCount { get; set; }
        public int UniqueRefererCount { get; set; }
        public int ErrorCount { get; set; }
        public int RequestCount { get; set; }
        public double ResponseSize { get; set; }
        public int PathCount { get; set; }
        public double ResponseTime { get; set; }
        public double VariabilityScore { get; set; }
    }
}