using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;


namespace TwitterWCFService.TweetModel
{
    public class Media
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MediaId { get; set; }
        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }
        [JsonProperty("media_type")]
        public string MediaType { get; set; }
        [ForeignKey("Tweet"), Column(Order=0)]
        public long TweetId { get; set; }
        [ForeignKey("Tweet"), Column(Order=1)]
        public int RequestId { get; set; }
        public virtual Tweet Tweet { get; set; }  //usunąć?
    }
}
