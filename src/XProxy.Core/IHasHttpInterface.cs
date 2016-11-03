using LinqInfer.Data.Remoting;

namespace XProxy.Core
{
    public interface IHasHttpInterface
    {
        void Register(IHttpApi api);
    }
}
