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
            foreach (var key in context1.Properties.Keys.Concat(context2.Properties.Keys).Distinct())
            {
                if (context1.Properties.ContainsKey(key) && context2.Properties.ContainsKey(key))
                {
                    if (!string.Equals(context1.Properties[key], context2.Properties[key]))
                    {
                        IncrementKey(key, new ComparisonInfo()
                        {
                            Differences = 1,
                            SampleSize = 1,
                            Score = context1.Properties[key].ComputeLevenshteinDifference(context2.Properties[key]).Value
                        });

                        GetOrCreatePropertyNode(key).Values.Add(context1.Properties[key]);
                        GetOrCreatePropertyNode(key).Values.Add(context2.Properties[key]);
                    }
                    else
                    {

                    }

                    continue;
                }

                IncrementKey(key, new ComparisonInfo() { Missing = 1, SampleSize = 1 });
            }

            foreach (var key in context1.Children.Keys.Concat(context2.Children.Keys).Distinct())
            {
                TextTreeComparison comparison;

                if (!Children.TryGetValue(key, out comparison))
                {
                    Children[key] = comparison = new TextTreeComparison();
                }

                if (context1.Children.ContainsKey(key) && context2.Children.ContainsKey(key))
                {
                    comparison.Compare(context1.Children[key], context2.Children[key]);
                }
            }

            return this;
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
                if (Values.Any() && !Values.Contains(value))
                {
                    Differences++;

                    var s = 0d;

                    foreach(var val in Values)
                    {
                        s += value.ComputeLevenshteinDifference(val).Value;
                    }

                    s = s / Values.Count;

                    Score = ((Score * Values.Count) + s) / (Values.Count + 1);

                    Values.Add(value);
                }

                SampleSize++;
            }
        }
    }
}