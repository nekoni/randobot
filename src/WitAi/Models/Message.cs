using System.Collections.Generic;
using Newtonsoft.Json;

namespace WitAi.Models
{
    public class Message : WitApiResponse
    {
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }

        [JsonProperty("_text")]
        public string Text { get; set; }

        public Dictionary<string, List<Entity>> Entities { get; set; }
    }
}
