using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AdvancedWebRequest.Core
{
    public class RequestBuilder
    {
        private readonly ApiClient _client;
        private string _path;
        private string _method = "GET";
        private object _body;
        private float? _timeoutSeconds;
        private RetryPolicy _retryPolicy;
        private Dictionary<string, string> _headers;

        internal RequestBuilder(ApiClient client, string path)
        {
            _client = client;
            _path = path;
        }

        public RequestBuilder Method(string method)
        {
            _method = method.ToUpper();
            return this;
        }

        public RequestBuilder Get()
        {
            _method = "GET";
            return this;
        }

        public RequestBuilder Post()
        {
            _method = "POST";
            return this;
        }

        public RequestBuilder Put()
        {
            _method = "PUT";
            return this;
        }

        public RequestBuilder Delete()
        {
            _method = "DELETE";
            return this;
        }

        public RequestBuilder WithBody(object body)
        {
            _body = body;
            return this;
        }

        public RequestBuilder WithTimeout(float seconds)
        {
            _timeoutSeconds = seconds;
            return this;
        }

        public RequestBuilder WithRetry(RetryPolicy policy)
        {
            _retryPolicy = policy;
            return this;
        }

        public RequestBuilder NoRetry()
        {
            _retryPolicy = RetryPolicy.NoRetry;
            return this;
        }

        public RequestBuilder WithHeader(string key, string value)
        {
            _headers ??= new Dictionary<string, string>();
            _headers[key] = value;
            return this;
        }

        public async UniTask<T> SendAsync<T>(CancellationToken ct = default)
        {
            var options = new RequestOptions
            {
                TimeoutSeconds = _timeoutSeconds ?? 30f,
                RetryPolicy = _retryPolicy,
                AdditionalHeaders = _headers
            };

            return await _client.SendJsonAsync<T>(_path, _method, _body, ct, options);
        }

        public async UniTask SendAsync(CancellationToken ct = default)
        {
            await SendAsync<object>(ct);
        }
    }

    public static class ApiClientExtensions
    {
        public static RequestBuilder Request(this ApiClient client, string path)
        {
            return new RequestBuilder(client, path);
        }
    }
}
