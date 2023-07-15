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
        /// ツイートが削除されたときにタイムラインに反映するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_DELETE_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "delete-timelines-tweet-trigger";

        /// <summary>
        /// リムーブされたユーザのツイートをタイムラインから削除するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_DELETE_TIMELINES_FOLLOW_TRIGGER_QUEUE_NAME = "delete-timelines-follow-trigger";

        /// <summary>
        /// リプライツイートをタイムラインに反映するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_UPDATE_REPLYFROM_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "update-replyfrom-timelines-tweet-trigger";

        /// <summary>
        /// リプライツイートをタイムラインに反映するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_UPDATE_REPLYTO_TIMELINES_TWEET_TRIGGER_QUEUE_NAME = "update-replyto-timelines-tweet-trigger";

        /// <summary>
        /// ユーザ情報が更新されたときにツイートを更新するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_UPDATE_USER_INFO_IN_TWEET_QUEUE_NAME = "update-user-info-in-tweet-trigger";

        /// <summary>
        /// ユーザ情報が更新されたときにタイムラインを更新するキュー名称
        /// </summary>
        public const string AZURE_STORAGE_UPDATE_USER_INFO_IN_TIMELINE_QUEUE_NAME = "update-user-info-in-timeline-trigger";
    }
}
