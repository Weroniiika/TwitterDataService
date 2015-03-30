using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace TwitterWCFService.TweetModel
{
    public class TweetDbContext: DbContext
    {
         public  TweetDbContext(): base("TwitterData")
        {
            Database.SetInitializer<TweetDbContext>(new DropCreateDatabaseIfModelChangesRegisterProcedure());
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<Media>  Media { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Request> Requests { get; set; }
    }

    public class DropCreateDatabaseIfModelChangesRegisterProcedure : DropCreateDatabaseIfModelChanges<TweetDbContext>
    {
        protected override void Seed(TweetDbContext context)
        {

            string procedure = @"Create procedure GetHashtagFreq @requestId int as Begin
	                                select distinct h.HashtagId, h.HashtagWord, Count(h.HashtagId) as TweetCount
	                                from Hashtags h join TweetHashtags th on h.HashtagId = th.Hashtag_HashtagId
	                                where th.Tweet_RequestId = @requestId
	                                Group by h.HashtagWord, h.HashtagId
	                                Order by TweetCount desc
                                end";
            context.Database.ExecuteSqlCommand(procedure);
        }
    }

//    public class DropCreateDatabaseAlwaysRegisterProcedure : DropCreateDatabaseAlways<TweetDbContext>
//    {
//        protected override void Seed(TweetDbContext context)
//        {

//            string procedure = @"Create procedure GetHashtagFreq @requestId int as Begin
//	                                select distinct h.HashtagId, h.HashtagWord, Count(h.HashtagId) as TweetCount
//	                                from Hashtags h join TweetHashtags th on h.HashtagId = th.Hashtag_HashtagId
//	                                where th.Tweet_RequestId = @requestId
//	                                Group by h.HashtagWord, h.HashtagId
//	                                Order by TweetCount desc
//                                end";
//            context.Database.ExecuteSqlCommand(procedure);
//        }
//    }
}
