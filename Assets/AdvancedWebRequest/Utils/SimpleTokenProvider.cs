using System.Threading;
using Cysharp.Threading.Tasks;

namespace AdvancedWebRequest.Core
{
    public class SimpleTokenProvider : ITokenProvider
    {
        private string _token;

        public SimpleTokenProvider(string token = null)
        {
            _token = token;
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public UniTask<string> GetAccessTokenAsync(CancellationToken ct = default)
        {
            return UniTask.FromResult(_token);
        }
    }
}
