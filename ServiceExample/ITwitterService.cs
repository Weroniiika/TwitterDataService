using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TwitterWCFService
{
    [ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples")]
    public interface ITwitterService
    {
        [OperationContract]
        [FaultContract(typeof(StreamFaultInfo))]
        RequestInfo InitStreamAndSaveData(RequestInfo data);

        [OperationContract]
        [FaultContract(typeof(string))]
        HashtagsCountData HashtagsFrequencyData(string code);

        [OperationContract]
        [FaultContract(typeof(string))]
        PicturesData Pictures(string code);

        [OperationContract]
        [FaultContract(typeof(string))]
        SentimentData Sentiment(string code);

    }

    [DataContract]
    public class RequestInfo
    {   
        [DataMember]
        public int NumberTweets { get; set; }

        [DataMember]
        public string TrackWord { get; set; }

        [DataMember]
        public string Code { get; set; }
    }

    [DataContract]
    public class HashtagsCountData: RequestData
    {
        [DataMember]
        public List<HashtagData> Data { get; set; }
    }

    [DataContract]
    public class PicturesData: RequestData
    {
        [DataMember]
        public List<TweetPictureData> Data { get; set; }
    }

    [DataContract]
    public class SentimentData: RequestData
    {
        public SentimentData(){
            PositiveWordsList = new List<WordCount>();
            NegativeWordsList = new List<WordCount>();
        }

        [DataMember]
        public List<WordCount> PositiveWordsList { get; set; }

        [DataMember]
        public List<WordCount> NegativeWordsList { get; set; }

        [DataMember]
        public int PositiveWordsCount { get; set; }

        [DataMember]
        public int NegativeWordsCount { get; set; }
    }
    
    [DataContract]
    public class RequestData
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime EndedAt { get; set; }

        [DataMember]
        public string TrackWord { get; set; }
    }

    [DataContract]
    public class StreamFaultInfo
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public RequestInfo RequestInfo { get; set; }
    }

    public class HashtagData {
        public int HashtagId {get; set;}
        public string HashtagWord {get; set;}
        public int TweetCount {get; set;}
    }

    public class WordCount {
        public string EmotionWord {get; set;}
        public int Count {get; set;}
    }

    public class TweetPictureData {
        public long TweetId { get; set; }
        public string PictureURL { get; set; }
        public string Text { get; set; }
    }
}
