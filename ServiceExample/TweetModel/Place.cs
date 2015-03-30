using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TwitterWCFService.TweetModel
{
    public class Place
    {
        [Key]
        [JsonProperty("id")]
        public string PlaceId { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        public string Country { get; set; }
        [JsonProperty("place_type")]
        public string PlaceType { get; set; }
    }
}
