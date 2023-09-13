using PheasantTails.TwiHigh.Data.Model.Feeds;
using PheasantTails.TwiHigh.Data.Model.TwiHighUsers;
using PheasantTails.TwiHigh.Data.Store.Entity;
using System.Collections.Generic;
using System.Linq;

namespace PheasantTails.TwiHigh.Functions.Feeds.Helpers
{
    internal static class FeedEntityHelper
    {
        internal static ResponseFeedsContext ToResponseFeedsContext(this IEnumerable<FeedContext> feeds)
        {
            var latest = feeds.Max(t => t.CreateAt < t.UpdateAt ? t.UpdateAt : t.CreateAt);
            var oldest = feeds.Min(t => t.CreateAt > t.UpdateAt ? t.UpdateAt : t.CreateAt);
            var response = new ResponseFeedsContext
            {
                Latest = latest,
                Oldest = oldest,
                Feeds = feeds.OrderByDescending(f => f.UpdateAt).ThenByDescending(f => f.CreateAt).ToArray()
            };

            return response;
        }

        internal static FeedContext CreateFeedContext(Feed feed, Tweet targetTweet, TwiHighUser feedByUser, Tweet feedByTweet)
        {
            var responseTwiHighUserContext = new ResponseTwiHighUserContext
            {
                AvatarUrl = feedByUser.AvatarUrl,
                Biography = feedByUser.Biography,
                CreateAt = feedByUser.CreateAt,
                DisplayId = feedByUser.DisplayId,
                DisplayName = feedByUser.DisplayName,
                Followers = feedByUser.Followers,
                Follows = feedByUser.Follows,
                Id = feedByUser.Id,
                Tweets = feedByUser.Tweets,
            };

            return new FeedContext
            {
                CreateAt = feed.CreateAt,
                FeedByTweet = feedByTweet,
                FeedByUser = responseTwiHighUserContext,
                FeedType = feed.FeedType,
                Id = feed.Id,
                InformationText = feed.InformationText,
                IsOpened = feed.IsOpened,
                ReferenceTweet = targetTweet,
                UpdateAt = feed.UpdateAt
            };
        }
    }
}
