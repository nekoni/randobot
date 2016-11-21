using System.Collections.Generic;
using Newtonsoft.Json;


namespace WitAi.Models
{
    public class ConverseResponse
    {
        public string Type { get; set; }

        public string Msg { get; set; }

        public string Action { get; set; }

        public Dictionary<string, List<Entity>> Entities { get; set; }

        public double Confidence { get; set; }

        [JsonIgnore]
        public MessageTypes TypeCode
        {
            get
            {
                switch (this.Type)
                {
                    case "merge":
                    {
                        return MessageTypes.Merge;
                    }
                    case "msg":
                    {
                        return MessageTypes.Msg;
                    }
                    case "action":
                    {
                        return MessageTypes.Action;
                    }
                    case "stop":
                    {
                        return MessageTypes.Stop;
                    }
                    default:
                    {
                        return MessageTypes.Stop;
                    }
                }
            }
        }
    }
}
