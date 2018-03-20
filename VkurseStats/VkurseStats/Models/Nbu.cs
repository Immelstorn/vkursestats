using Newtonsoft.Json;

namespace VkurseStats.Models
{

    public  class Nbu
    {
        [JsonProperty("r030")]
        public long R030 { get; set; }

        [JsonProperty("txt")]
        public string Txt { get; set; }

        [JsonProperty("rate")]
        public string Rate { get; set; }

        [JsonProperty("cc")]
        public string Cc { get; set; }

        [JsonProperty("exchangedate")]
        public string Exchangedate { get; set; }
    }
}
