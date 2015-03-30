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
    public class Tweet
    {
        public Tweet()
        {
            Hashtags = new HashSet<Hashtag>();
            //Medias = new List<Media>();
        }

        [Key, Column(Order=0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TweetId { get; set; }
        [Key, Column(Order=1)]
        public int RequestId { get; set; } //request for data number
        public long UserId { get; set; }
        public String PlaceId { get; set; }
        public long? RetweetInfoId { get; set; }
        public string Text { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        [Column(TypeName="DateTime2")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("favorite_count")]
        public int FavoriteCount { get; set; }
        [JsonProperty("retweet_count")]
        public int RetweetCount { get; set; }
        public bool ParsedWithErrors { get; set; }
        public virtual ICollection<Hashtag> Hashtags { get; set; }
        //public virtual ICollection<Media> Medias { get; set; }
    }
}
