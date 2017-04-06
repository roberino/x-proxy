using LinqInfer.Data.Remoting;

namespace XProxy.Core
{
    class RequestKeyGenerator
    {
        public string GetKey(IOwinContext context)
        {
            return context.RequestUri.PathAndQuery;
        }
    }
}
