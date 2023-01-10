using JHA_Challenge;
using JHA_Challenge.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Sample Stream Tweet Characteristics");

const string APP_SETTINGS_FILE = "appSettings.json";
const string TWITTER_BEARER_TOKEN_KEY = "TwitterBearerToken";
const string RESULTS_PRINTING_INTERVAL_KEY = "ResultsPrintingIntervalInMilliseconds";
const string HASHTAG_NUMBER_OF_TOP_TO_DISPLAY_KEY = "Hashtag_NumberToDisplay";

var configurationBuilder = new ConfigurationBuilder().AddJsonFile(APP_SETTINGS_FILE);
IConfiguration config = configurationBuilder.Build();

int resultIntervalInMs = Convert.ToInt32(config[RESULTS_PRINTING_INTERVAL_KEY]);
int numberOfTopHashtagsToDisplay = Convert.ToInt32(config[HASHTAG_NUMBER_OF_TOP_TO_DISPLAY_KEY]);

//----To Do-----
//Do we have a cap on the message queue size?
//repository pattern so we could swap in a database or something?

//Could replace this with any messaging system, like RabbitMQ, etc.
ConcurrentQueue<Tweet> cq = new ConcurrentQueue<Tweet>();
ConcurrentDictionary<string, int> hashTags = new ConcurrentDictionary<string, int>();

//Other Possible Stats:
//Number/Percentage of tweets with links
//Number/Percentage of tweets with hashtags
//Average number of hashtags per tweet
int processedTweets = 0;



/*------------------------------------------------------
 * Kick off a thread to handle the results reporting
 *----------------------------------------------------*/
Thread resultsReportingThread = new Thread(PrintResults);
resultsReportingThread.Start();

void PrintResults()
{
    while (true)
    {
        Task.Delay(resultIntervalInMs).Wait();

        var topHashTags = TwitterHelpers.GetTopHashtags(numberOfTopHashtagsToDisplay, hashTags);
        TwitterHelpers.WriteTopHashtagsToConsole(numberOfTopHashtagsToDisplay, topHashTags);

        //Console.WriteLine($"Queued Tweet Count is: {cq.Count}");
        Console.WriteLine($"Hashtag Count is: {hashTags.Count}");
        Console.WriteLine($"Processed Tweets Count is: {processedTweets}");

        Console.WriteLine();
    }
}

/*------------------------------------------------------
 * An action to consume the ConcurrentQueue
 *----------------------------------------------------*/
Action action = () =>
{
    Tweet localTweet;

    while (cq.TryDequeue(out localTweet))
    {
        TwitterHelpers.AggregateHashtags(hashTags, localTweet);
        Interlocked.Add(ref processedTweets, 1);
    }
};


try
{
    string bearerToken = config[TWITTER_BEARER_TOKEN_KEY];
    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    
    HttpClient httpClient = TwitterHelpers.CreateTwitterClient(bearerToken);
    var stream = await httpClient.GetStreamAsync(httpClient.BaseAddress);

    using (var streamReader = new StreamReader(stream))
    {
        while (!streamReader.EndOfStream)
        {
            var currentLine = streamReader.ReadLine();
            if (string.IsNullOrEmpty(currentLine))
            {
                continue;
            }
            var currentRawData = JsonSerializer.Deserialize<RawResponse>(currentLine, jsonOptions);
            var currentTweet = JsonSerializer.Deserialize<Tweet>(currentRawData.Data, jsonOptions);
            cq.Enqueue(currentTweet);

            //This could be used to call the action on multiple threads if needed, in case the reporting can't keep up with the enqueueing
            Parallel.Invoke(action);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}






