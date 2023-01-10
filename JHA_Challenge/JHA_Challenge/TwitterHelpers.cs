using JHA_Challenge.Models;
using System.Collections.Concurrent;

namespace JHA_Challenge
{
    public static class TwitterHelpers
    {
        public static List<KeyValuePair<string, int>> GetTopHashtags(int hashTagsToDisplay, ConcurrentDictionary<string, int> hashTags)
        {
            if (hashTagsToDisplay == 0) throw new ArgumentException();
            if (hashTags == null) throw new ArgumentNullException();
            return hashTags.OrderByDescending(x => x.Value).Take(Math.Min(hashTagsToDisplay, hashTags.Count)).ToList();
        }

        public static HttpClient CreateTwitterClient(string bearerToken)
        {
            if (bearerToken == null) throw new ArgumentNullException();
            var httpClient = new HttpClient();

            //The base address could be separated from the rest of the endpoint if we wanted to be more flexible about it
            //The base address and endpoint address could be in appSettings
            httpClient.BaseAddress = new Uri("https://api.twitter.com/2/tweets/sample/stream?tweet.fields=entities");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);

            //var httpRequest = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress);
            return httpClient;
        }

        public static void WriteTopHashtagsToConsole(int numberOfTopHashtagsToDisplay, List<KeyValuePair<string, int>> topHashTags)
        {
            if(numberOfTopHashtagsToDisplay == 0) throw new ArgumentException();
            if(topHashTags == null) throw new ArgumentNullException();
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
            if (tweetToParse == null) throw new ArgumentNullException();
            if (hashTags == null) throw new ArgumentNullException();
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

