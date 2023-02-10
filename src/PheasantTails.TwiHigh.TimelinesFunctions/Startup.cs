using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.FunctionCore;

[assembly: FunctionsStartup(typeof(PheasantTails.TwiHigh.TimelinesFunctions.Startup))]
namespace PheasantTails.TwiHigh.TimelinesFunctions
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
