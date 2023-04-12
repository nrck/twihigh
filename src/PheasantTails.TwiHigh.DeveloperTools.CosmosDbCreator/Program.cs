using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using PheasantTails.TwiHigh.Data.Store.Entity;
using static PheasantTails.TwiHigh.Functions.Core.StaticStrings;

Console.WriteLine("本アプリケーションは開発環境用のデータベースを作成します。");
Console.WriteLine();
Console.WriteLine("Cosmos DB の Primary Connection String を入力してください。");
Console.WriteLine("手順");
Console.WriteLine("  0) Azure Cosmos DB Emulatorを起動していない場合は起動します。");
Console.WriteLine("  1) https://localhost:8081/_explorer/index.html にアクセスします。");
Console.WriteLine("  2) 画面に表示されている「Primary Connection String」をコピーします。");
Console.WriteLine("  3) この画面にコピーした値を貼り付け、Enterキーを押下します。");
Console.WriteLine();

var connectionString = Console.ReadLine();

var client = new CosmosClientBuilder(connectionString)
    .WithApplicationName("TwiHighAPI")
    .WithConnectionModeDirect() // Gatewayを通さずTCPで直接接続（この方がパフォーマンスが良いらしい）
    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
    .WithBulkExecution(true)
    .Build();

// DBの作成
await client.CreateDatabaseIfNotExistsAsync(TWIHIGH_COSMOSDB_NAME);
var database = client.GetDatabase(TWIHIGH_COSMOSDB_NAME);

// Timeline
await database.DefineContainer(TWIHIGH_TIMELINE_CONTAINER_NAME, Timeline.PARTITION_KEY)
    .WithUniqueKey()
        .Path("/tweetId")
    .Attach()
    .CreateIfNotExistsAsync();

// Tweet
await database.DefineContainer(TWIHIGH_TWEET_CONTAINER_NAME, Tweet.PARTITION_KEY)
    .CreateIfNotExistsAsync();

// TwiHighUser
await database.DefineContainer(TWIHIGH_USER_CONTAINER_NAME, TwiHighUser.PARTITION_KEY)
    .CreateIfNotExistsAsync();

Console.WriteLine();
Console.WriteLine("作成が完了しました。なにかキーを押すと画面を閉じます。");
Console.ReadKey();
