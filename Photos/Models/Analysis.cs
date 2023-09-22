using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Analysis
{
    [JsonProperty("metadata")]
    public JObject Metadata { get; set; }

}