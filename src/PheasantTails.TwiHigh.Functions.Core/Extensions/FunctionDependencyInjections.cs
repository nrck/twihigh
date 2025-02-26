using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PheasantTails.TwiHigh.Functions.Core.Extensions;

public static class FunctionDependencyInjections
{
    public static IHostBuilder ConfigureTwiHighFunctions(this IHostBuilder hostBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables()
                .Build();

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton((s) =>
            {
                // エンドポイントの読み込み
                string cosmosDbEndPointUrl = configuration["COSMOS_DB_END_POINT_URL"];
                if (string.IsNullOrEmpty(cosmosDbEndPointUrl))
                {
                    throw new NullReferenceException("You must configure environment value 'COSMOS_DB_END_POINT_URL'.");
                }

                // 認証キーの読み込み
                string cosmodDbAuthorizationKey = configuration["COSMOS_DB_AUTHORIZATION_KEY"];
                if (string.IsNullOrEmpty(cosmodDbAuthorizationKey))
                {
                    throw new NullReferenceException("You must configure environment value 'COSMOS_DB_AUTHORIZATION_KEY'.");
                }

                // CosmosClientの設定
                var configurationBuilder = new CosmosClientBuilder(cosmosDbEndPointUrl, cosmodDbAuthorizationKey);
                return configurationBuilder
                        .WithApplicationName("TwiHighApi")
                        //.WithCustomSerializer(cosmosSystemTextJsonSerializer) // CustomSerializerを使うとLinqクエリでプロパティ名がCamelCaseにならないという現段階の仕様のためコメントアウト
                        // 参考: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2685
                        // 参考: https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2386
                        .WithConnectionModeDirect() // Gatewayを通さずTCPで直接接続（この方がパフォーマンスが良いらしい）
                        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                        .WithBulkExecution(true)
                        .Build();
            });

            services.AddSingleton((s) =>
            {
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT_SECURITY_KEY"]));
                TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT_ISSUER"],

                    ValidateAudience = true,
                    ValidAudience = configuration["JWT_AUDIENCE"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    IssuerSigningKey = key
                };

                return tokenValidationParameters;
            });
        });

        return hostBuilder;
    }
}
