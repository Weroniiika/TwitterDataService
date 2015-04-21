using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using TwitterWCFService.TweetModel;
using Autofac;
using System.Reflection;
using System.Data.SqlClient;
using NLog;

namespace TwitterWCFService
{
    public class TwitterService : ITwitterService
    {
        private TweetDbContext dbContext = new TweetDbContext();
        private static Logger logger = LogManager.GetLogger("TwitterService");

        /// <summary>
        /// Initializes process of tweets collection in separate tasks and returns RequestInfo object as a confirmation that initailization succeeded.
        /// It throws FaultException with StreamFaultInfo in case if initialization failed.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public RequestInfo InitStreamAndSaveData(RequestInfo data)
        {
            try
            {
                logger.Info("============= Request received from client. Process of getting tweets from Twitter started. ================== ");
                TwitterStreamData tsd = new TwitterStreamData(data);
                EventWaitHandle readerStarted = new EventWaitHandle(false, EventResetMode.ManualReset);
                Exception exQueueTweets = null;
                Task.Run(() => tsd.QueueTweets(readerStarted, out exQueueTweets));
                bool signaled = readerStarted.WaitOne(5000);
                if (signaled)
                {
                    if (exQueueTweets == null)
                    {
                        Task.Run(() => tsd.ParseAndInsertTweetsToDB());
                    }
                    else
                    {
                        //throw new Exception("Queueing tweets failed.");
                        throw exQueueTweets;
                    }
                }
                else
                {
                    throw new TimeoutException("Timeout in InitStreamAndSaveData method waiting for task runing TwitterStreamData.QueueTweets method to signal that stream was established and reader has started.");
                }
                return tsd.RequestInfo();
            }
            catch(Exception ex)
            {
                logger.Error(String.Format("Exception while processing request from client; method name: {0}", MethodBase.GetCurrentMethod().Name), ex);
                throw new FaultException<StreamFaultInfo>(
                    new StreamFaultInfo() { 
                        Description = ex.Message, 
                        RequestInfo = data }, 
                new FaultReason(ex.Message + ((ex.InnerException == null)?"":ex.InnerException.Message)));
            }
        }

        /// <summary>
        /// Returns HashtagsCountData object with information about request and list of HashtagData object that contain information about counts of each given hashtag.
        /// In case of exceptions FaultException is thrown.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public HashtagsCountData HashtagsFrequencyData(string code)
        {
            Request req;
            try
            {
                req = ValidateCodeArgAndGetRequest(code);
            }
            catch (Exception ex)
            {
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }

            List<HashtagData> hashtagsList;
            try
            {
                SqlParameter requestIdParam = new SqlParameter("@requestId", req.RequestId);
                hashtagsList = dbContext.Database.SqlQuery<HashtagData>("GetHashtagFreq @requestId", requestIdParam).ToList() as List<HashtagData> ;
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Exception when retrieving data from database; method name: {0}", MethodBase.GetCurrentMethod().Name), ex);
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }

            HashtagsCountData hashtagsData = new HashtagsCountData();
            setRequestData(req, hashtagsData);
            hashtagsData.Data = hashtagsList;
            return hashtagsData;
        }

        /// <summary>
        /// Returns PicturesData object with information about request and list of TweetPictureData object containing picture Url and tweet text and id.
        /// In case of exceptions FaultException is thrown.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public PicturesData Pictures(string code)
        {
            Request req;
            try
            {
                req = ValidateCodeArgAndGetRequest(code);
            }
            catch (Exception ex)
            {
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }

            List<TweetPictureData> pictureData = null;
            try
            {
                var tweets = dbContext.Tweets.Where(t => t.RequestId == req.RequestId).Select(t=> t.TweetId).ToList();

                pictureData = dbContext.Media.Where(m => tweets.Contains(m.TweetId))
                    .Join(dbContext.Tweets, media => media.TweetId, tweet => tweet.TweetId, (media, tweet) => new TweetPictureData { TweetId = tweet.TweetId, PictureURL = media.MediaUrl, Text = tweet.Text })
                    .Where(m => (m.PictureURL.ToLower().EndsWith(".png") || m.PictureURL.ToLower().EndsWith(".jpg")))
                    .ToList<TweetPictureData>();
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Exception when retrieving data from database; method name: {0}", MethodBase.GetCurrentMethod().Name), ex);
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }

            PicturesData picData = new PicturesData();
            setRequestData(req, picData);
            picData.Data = pictureData;
            return picData;
        }

        /// <summary>
        /// Returns SentimentData object with information about request and results of sentiment semi-analysis. See SentimentAnalyzer class to get more info.
        /// In case of exceptions FaultException is thrown.</summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public SentimentData Sentiment(string code)
        {
            Request req;
            try
            {
               req = ValidateCodeArgAndGetRequest(code);
            }
            catch (Exception ex)
            {
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }
           
            SentimentData sentimentData = new SentimentData();
            try
            {
                var tweets = dbContext.Tweets.Where(t => t.RequestId == req.RequestId).Select(t => t.Text).ToList();
                SentimentAnalyzer sa = SentimentAnalyzer.Instance;
                sa.GetResults(tweets, sentimentData);
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Exception when retrieving data from database; method name: {0}", MethodBase.GetCurrentMethod().Name), ex);
                throw new FaultException<string>(code, new FaultReason(ex.Message));
            }

            setRequestData(req, sentimentData);
            return sentimentData;
        }

        private Request ValidateCodeArgAndGetRequest(string code) {

            if (!(code.Length == 4))
            {
                throw new Exception("Invalid request code.");
            }
            else {
                List<Request> query = dbContext.Requests.Where(r => r.AccessCode.Equals(code)).ToList();
                if (query.Count == 0)
                {
                    throw new Exception("Request data for the given code was not found.");
                }
                else
                {
                    Request req = (Request)query.First();
                    return req;
                }
            }
        }

        private static void setRequestData(Request req, RequestData data)
        {
            data.Code = req.AccessCode;
            data.CreatedAt = req.CreatedAt;
            data.EndedAt = req.EndedAt;
            data.TrackWord = req.TrackExpr;
        }
    }
}
