using System.Collections.Generic;

namespace AdvancedWebRequest.Core
{
    public class ApiErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }
}
