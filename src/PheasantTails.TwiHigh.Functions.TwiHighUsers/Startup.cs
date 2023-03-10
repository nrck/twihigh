using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.FunctionCore;
using PheasantTails.TwiHigh.Functions.TwiHighUsers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.TwiHighUsers
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
