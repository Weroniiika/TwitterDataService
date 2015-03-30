using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterWCFService.TweetModel
{
    public class User
    {   
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public string Description { get; set; }
        [JsonProperty ("favourites_count")]
        public int FavouritesCount { get; set; }
        [JsonProperty ("followers_count")]
        public int FollowersCount { get; set; }
        [JsonProperty("friends_count")]
        public int FriendsCount { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        [JsonProperty ("lang")]
        public string InterfaceLang { get; set; }
        [JsonProperty("statuses_count")]
        public string TweetCount { get; set; }
        [JsonProperty ("time_zone")]
        public string TimeZone { get; set; }
    }
}
