namespace PheasantTails.TwiHigh.Functions.Core
{
    public class StaticStrings
    {
        public const string TWIHIGH_COSMOSDB_NAME = "TwiHighDB";
        public const string TWIHIGH_TWEET_CONTAINER_NAME = "Tweets";
        public const string TWIHIGH_TIMELINE_CONTAINER_NAME = "Timelines";
        public const string TWIHIGH_USER_CONTAINER_NAME = "Users";

        public const string QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME = "StorageConnectionString";

        public const string AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "add-timelines-tweet-trigger";
        public const string AZURE_STORAGE_ADD_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME = "add-timelines-follow-trigger";
        public const string AZURE_STORAGE_DELETE_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "delete-timelines-tweet-trigger";
        public const string AZURE_STORAGE_UPDATE_REPLYFROM_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "update-replyfrom-timelines-tweet-trigger";
        public const string AZURE_STORAGE_UPDATE_REPLYTO_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "update-replyto-timelines-tweet-trigger";
    }
}
