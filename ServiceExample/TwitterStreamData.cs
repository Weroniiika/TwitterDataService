using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using TwitterWCFService.TweetModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Reflection;
using NLog; 

namespace TwitterWCFService
{
    /// <summary>
    /// This class is used to parse Twitter Public Stream data and than save each and every tweet in relational database. 
    /// Not all tweet information is persisted, see TweetModel to get more information about what kind of data is saved.
    /// Two methods QueueTweets(EventWaitHandle readerStarted) and ParseAndInsertTweetsToDB() are intended to be used in separat tasks and operate simultanousely.
    /// </summary>
    public class TwitterStreamData
    {
        private static readonly string dbErrorText = "Exception while saving data to Db";
        private static Logger logger = LogManager.GetLogger("TwitterStreamData");
        private int numberOfSavedTweets = 0;
        private int numberOfTweetsReadFromStream = 0;
        private int numberOfTweetsAlreadyInDB = 0;
        private int parsingErrors = 0;
        private static int noMinutesForTweetCollection = 15;
        private bool allTweetsReceivedByReader = false; //flag information stating if BinaryReader received all tweets and put them in the rawTweets BlockingCollection; it means QueueTweets method ended processing;

        private HttpWebRequest request;
        private RequestInfo requestInfo;

        private BlockingCollection<string> rawTweets = new BlockingCollection<string>();
        private Queue<string> leftovers = new Queue<string>();
        private Dictionary<string, Hashtag> hashtagsDict = new Dictionary<string, Hashtag>();
        private TweetDbContext dbContext; 

        /// <summary>
        /// Class Constructor. Extracts RequestInfo data and initializes request field.
        /// </summary>
        public TwitterStreamData(RequestInfo data)
        {
            try
            {
                dbContext = new TweetDbContext();
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Initialization of TweetDbContext failed. Method name: {0};",MethodBase.GetCurrentMethod().Name), ex);
                throw;
            }

            requestInfo = data;
            string trackValue = requestInfo.TrackWord;
            try
            {
                requestInfo.Code = GenerateCODE();
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Request code generation failed. Method name: {0};", MethodBase.GetCurrentMethod().Name), ex);
                throw;
            }
            
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                request = TwitterRequestFactory.InitRequest(trackValue);
                timer.Stop();
                logger.Info(String.Format("Request with code = {0} created successfully in time: {1};", requestInfo.Code, timer.Elapsed));
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Exception when creating request; Method name: {0}", MethodBase.GetCurrentMethod().Name), ex);
                throw;
            }
        }

        /// <summary>
        /// Helper method used to generate random code that serves to identify a user request in the database
        /// </summary>
        /// <returns>Returns alphanumeric string of length 4</returns>
        private string GenerateCODE()
        {
            bool isInDb = false;
            string result = "";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            do
            {
                var random = new Random();
                result = new string(
                    Enumerable.Repeat(chars, 4)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
                if (dbContext.Requests.Where(r => r.AccessCode.Equals(result)) != null)
                    isInDb = true;
            }
            while (!isInDb);
            return result;
        }

        public RequestInfo RequestInfo()
        {
            return requestInfo;
        }

        /// <summary>
        /// This method handles Twitter Public Stream data. It takes JSON format tweets, one by one, and adds them to rawTweets collection for further processing.
        /// </summary>
        /// <param name="streamStarted">This parameter is used to determine when streaming is begun. 
        /// Method ParseAndInsertTweetsToDB is started only when this parameter equals true, as it makes no sense to call that method when rawTweets collection is empty.</param>
        public void QueueTweets(EventWaitHandle readerStarted, out Exception exception)
        {
            try
            {
                exception = null;
                Stream stream = InitStream();
                allTweetsReceivedByReader = ReadStream(stream, readerStarted);
            }
            catch (Exception ex)
            {
                exception = ex;
                readerStarted.Set();
            }
        }

        public Stream InitStream()
        {
            Stopwatch responseStreamTimer = Stopwatch.StartNew();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                responseStreamTimer.Stop();
                logger.Info(String.Format("Response stream initialized successfully in time: {0};", responseStreamTimer.Elapsed));
                return stream;
            }
            catch (Exception ex)
            {   
                logger.Error("Exception while getting response from Twitter and establishing stream;", ex);
                throw;
            }
        }

        public bool ReadStream(Stream stream, EventWaitHandle readerStarted)
        {
            Stopwatch readerTimer = Stopwatch.StartNew();
            try
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    readerStarted.Set();
                    //notify TwitterService that the reader started and ParseAndInsertTweetsToDB() method can be called as a task
                    StringBuilder lengthChar = new StringBuilder();
                    Stopwatch timer = Stopwatch.StartNew();
                    for (int twittCounter = 0; twittCounter < requestInfo.NumberTweets; twittCounter++)
                    {
                        lengthChar.Length = 0;

                        //get the size of the tweet
                       
                        char readChar = reader.ReadChar();
                        while (!(readChar.Equals('\n') || readChar.Equals('\r')))
                        {
                            lengthChar.Append(readChar);
                            readChar = reader.ReadChar();
                        }
                        //following conditional statement ensures proper handling of extra empty lines between tweets
                        if (lengthChar.Length == 0)
                        {
                            twittCounter--;
                            continue;
                        }

                        //according to Twitter documentation number indicating size of the tweet is followed by an empty line
                        SkipEmptyLine(reader);

                        //get array of bytes with Tweet data and read it to JSON format Tweet string
                        int length = Int16.Parse(lengthChar.ToString());
                        byte[] twittBuffer = new byte[length];
                        twittBuffer = reader.ReadBytes(length);
                        string tweet = System.Text.Encoding.ASCII.GetString(twittBuffer);

                        AddJSONTweetToRawTweets(tweet);
                        numberOfTweetsReadFromStream++;
                        
                        //stop tweet collection if the time limit was reached
                        if (!timer.IsRunning || !(timer.Elapsed.TotalMinutes < noMinutesForTweetCollection)) {
                            timer.Stop();
                            break;
                        }
                    }
                    if (timer.IsRunning)
                        timer.Stop();
                }
                logger.Info(String.Format("Reading of stream and queueing tweets successfully ended in time: {0}; number of tweets read: {1};", readerTimer.Elapsed, numberOfTweetsReadFromStream));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Exception while reading stream from Twitter. Number of tweets read: {0}; Method name: {1};", numberOfTweetsReadFromStream, MethodBase.GetCurrentMethod().Name),ex);
                return false;
            }
            finally
            {
                rawTweets.CompleteAdding();
                readerTimer.Stop();
                stream.Close();
            }
        } 

        /// <summary>
        /// This helper method skips empty lines in TwitterStream. According to Twitter documentation empty lines are indicated by '\r\n' string. <see cref="https://dev.twitter.com/docs/streaming-apis/processing"/>
        /// If the first char read by the method form the stream is '\r', the method advances by one char more, assuming it will be '\n'.
        /// </summary>
        /// <param name="reader">Twitter Public Stream reader</param>
        private void SkipEmptyLine(BinaryReader reader)
        {
            char readChar = reader.ReadChar();
            if (readChar.Equals('\r'))
            {
                readChar = reader.ReadChar();
            }
            Debug.Assert(readChar.Equals('\n'));
        }

        /// <summary>
        /// This method goes through rawTweets BlockingCollection and takes tweet JSON strings, one by one,
        /// processes them and saves in the DB until the streaming is ended and rawTweets collection is empty.
        /// </summary>
        public void ParseAndInsertTweetsToDB()
        {
            int requestId;
            try 
            {
               requestId = SaveRequestData(); 
            }
            catch
            {
                logger.Error("{0}: Parsing and saving Tweets was not started. RequestId could not be obtained from DB", MethodBase.GetCurrentMethod().Name);
                return;
            }
            
            while (!rawTweets.IsCompleted)
            {
                string t;
                if (rawTweets.TryTake(out t, 10))
                {
                    Tweet tweet = new Tweet();
                    tweet.TweetId = 0;
                    Place place = null;
                    User user = null;
                    List<Media> mediaList = null;

                    try
                    {
                        dynamic jTweet = JValue.Parse(t);
                        //JObject newTweet = jTweet as JObject;
                        tweet = jTweet.ToObject<Tweet>();
                        tweet.TweetId = long.Parse((string)jTweet.id_str);
                        tweet.RequestId = requestId;
                        tweet.CreatedAt = ParseTwitterDateTime((string)jTweet.created_at);

                        //Find method searches first in the current DBContext (here: TweetDbContext) and only if it does not find tweet there, it checks in the DB.
                        if (dbContext.Tweets.Find(tweet.TweetId, tweet.RequestId) == null)
                        {
#region Parse Tweet
                            ParseRetweetStatus(tweet, jTweet);

                            ParseTweetCoordinates(tweet, jTweet);

                            user = ParseAndAddTweetUserToDbCont(tweet, jTweet);

                            place = ParseAndAddTweetPlaceToDbCont(tweet, jTweet);

                            mediaList = ParseAndAddTweetMediaToDbCont(tweet, jTweet);

                            ParseAndSaveHashtagsToDb(tweet, jTweet);
#endregion
                        }
                        else
                        {
                            //This code should be only called in case of retweets. 
                            numberOfTweetsAlreadyInDB++;
                         }
                    }                                               
                    catch (Exception ex)
                    {
                        parsingErrors++;
                        //log ex
                        logger.Error(String.Format("Error while parsing tweet. Full tweet string: {0}", t), ex);

                        //clean up other entities if tweet was not successfully added, otherwise mark this tweet data as parsed with errors
                        if (dbContext.Entry(tweet).State == EntityState.Detached)
                        {
                            if (!(user == null) && !(dbContext.Entry(user).State == EntityState.Detached))
                                dbContext.Users.Remove(user);

                            if (!(place == null) && !(dbContext.Entry(place).State == EntityState.Detached))
                                    dbContext.Places.Remove(place);
                            
                            if (!(mediaList == null) && mediaList.Count > 0)
                            {
                                foreach (Media media in mediaList)
                                {
                                    if (!(dbContext.Entry(media).State == EntityState.Detached))
                                    {
                                        dbContext.Media.Remove(media);
                                    }
                                }
                            }
                        
                        }
                        else
                        {
                            dbContext.Tweets.Find(tweet.TweetId, tweet.RequestId).ParsedWithErrors = true;
                        }
                    }
                }
                else
                {
                    if (allTweetsReceivedByReader)
                    {
                        rawTweets.CompleteAdding(); //This sets rawTweets.IsComplete to true, so the current while loop will end in the next iteration.
                    }
                }                
            }

            var request = dbContext.Requests.Find(requestId);
            request.Succeded = (numberOfSavedTweets == numberOfTweetsReadFromStream) ;
            request.NumberTweetsSaved = numberOfSavedTweets;
            request.NumberParsingErrors = parsingErrors;
            request.EndedAt = DateTime.Now;
           
            try
            {
                SaveChangesToTweetDb(MethodBase.GetCurrentMethod().Name);
                logger.Info(String.Format("All tweets from the queue were processed and saved in database with {0} parsing errors. Number of tweets processed: {1}", parsingErrors, numberOfSavedTweets));
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("{0}; Request ID = {1}; Method name: {2};", dbErrorText, requestId.ToString(),  MethodBase.GetCurrentMethod().Name), ex);
            }
            dbContext.Dispose();
                        
            //Assert statments to verify weather each and every tweet was processed
            #if (DEBUG)
                Debug.Assert(numberOfSavedTweets + numberOfTweetsAlreadyInDB == requestInfo.NumberTweets);
                Debug.Assert(leftovers.Count == 0);
            #endif
        }

        public void ParseTweetCoordinates(Tweet tweet, dynamic jTweet)
        {
            JArray coordinates = jTweet.coordinates as JArray;
            if (coordinates != null)
            {
                tweet.Longitude = (float)coordinates[0];
                tweet.Latitude = (float)coordinates[1];
            }
        }

        public User ParseAndAddTweetUserToDbCont(Tweet tweet, dynamic jTweet)
        {
            JObject newUser = jTweet.user as JObject;
            User user = newUser.ToObject<User>();
            user.UserId = long.Parse((string)jTweet.user.id_str);
            tweet.UserId = user.UserId;
            if (dbContext.Users.Find(user.UserId) == null)
            {
                dbContext.Users.Add(user);
            }
            else
            {
                logger.Info("check");
            }
            return user;
        }

        public Place ParseAndAddTweetPlaceToDbCont(Tweet tweet, dynamic jTweet) {
            JObject newPlace = jTweet.place as JObject;
            Place place = null;
            if (newPlace != null)
            {
                place = newPlace.ToObject<Place>();
                tweet.PlaceId = place.PlaceId;
                if (dbContext.Places.Find(place.PlaceId) == null)
                    dbContext.Places.Add(place);
            }
            return place;
        }

        public List<Media> ParseAndAddTweetMediaToDbCont(Tweet tweet, dynamic jTweet)
        {
            JArray mediaArray = jTweet.entities.media as JArray;
            List<Media> mediaList = new List<Media>();
            if (mediaArray != null && mediaArray.HasValues)
            {
                foreach (JToken jtMedia in mediaArray)
                {
                    Media media = jtMedia.ToObject<Media>();
                    media.TweetId = tweet.TweetId;
                    media.RequestId = tweet.RequestId;
                    mediaList.Add(media);
                    dbContext.Media.Add(media);
                }
            }
            else
            {
                mediaList = null;
            }
            return mediaList;
        }

        public void ParseAndSaveHashtagsToDb(Tweet tweet, dynamic jTweet)
        {
            JArray hashtagsArray = jTweet.entities.hashtags as JArray;
            if (hashtagsArray != null && hashtagsArray.HasValues)
            {
                AddTweetHashtagsToDbAndSave(hashtagsArray, tweet);
            }
            else
            {
                dbContext.Tweets.Add(tweet);
                numberOfSavedTweets++;
            }
        }

        public void ParseRetweetStatus(Tweet tweet, dynamic jTweet)
        {
            JObject rTweet = jTweet.retweeted_status;
            if (rTweet != null)
            {
                string r = rTweet.ToString();
                tweet.RetweetInfoId = long.Parse((string)jTweet.retweeted_status.id_str);
                //AddJSONTweetToRawTweets(r); //delete???
            }
        }

        /// <summary>
        /// Initializes new Request object and saves basic information to database.
        /// </summary>
        /// <returns>Returns new RequestId or 0 if saving to db did not succeed</returns>
        private int SaveRequestData()
        {
            Request tweetReq = new Request();
            tweetReq.AccessCode = requestInfo.Code;
            tweetReq.CreatedAt = DateTime.Now;
            tweetReq.NumberTweetsRequested = requestInfo.NumberTweets;
            tweetReq.TrackExpr = requestInfo.TrackWord;

            dbContext.Requests.Add(tweetReq);
            SaveChangesToTweetDb(MethodBase.GetCurrentMethod().Name);
            return tweetReq.RequestId;
        }

        /// <summary>
        /// Helper method that updates database with current context state and logs basic info.
        /// </summary>
        /// <param name="methodName"></param>
        private void SaveChangesToTweetDb(string methodName)
        {
            Stopwatch timer = Stopwatch.StartNew();
            dbContext.SaveChanges();
            timer.Stop();
            logger.Info(String.Format("Data saved in database successfully in method: {0}, in time: {1};", methodName, timer.Elapsed));
        }

        /// <summary>
        /// This helper method processes all of the tweets hashtags, one by one. It uses hashtagsDict to store hashtag objects that have been processed by current instance of TwitterStreamData. 
        /// When hashtags repeat during current stream processing there is no need to send data request to database to obtain primary key. This is necessary as there is many to many relation between tweets and hashtags.
        /// Also because of that TweetDbContext must call SaveChanges() method more frequently. It is the only way to assign primary key to hashtag object and hence process remaining hashtags consistently. 
        /// Considering the fact that many instances of application can be saving data to DB, it must be database that assigns primary key to hashtags  (to avoid using GlobalUniqIdentifier), not TwitterStreamData.
        /// </summary>
        /// <param name="hashtagsArray">A JArray of tweet's hashtags</param>
        /// <param name="tweet">Tweet object that is currently being processed</param>
        private void AddTweetHashtagsToDbAndSave(JArray hashtagsArray, Tweet tweet)
        {
            foreach (JToken jt in hashtagsArray)
            {
                Hashtag h = (((JObject)jt)).ToObject<Hashtag>();

                //check if this hashtag word occured already in this instance of TwitterStreamData.
                if (hashtagsDict.ContainsKey(h.HashtagWord))
                {
                    h = hashtagsDict[h.HashtagWord];
                  //  h = tweetDb.Hashtags.Find(h1.HashtagId);
                }
                else
                {
                    //check in db if the hashtag word has already been inserted
                    var dbRecord = dbContext.Hashtags.Where(record => record.HashtagWord.Equals(h.HashtagWord)).Take(1).ToList();
                    if (dbRecord.Count > 0)
                    {
                        h = dbRecord[0];
                    }
                }
                tweet.Hashtags.Add(h);
            }
            dbContext.Tweets.Add(tweet);

            try
            {
                SaveChangesToTweetDb(MethodBase.GetCurrentMethod().Name);
                numberOfSavedTweets++;
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("{0}; Request ID = {1}; Method name: {2};", dbErrorText, tweet.RequestId.ToString(), MethodBase.GetCurrentMethod().Name), ex);              
                return;  //method returns and hashtagsDict is not updated, as DB was not upadted
            }

            foreach (Hashtag h in tweet.Hashtags)
            {
                if (!hashtagsDict.ContainsKey(h.HashtagWord))
                    hashtagsDict.Add(h.HashtagWord, h);
            }
        }

        /// <summary>
        /// Adds JSON format Tweet string to BlockingCollection rawTweets. If for some reason JSON Tweet string cannot be added to the collection it is stored in leftovers queue, but only for debugging purposes. 
        /// </summary>
        /// <param name="t">JSON format Tweet string</param>
        /// <returns>Returns true if JSON Tweet string was added successfully to the rawTweets and false if there was a failure add JSON Tweet string was added to leftovers queue</returns>
        private bool AddJSONTweetToRawTweets(string t)
        {
            bool success = false;
            try
            {
                success = rawTweets.TryAdd(t, 2);
            }
            catch
            {
                leftovers.Enqueue(t);
            }

            if (!success)
            {
                //test and correctness control purpose. See end of ParseAndInsertTweetsToDB method for Debug.Assert statement.
                leftovers.Enqueue(t);
            }
            return success;
        }

        /// <summary>
        /// Helper method that converts Twitter datetime information string to .NET DateTime type and returns it.
        /// </summary>
        /// <param name="date">String parsed from JSON Tweet data attribute value "created_at". It stores information about date and time in following format: "ddd MMM dd HH:mm:ss +ffff yyyy" </param>
        /// <returns>The same information as passed parameter, but in standard .NET DateTime format.</returns>
        private DateTime ParseTwitterDateTime(string date)
        {
            const string format = "ddd MMM dd HH:mm:ss +ffff yyyy";
            DateTime output = DateTime.ParseExact(date, format, new System.Globalization.CultureInfo("en-US"));
            return output;
        }
    }
}
