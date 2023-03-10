using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.FunctionCore;

[assembly: FunctionsStartup(typeof(PheasantTails.TwiHigh.FollowsFunctions.Startup))]
namespace PheasantTails.TwiHigh.FollowsFunctions
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
