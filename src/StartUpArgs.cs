using PlasticMetal.MobileSuit.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HitScheduleMaster
{
    public class StartUpArgs:AutoDynamicParameter
    {
        [Option("i")]
        public string Input { set; get; }
        [Option("o")]
        public string Output { set; get; }
    }
}
