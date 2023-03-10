﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using PheasantTails.TwiHigh.Functions.Core;
using PheasantTails.TwiHigh.Functions.Follows;

[assembly: FunctionsStartup(typeof(Startup))]
namespace PheasantTails.TwiHigh.Functions.Follows
{
    public class Startup : TwiHighFunctionStartup
    {
    }
}
