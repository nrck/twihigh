using Microsoft.Extensions.Logging;

namespace PheasantTails.TwiHigh.Functions.Core.Extensions
{
    public static class TwihighLoggerExtensions
    {
        private const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss.fffff";

        private static string Now => DateTime.UtcNow.ToString(DATETIME_FORMAT);

        public static void TwiHighLogStart(this ILogger logger, string functionName)
            => logger.LogInformation($"[{Now}][{functionName}] Method started.");

        public static void TwiHighLogEnd(this ILogger logger, string functionName)
            => logger.LogInformation($"[{Now}][{functionName}] Method finished.");

        public static void TwiHighLogInformation(this ILogger logger, string functionName, string message, params object[] args)
            => logger.LogInformation($"[{Now}][{functionName}] {message}", args);

        public static void TwiHighLogWarning(this ILogger logger, string functionName, string message, params object[] args)
            => logger.LogWarning($"[{Now}][{functionName}] {message}", args);

        public static void TwiHighLogWarning(this ILogger logger, string functionName, Exception exception)
        {
            logger.LogWarning($"[{Now}][{functionName}] Exception has happened. Detail is under.");
            logger.LogWarning($"[{Now}][{functionName}] Message     : {exception.Message}");
            logger.LogWarning($"[{Now}][{functionName}] Stack Trace : {exception.StackTrace}");
        }

        public static void TwiHighLogError(this ILogger logger, string functionName, string message, params object[] args)
            => logger.LogError($"[{Now}][{functionName}] {message}", args);

        public static void TwiHighLogError(this ILogger logger, string functionName, Exception exception)
        {
            logger.LogError($"[{Now}][{functionName}] Exception has happened. Detail is under.");
            logger.LogError($"[{Now}][{functionName}] Message     : {exception.Message}");
            logger.LogError($"[{Now}][{functionName}] Stack Trace : {exception.StackTrace}");
        }
    }
}
