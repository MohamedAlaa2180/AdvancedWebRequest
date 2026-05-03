namespace AdvancedWebRequest.Core
{
    public class RequestSpec
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public object Body { get; set; }
        public RequestOptions Options { get; set; }
    }
}
