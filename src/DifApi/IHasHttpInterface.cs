using LinqInfer.Data.Remoting;

namespace DifApi
{
    public interface IHasHttpInterface
    {
        void Register(IHttpApi api);
    }
}
