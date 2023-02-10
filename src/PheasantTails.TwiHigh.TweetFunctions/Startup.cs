using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.FunctionCore;

[assembly: FunctionsStartup(typeof(PheasantTails.TwiHigh.TweetFunctions.Startup))]
namespace PheasantTails.TwiHigh.TweetFunctions
{
    public class Startup: TwiHighFunctionStartup
    {
    }
}
