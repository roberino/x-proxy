using System.IO;
using System.Threading.Tasks;

namespace XProxy.Core.Models
{
    public interface ICanPersist
    {
        Task WriteAsync(Stream output);
        Task ReadAsync(Stream input);
    }
}
