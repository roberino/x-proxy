using LinqInfer.Maths;
using LinqInfer.Maths.Probability;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using XProxy.Core.Models;

namespace XProxy.Core.Analysers.Faults
{
    public class HypotheticalFault : ICanPersist
    {
        public HypotheticalFault(Uri url, Fraction? priorFaultProbability = null)
        {
            Url = url;
            IsFaulty = P.Hypothesis(true, priorFaultProbability.GetValueOrDefault((1).OutOf(100)));
            IsNotFaulty = P.Hypothesis(false, IsFaulty.PriorProbability.Invert());
            SetupHypos();
        }

        public Uri Url { get; private set; }

        public DateTime LastUpdated { get; private set; }

        public Hypothetheses<bool> Hypos { get; private set; }

        public IHypotheticalOutcome<bool> IsFaulty { get; private set; }

        public IHypotheticalOutcome<bool> IsNotFaulty { get; private set; }

        public async Task WriteAsync(Stream output)
        {
            var json = JsonConvert.SerializeObject(this);

            using (var writer = new StreamWriter(output))
            {
                await writer.WriteAsync(json);
            }
        }

        public async Task ReadAsync(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var json = await reader.ReadToEndAsync();

                var data = JsonConvert.DeserializeObject<dynamic>(json);

                Url = new Uri((string)data.Url);
                IsFaulty = P.Hypothesis(true, GetFraction(data.IsFaulty.PosteriorProbability));
                IsNotFaulty = P.Hypothesis(false, GetFraction(data.IsNotFaulty.PosteriorProbability));
                SetupHypos();
            }
        }

        private Fraction GetFraction(dynamic value)
        {
            var d = (int)value.Denominator;
            var n = (int)value.Numerator;
            return new Fraction(n, d);
        }

        private void SetupHypos()
        {
            Hypos = new[] { IsFaulty, IsNotFaulty }.AsHypotheses();
            Hypos.Updated += (s, e) =>
            {
                LastUpdated = DateTime.UtcNow;
            };
        }
    }
}