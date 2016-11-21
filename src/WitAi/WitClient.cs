using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WitAi.Models;
using WitAi.Utilities;

namespace WitAi
{
    public class WitClient
    {
        private readonly HttpClient client = new HttpClient();

        public WitClient(string token)
        {
            this.client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public string ApiVersion { get; set; } = "20160516";
        
        public int MaxSteps { get; set; } = 5;

        public int CallbackTimeout { get; set; } = 10000;

        public string WitApiUrl { get; set; } = "https://api.wit.ai";

        public async Task<Message> GetMessageAsync(string query, string messageId = null, string threadId = null)
        {
            var url = $"{this.WitApiUrl}/message?v={this.ApiVersion}&q={query}";
            if (messageId != null)
            {
                url+=$"&msg_id={messageId}";
            }
            if (threadId != null)
            {
                url+=$"&thread_id={threadId}";
            }

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            return MessageSerializer.Deserialize<Message>(json);
        }

        public async Task<ConverseResponse> ConverseAsync(string sessionId, string query = null, object context = null)
        {
            var payload = string.Empty;
            var url = $"{this.WitApiUrl}/converse?v={this.ApiVersion}&session_id={sessionId}";

            if (query != null)
            {
                url+=$"&q={query}";
            }
            if (context != null)
            {
                payload = MessageSerializer.Serialize(context);
            }

            var content = new StringContent(payload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
  
            var response = await client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            return MessageSerializer.Deserialize<ConverseResponse>(json);
        }
    }
}
