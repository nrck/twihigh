using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.FunctionCore;

[assembly: FunctionsStartup(typeof(PheasantTails.TwiHigh.AppUserFunctions.Startup))]
namespace PheasantTails.TwiHigh.AppUserFunctions
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
