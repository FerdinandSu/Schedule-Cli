using PlasticMetal.MobileSuit.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HITScheduleMasterCLI
{
    public class StartUp:AutoDynamicParameter
    {
        [Option("i")]
        public string Input { set; get; }
        [Option("o")]
        public string Output { set; get; }
    }
}
