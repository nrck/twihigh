using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(PheasantTails.TwiHigh.TimelinesFunctions.Startup))]
namespace PheasantTails.TwiHigh.TimelinesFunctions
{
    public class Startup: FunctionsStartup
    {

        // 外部設定値の読み込み
        protected static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables()
                .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((s) =>
            {
                // エンドポイントの読み込み
                var endpoint = configuration["COSMOS_DB_END_POINT_URL"];
                if (string.IsNullOrEmpty(endpoint))
                {
                    throw new ArgumentNullException("COSMOS_DB_END_POINT_URL", "Azure Functionsの設定値「COSMOS_DB_END_POINT_URL」が未設定です。");
                }

                // 認証キーの読み込み
                var authKey = configuration["COSMOS_DB_AUTHORIZATION_KEY"];
                if (string.IsNullOrEmpty(authKey))
                {
                    throw new ArgumentNullException("COSMOS_DB_AUTHORIZATION_KEY", "Azure Functionsの設定値「COSMOS_DB_AUTHORIZATION_KEY」が未設定です。");
                }

                // CosmosClientの設定
                var configurationBuilder = new CosmosClientBuilder(endpoint, authKey);
                return configurationBuilder
                        .WithApplicationName("KinmuSystemAPI")
                        //.WithCustomSerializer(cosmosSystemTextJsonSerializer) // CustomSerializerを使うとLinqクエリでプロパティ名がCamelCaseにならないという現段階の仕様のためコメントアウト
                        // 参考: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2685
                        // 参考: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2386
                        .WithConnectionModeDirect() // Gatewayを通さずTCPで直接接続（この方がパフォーマンスが良いらしい）
                        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                        .WithBulkExecution(true)
                        .Build();
            });
        }
    }
}
