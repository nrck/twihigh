using System.Diagnostics;

namespace PheasantTails.TwiHigh.Beta.Client.Extensions
{
    public static class LoggerExtension
    {
        private const string START_FORMAT = "Start: {FunctionName} is started.";
        private const string FINISH_FORMAT = "Finish: {FunctionName} is finished.";

        public static void LogStart(this ILogger logger)
        {
            var callerFrame = new StackFrame(1, true).GetMethod();
            if (callerFrame == null)
            {
                return;
            }
            var functionName = $"{callerFrame.ReflectedType}.{callerFrame.Name}()";
            logger.LogDebug(START_FORMAT, functionName);
        }

        public static void LogFinish(this ILogger logger)
        {
            var callerFrame = new StackFrame(1, true).GetMethod();
            if (callerFrame == null)
            {
                return;
            }
            var functionName = $"{callerFrame.ReflectedType}.{callerFrame.Name}()";
            logger.LogDebug(FINISH_FORMAT, functionName);
        }

    }
}
