using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TwitterWCFService.TweetModel
{
    public class TweetHashtags
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Tweet")]
        public long TweetId { get; set; }
        public virtual Tweet Tweet { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Hashtag")]
        public int HashtagId { get; set; }
        public virtual Hashtag Hashtag { get; set; }
    }
}