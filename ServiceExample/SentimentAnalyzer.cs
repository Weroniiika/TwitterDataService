using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace TwitterWCFService
{
    public class SentimentAnalyzer
    {
        private readonly List<string> negativeWordsDict;
        private readonly List<string> positiveWordsDict;

        private string negativDictFile = @"../../../negative-words.txt";
        private string positiveDictFile = @"../../../positive-words.txt";

        private Logger logger = LogManager.GetLogger("SentimentAnalyzer");
        private static readonly SentimentAnalyzer instance = new SentimentAnalyzer();

        private SentimentAnalyzer()
        {
            negativeWordsDict = ReadWordsFromFile(negativDictFile) as List<string>;
            positiveWordsDict = ReadWordsFromFile(positiveDictFile) as List<string>;
        }

        public static SentimentAnalyzer Instance
        {
            get
            {
                return instance;
            }
        }

        public void GetResults(IEnumerable<string> tweets, SentimentData data)
        {
            NegativeWordsFreq(tweets, data);
            PositiveWordsFreq(tweets, data);
        }

        private void NegativeWordsFreq(IEnumerable<string> tweets, SentimentData data)
        {
            if (data.NegativeWordsList == null)
                data.NegativeWordsList = new List<WordCount>();

            //GetTopWordsCounts not only returns number of all the sent words found, but also changes the list of the highest counts of words
            data.NegativeWordsCount = GetTopWordsCounts(tweets, negativeWordsDict, data.NegativeWordsList);
        }

        private void PositiveWordsFreq(IEnumerable<string> tweets, SentimentData data)
        {
            if (data.PositiveWordsList == null)
                data.PositiveWordsList = new List<WordCount>();

            //GetTopWordsCounts not only returns number of all the sent words found, but also changes the list of the highest words counts
            data.PositiveWordsCount = GetTopWordsCounts(tweets, positiveWordsDict, data.PositiveWordsList);
        }

        /// <summary>
        /// Returns total count of found dicationary words (positive or negative) in tweets collection. 
        /// At the same it modifies passed sentimentWordList, so it contains top 5 most frequent dictionary words and their counts.
        /// </summary>
        /// <param name="tweets"></param>
        /// <param name="dict"></param>
        /// <param name="sentimentWordList">This reference param is modified to contain top 5 most frequent dictionary words and their counts when the method finishes.</param>
        /// <returns></returns>
        private int GetTopWordsCounts(IEnumerable<string> tweets, IEnumerable<string> dict, List<WordCount> sentimentWordList)
        {
            List<WordCount> result = new List<WordCount>();
            Dictionary<string, int> wordCount = new Dictionary<string, int>();

            Regex rgx = new Regex("[^a-zA-Z0-9 +-]");
            int countTotal = 0;

            foreach (string text in tweets)
            {
                string words = rgx.Replace(text, " ");
                foreach (string word in words.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (dict.Contains(word))
                    {
                        int count = 0;
                        if (wordCount.TryGetValue(word, out count))
                        {
                            countTotal++;
                            wordCount[word] = wordCount[word] + 1;
                        }
                        else
                        {
                            wordCount.Add(word, 1);
                            countTotal++;
                        }
                    }
                }
            }

            List<WordCount> resultSorted = (from element in wordCount orderby element.Value descending select new WordCount() { EmotionWord = element.Key, Count = element.Value })
                                            .Take(5).ToList<WordCount>();

            foreach (var el in resultSorted)
            {
                sentimentWordList.Add(el);
            }
            return countTotal;
        }

        private IEnumerable<string> ReadWordsFromFile(string filePath)
        {
            List<string> words = new List<string>();
            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    words.Add(line);
                }
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Dictionary words could not be read from file {0}", filePath), ex);
                throw;
            }
            return words;
        }

    }
}