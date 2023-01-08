// See https://aka.ms/new-console-template for more information
using JHA_Challenge.Models;
using System.Collections.Concurrent;
using System.Text.Json;

Console.WriteLine("Sample Stream Tweet Characteristics");

//----Done-----
//Connect to Twitter
//Read a Tweet (does this happen on its own?)
//Get Hashtags
//Parse tweets from JSON into objects

//----To Do-----
//Store the tweet in memory somehow (add to a message queue?)
    //unit test for reading the stream?
//What if we add faster than we can remove?  Do we have a cap on the message queue size?
//pull the tweets from the queue
    //create some kind of hashtag hashmap at this point?  unit test
//put them in some data structure and remove them from the queue
    //repository pattern so we could swap in a database or something?
//run some numbers
    //number of tweets
    //top ten hashtags
        //convert the hashmap to a list so I can sort it and display it?  unit test
        //tag, count
//Don't block new tweets from being added

//How do I make this more abstract?  Some interface with generic methods for messaging queues?
ConcurrentQueue<Tweet> cq = new ConcurrentQueue<Tweet>();
ConcurrentDictionary<string, int> hashTags = new ConcurrentDictionary<string, int>();

//What other stats besides hashtags should I include?
int processedTweets = 0;

//Right now print results is also tabulating the hashtag stuff, and that could go in a separate function
//"thread1" isn't very descriptive.
Thread thread1 = new Thread(PrintResults);
thread1.Start();

//This should be abstracted somehow so that it isn't so tightly coupled to the queue object
void PrintResults () {
    while (true)
    {
        //This wait time should be configurable somewhere
        Task.Delay(5000).Wait();


        var hashTagList = hashTags.OrderByDescending(x => x.Value).ToList();
        
        Console.WriteLine($"Rank\tCount\tHashtag");
        if (hashTagList.Count >= 10)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{i + 1}\t{hashTagList[i].Value}\t{hashTagList[i].Key}");
            }
        }
        Console.WriteLine();

        Console.WriteLine($"Tweet Count is: {cq.Count}");
        Console.WriteLine($"Hashtag Count is: {hashTags.Count}");
        Console.WriteLine($"Processed Tweets Count is: {processedTweets}");
        
        Console.WriteLine();
    }
}

//This should be stored in appSettings or something
string bearerToken = "AAAAAAAAAAAAAAAAAAAAAO1VlAEAAAAAuiRHAOr30yDunXJJigOqeSy7%2Fjw%3DDZHccmxQKNYTFWyFVz6uWs9qNSVi7af6BNmpEdpaxCJHxYqlSn";


//----All this code could probably be in a Twitter Http Client class of some sort----
var httpClient = new HttpClient();
//The base address should be separated from the rest of the endpoint if we wanted to be more flexible about it
//The base address and endpoint address should be in appSettings, maybe
httpClient.BaseAddress = new Uri("https://api.twitter.com/2/tweets/sample/stream?tweet.fields=entities");
httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);

var httpRequest = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress);
var _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };




//// An action to consume the ConcurrentQueue.
///This could totally be some separate subscriber class or interface
Action action = () =>
{
    Tweet localTweet;

    while (cq.TryDequeue(out localTweet))
    {
        if (localTweet.Entities.Hashtags != null)
        {
            var outCount = 0;
            foreach (var hashTag in localTweet.Entities.Hashtags)
            {
                if (!hashTags.TryGetValue(hashTag.Tag, out outCount))
                {
                    hashTags.TryAdd(hashTag.Tag, 1);
                }
                else
                {
                    outCount++;
                    hashTags[hashTag.Tag] = outCount;
                }
            }
        }
        Interlocked.Add(ref processedTweets, 1);
    }
};


//This is the actual "main" code of the app
try
{
    var stream = await httpClient.GetStreamAsync(httpClient.BaseAddress);

    //There is no error handling around this
    //What if the stream goes down or something?
    using (var streamReader = new StreamReader(stream))
    {
        while (!streamReader.EndOfStream)
        {
            var currentLine = streamReader.ReadLine();
            if (string.IsNullOrEmpty(currentLine))
            {
                continue;
            }
            var currentRawData = JsonSerializer.Deserialize<RawResponse>(currentLine, _options);
            var currentTweet = JsonSerializer.Deserialize<Tweet>(currentRawData.Data, _options);
            cq.Enqueue(currentTweet);
            Parallel.Invoke(action);
        }
    }
}
catch (Exception ex)
{

}