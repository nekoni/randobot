using Newtonsoft.Json.Linq;

namespace WitAi.Models
{
    public class Entity
    {
        public string Metadata { get; set; }

        public JToken Value{get;set;}

        public double Confidence { get; set; }
    }
}
