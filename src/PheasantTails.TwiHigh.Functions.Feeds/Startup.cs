using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Feeds;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.Feeds
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
