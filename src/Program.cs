using System.Threading;
using PlasticMetal.MobileSuit;
using PlasticMetal.MobileSuit.Core;
using PlasticMetal.MobileSuit.ObjectModel.Future;

namespace HITScheduleMasterCLI
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Suit.GetBuilder()
                .UsePrompt<PowerLineThemedPromptServer>()
                //.UseLog(ILogger.OfDirectory("D:\\"))
                .Build<Driver>()
                .Run(args);
 

        }
        

    }
}
