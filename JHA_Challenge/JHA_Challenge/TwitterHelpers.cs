using JHA_Challenge.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHA_Challenge
{
    public static class TwitterHelpers
    {
        public static List<KeyValuePair<string, int>> GetTopHashtags(int hashTagsToDisplay, ConcurrentDictionary<string, int> hashTags)
        {
            return hashTags.OrderByDescending(x => x.Value).Take(hashTagsToDisplay).ToList();
        }

        public static HttpClient CreateTwitterClient(string bearerToken)
        {
            //----All this code could probably be in a Twitter Http Client class of some sort----
            var httpClient = new HttpClient();

            //The base address could be separated from the rest of the endpoint if we wanted to be more flexible about it
            //The base address and endpoint address could be in appSettings
            httpClient.BaseAddress = new Uri("https://api.twitter.com/2/tweets/sample/stream?tweet.fields=entities");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress);
            return httpClient;
        }

        public static void WriteTopHashtagsToConsole(int numberOfTopHashtagsToDisplay, List<KeyValuePair<string, int>> topHashTags)
        {
            Console.WriteLine($"Rank\tCount\tHashtag");
            if (topHashTags.Count >= numberOfTopHashtagsToDisplay)
            {
                for (int i = 0; i < numberOfTopHashtagsToDisplay; i++)
                {
                    Console.WriteLine($"{i + 1}\t{topHashTags[i].Value}\t{topHashTags[i].Key}");
                }
            }
            Console.WriteLine();
        }

        public static bool AggregateHashtags(ConcurrentDictionary<string, int> hashTags, Tweet tweetToParse)
        {
            try
            {
                if (tweetToParse.Entities.Hashtags != null)
                {
                    var aggregateCount = 0;
                    foreach (var hashTag in tweetToParse.Entities.Hashtags)
                    {
                        if (!hashTags.TryGetValue(hashTag.Tag, out aggregateCount))
                        {
                            hashTags.TryAdd(hashTag.Tag, 1);
                        }
                        else
                        {
                            aggregateCount++;
                            hashTags[hashTag.Tag] = aggregateCount;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //Production app would have this replaced with call to logging framework
                Console.WriteLine(ex.ToString());
                return false;
            }

        }
    }
}

