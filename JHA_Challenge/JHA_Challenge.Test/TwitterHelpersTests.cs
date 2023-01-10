using JHA_Challenge.Models;
using JHA_Challenge.Models.Entity;
using System.Collections.Concurrent;

namespace JHA_Challenge.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CreateTwitterClient_SetsBearerToken()
        {
            //Arrange
            var bearerToken = "expectedToken";
            var expectedBearerTokenValue = "Bearer " + bearerToken;

            //Act
            var httpClient = TwitterHelpers.CreateTwitterClient(bearerToken);

            //Assert
            var actualBearerTokenValue = httpClient.DefaultRequestHeaders.Authorization;
            Assert.That(string.Equals(expectedBearerTokenValue, actualBearerTokenValue.ToString()));
        }

        [Test]
        public void CreateTwitterClient_SetsBaseUrl()
        {
            //Arrange
            var expectedBaseUrl = "https://api.twitter.com/2/tweets/sample/stream?tweet.fields=entities";
            var bearerToken = "expectedToken";

            //Act
            var httpClient = TwitterHelpers.CreateTwitterClient(bearerToken);

            //Assert
            Assert.That(string.Equals(httpClient.BaseAddress.AbsoluteUri, expectedBaseUrl));
        }

        [Test]
        public void CreateTwitterClient_FailsWhen_TokenIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => TwitterHelpers.CreateTwitterClient(null));
        }

        [Test]
        public void AggregateHashTags_Succeeds_WhenHashTagNotInDictionary()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();
            var hashTag = new EntityText();
            hashTag.Tag = "testTag";

            var tweetToParse = new Tweet();
            tweetToParse.Text = "tweetText";
            tweetToParse.Entities = new Models.Entity.Entities();
            tweetToParse.Entities.Hashtags = new EntityText[] { hashTag };

            //Act
            TwitterHelpers.AggregateHashtags(hashTags, tweetToParse);

            //Assert
            Assert.That(hashTags.Count, Is.EqualTo(1));
            Assert.That(hashTags[hashTag.Tag] == 1);
        }

        [Test]
        public void AggregateHashTags_Succeeds_WhenHashTagInDictionary()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();
            
            var hashTag = new EntityText();
            hashTag.Tag = "testTag";
            hashTags.TryAdd(hashTag.Tag, 2);

            var tweetToParse = new Tweet();
            tweetToParse.Text = "tweetText";
            tweetToParse.Entities = new Models.Entity.Entities();
            tweetToParse.Entities.Hashtags = new EntityText[] { hashTag };

            //Act
            TwitterHelpers.AggregateHashtags(hashTags, tweetToParse);

            //Assert
            Assert.That(hashTags.Count, Is.EqualTo(1));
            Assert.That(hashTags[hashTag.Tag] == 3);
        }

        [Test]
        public void AggregateHashTags_Fails_WhenDictionaryIsNull()
        {
            //Arrange
            var hashTag = new EntityText();
            hashTag.Tag = "testTag";

            var tweetToParse = new Tweet();
            tweetToParse.Text = "tweetText";
            tweetToParse.Entities = new Models.Entity.Entities();
            tweetToParse.Entities.Hashtags = new EntityText[] { hashTag };

            //Act/Assert
            Assert.Throws<ArgumentNullException>(() => TwitterHelpers.AggregateHashtags(null, tweetToParse));
        }

        [Test]
        public void AggregateHashTags_Fails_WhenTweetIsNull()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();

            var hashTag = new EntityText();
            hashTag.Tag = "testTag";
            hashTags.TryAdd(hashTag.Tag, 2);

            //Act/Assert
            Assert.Throws<ArgumentNullException>(() => TwitterHelpers.AggregateHashtags(hashTags, null));
        }

        [Test]
        public void GetTopHashTags_Succeeds()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();
            for(int i = 0; i < 20; i++)
            {
                hashTags.TryAdd($"number_{i}", i*2);
            }

            //Act
            var topHashTags = TwitterHelpers.GetTopHashtags(10, hashTags);

            //Assert
            Assert.That(topHashTags.Count, Is.EqualTo(10));
            Assert.That(topHashTags[0].Value == 38);
        }

        [Test]
        public void GetTopHashTags_Fails_WhenNumberToDisplayIsZero()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();
            for (int i = 0; i < 20; i++)
            {
                hashTags.TryAdd($"number_{i}", i * 2);
            }

            //Act/Assert
            Assert.Throws<ArgumentException>(() => TwitterHelpers.GetTopHashtags(0, hashTags));
        }

        [Test]
        public void GetTopHashTags_Fails_WhenDictionaryIsNull()
        {
            //Arrange//Act/Assert
            Assert.Throws<ArgumentNullException>(() => TwitterHelpers.GetTopHashtags(10, null));
        }

        [Test]
        public void GetTopHashTags_Succeeds_WhenNumberToDisplayGreaterThanHashTagCount()
        {
            //Arrange
            var hashTags = new ConcurrentDictionary<string, int>();
            for (int i = 0; i < 20; i++)
            {
                hashTags.TryAdd($"number_{i}", i * 2);
            }

            //Act
            var topHashTags = TwitterHelpers.GetTopHashtags(50, hashTags);

            //Assert
            Assert.That(topHashTags.Count, Is.EqualTo(20));
            Assert.That(topHashTags[0].Value == 38);
        }


    }
}