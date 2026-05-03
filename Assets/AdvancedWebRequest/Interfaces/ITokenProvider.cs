using System.Threading;
using Cysharp.Threading.Tasks;

namespace AdvancedWebRequest.Core
{
    public interface ITokenProvider
    {
        UniTask<string> GetAccessTokenAsync(CancellationToken ct = default);
    }
}
