using System.Threading.Tasks;

namespace XProxy.Core.Jobs
{
    public abstract class ContinuousJob
    {
        public abstract Task Execute(ExecutionContext context);
    }
}
