using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterWCFService.TweetModel
{
    public class Request
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime CreatedAt { get; set; }
        [Column(TypeName = "DateTime2")]
        public DateTime EndedAt { get; set; }
        public int NumberTweetsRequested { get; set; }
        public int NumberTweetsSaved { get; set; }
        public int NumberParsingErrors { get; set; }
        [Index]
        [StringLength(4)]
        public string AccessCode { get; set; }
        public bool Succeded { get; set; }
        public string TrackExpr { get; set; }
        public string Location { get; set; }
    }
}
