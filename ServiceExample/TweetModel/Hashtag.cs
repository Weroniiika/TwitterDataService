using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterWCFService.TweetModel
{
    public class Hashtag
    {
        public Hashtag() {
            Tweets = new HashSet<Tweet>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HashtagId { get; set; }
        
        [JsonProperty("text")]
        [Index]
        [StringLength(400)] 
        public string HashtagWord { get; set; }

        public ICollection<Tweet> Tweets { get; set; }
    }
}
