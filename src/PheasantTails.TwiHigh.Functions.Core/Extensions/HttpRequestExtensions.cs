﻿using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PheasantTails.TwiHigh.Functions.Core.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// <see cref="HttpRequest.Body"/>に格納されたJsonを<typeparamref name="T"/>にデシリアライズします。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T?> JsonDeserializeAsync<T>(this HttpRequest request, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };
            }
            return await JsonSerializer.DeserializeAsync<T>(request.Body, options, cancellationToken);
        }

        public static bool TryGetUserId(this HttpRequest request, TokenValidationParameters tokenValidationParameters, out string id)
        {
            id = string.Empty;

            // Authorization ヘッダーの取得
            string authorizationHeader = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return false;
            }

            // jwtの取得
            try
            {
                var bearerToken = authorizationHeader.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(bearerToken);
                handler.ValidateToken(bearerToken, tokenValidationParameters, out var _);

                id = jwt.Payload.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? string.Empty;
                return !string.IsNullOrEmpty(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static DateTimeOffset GetSinceDatetime(this HttpRequest request)
        {
            if (request.Query.TryGetDateTimeOffsetFromQuery("since", out var datetimeOffset))
            {
                return datetimeOffset;
            }
            else
            {
                return DateTimeOffset.MinValue;
            }
        }

        public static DateTimeOffset GetUntilDatetime(this HttpRequest request)
        {
            if (request.Query.TryGetDateTimeOffsetFromQuery("until", out var datetimeOffset))
            {
                return datetimeOffset;
            }
            else
            {
                return DateTimeOffset.MaxValue;
            }
        }

        private static bool TryGetDateTimeOffsetFromQuery(this IQueryCollection collection, string key, out DateTimeOffset dateTimeOffset)
        {
            dateTimeOffset = default;
            return collection.TryGetValue(key, out var since) && DateTimeOffset.TryParse(since, out dateTimeOffset);
        }
    }
}