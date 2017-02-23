using LinqInfer.Text;
using System.Collections.Generic;
using System.Linq;

namespace XProxy.Core.Analysers
{
    public class TextTreeComparison
    {
        public TextTreeComparison()
        {
            Properties = new Dictionary<string, ComparisonInfo>();
            Children = new Dictionary<string, TextTreeComparison>();
        }

        public TextTreeComparison Compare(TextTree context1)
        {
            foreach (var keyValue in context1.Properties)
            {
                GetOrCreatePropertyNode(keyValue.Key).AddValueForComparison(keyValue.Value);
            }

            foreach (var child in context1.Children)
            {
                TextTreeComparison comparison;

                if (!Children.TryGetValue(child.Key, out comparison))
                {
                    Children[child.Key] = comparison = new TextTreeComparison();
                }

                comparison.Compare(child.Value);
            }

            return this;
        }

        public TextTreeComparison Compare(TextTree context1, TextTree context2)
        {
            return Compare(context1).Compare(context2);
        }

        public int TotalDiffs
        {
            get
            {
                return Properties.Values.Select(v => v.Differences).Sum() + Children.Values.Sum(c => c.TotalDiffs);
            }
        }

        public IDictionary<string, ComparisonInfo> Properties { get; private set; }

        public IDictionary<string, TextTreeComparison> Children { get; private set; }

        private ComparisonInfo GetOrCreatePropertyNode(string key)
        {
            ComparisonInfo x = null;

            if (!Properties.TryGetValue(key, out x))
            {
                Properties[key] = x = new ComparisonInfo();
            }

            return x;
        }

        private void IncrementKey(string key, ComparisonInfo newData)
        {
            ComparisonInfo x = null;

            if (Properties.TryGetValue(key, out x))
            {
                x.Differences += newData.Differences;
                x.Missing += newData.Missing;
                x.Score += ((x.Score * x.SampleSize) + newData.Score) / (double)(newData.SampleSize + 1);
                x.SampleSize += newData.SampleSize;
            }
            else
            {
                Properties[key] = newData;
            }
        }

        public class ComparisonInfo
        {
            public ComparisonInfo()
            {
                Values = new HashSet<string>();
            }

            public double Score { get; set; }

            public int Differences { get; set; }

            public int Missing { get; set; }

            public int SampleSize { get; set; }

            public HashSet<string> Values { get; set; }

            public void AddValueForComparison(string value)
            {
                if (value == null)
                {
                    Missing++;
                }
                else
                {
                    if (Values.Any())
                    {
                        if (!Values.Contains(value))
                        {
                            Differences++;

                            var s = 0d;

                            foreach (var val in Values)
                            {
                                s += value.ComputeLevenshteinDifference(val).Value;
                            }

                            s = s / Values.Count;

                            Score = ((Score * Values.Count) + s) / (Values.Count + 1);

                            Values.Add(value);
                        }
                        else
                        {
                            Score = (Score * Values.Count) / (Values.Count + 1);
                        }
                    }
                    else
                    {
                        Values.Add(value);
                    }
                }

                SampleSize++;
            }
        }
    }
}