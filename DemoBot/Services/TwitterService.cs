using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace DemoBot.Services
{
    public static class TwitterService
    {
        private const string CONSUMER_KEY = "<consumer_key>";
        private const string CONSUMER_SECRET = "<consumer_secret>";

        static TwitterService()
        {
            Auth.SetApplicationOnlyCredentials(CONSUMER_KEY, CONSUMER_SECRET, true);
        }

        public static async Task<TwitterSearchResult> GetTopResultForSearchQueryAsync(string query)
        {
            var tweets = await SearchAsync.SearchTweets(new SearchTweetsParameters(query)
            {
                MaximumNumberOfResults = 1,
                SearchType = SearchResultType.Recent,
                Filters = TweetSearchFilters.Images
            });

            var tweet = tweets.FirstOrDefault();
            if (tweet == null)
            {
                return TwitterSearchResult.Empty;
            }

            return new TwitterSearchResult
            {
                Text = tweet.FullText,
                Author = tweet.CreatedBy.ScreenName,
                ImageUrl = tweet.Media.FirstOrDefault()?.MediaURL
            };
        }
    }

    public class TwitterSearchResult
    {
        public static TwitterSearchResult Empty { get; } = new TwitterSearchResult();

        public string Text { get; set; }

        public string Author { get; set; }

        public string ImageUrl { get; set; }
    }
}
