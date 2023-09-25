namespace PheasantTails.TwiHigh.Functions.Core
{
    public class StaticStrings
    {
        /// <summary>
        /// ツイハイDB名称
        /// </summary>
        public const string TWIHIGH_COSMOSDB_NAME = "TwiHighDB";

        /// <summary>
        /// ツイートを格納するコンテナ名称
        /// </summary>
        public const string TWIHIGH_TWEET_CONTAINER_NAME = "Tweets";

        /// <summary>
        /// タイムラインを格納するコンテナ名称
        /// </summary>
        public const string TWIHIGH_TIMELINE_CONTAINER_NAME = "Timelines";

        /// <summary>
        /// ユーザ情報を格納するコンテナ名称
        /// </summary>
        public const string TWIHIGH_USER_CONTAINER_NAME = "Users";

        /// <summary>
        /// 通知情報を格納するコンテナ名称
        /// </summary>
        public const string TWIHIGH_FEED_CONTAINER_NAME = "Feeds";

        /// <summary>
        /// キュートリガー用のキューが格納されているBlobStorageの接続文字列を保存している環境変数名
        /// </summary>
        public const string QUEUE_STORAGE_CONNECTION_STRINGS_ENV_NAME = "StorageConnectionString";

        /// <summary>
        /// 新規ツイートをタイムラインに反映するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_ADD_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "add-timelines-tweet-trigger";

        /// <summary>
        /// フォローされたユーザのツイートをタイムラインに反映するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_ADD_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME = "add-timelines-follow-trigger";

        /// <summary>
        /// リムーブされたユーザのツイートをタイムラインから削除するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME = "delete-timelines-follow-trigger";

        /// <summary>
        /// ユーザ情報が更新されたときにツイートを更新するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_UPDATE_USER_INFO_IN_TWEET_QUEUE_NAME = "update-user-info-in-tweet-trigger";

        /// <summary>
        /// お気に入り登録されたときにフィードを作成するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_FEED_FAVORED_BY_USER_QUEUE_NAME = "feed-favored-by-user-trigger";

        /// <summary>
        /// タイムライン内のツイートに対するPatchOperationを実行するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TWEET_IN_TIMELINES_QUEUE_NAME = "patch-tweet-in-timelines";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_DELETE_FAVORITE_QUEUE_NAME = "patch-timelines-by-delete-favorite";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_DELETE_TWEET_QUEUE_NAME = "patch-timelines-by-delete-tweet";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_ADD_REPLYFROM_QUEUE_NAME = "patch-timelines-by-add-replyfrom";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_REMOVE_REPLYTO_QUEUE_NAME = "patch-timelines-by-remove-replyto";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_ADD_FAVORITE_FROM_NAME = "patch-timelines-by-add-favorite-from";

        /// <summary>
        /// 
        /// </summary>
        public const string AZURE_STORAGE_PATCH_TIMELINES_BY_UPDATE_USER_INFO_NAME = "patch-timelines-by-update-user-info";

        /// <summary>
        /// お気に入り登録されたときにフィードを作成するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_FEED_MENTIONED_BY_USER_QUEUE_NAME = "feed-mentioned-by-user-trigger";
    }
}
