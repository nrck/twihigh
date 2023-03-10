using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Timelines;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.Timelines
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
