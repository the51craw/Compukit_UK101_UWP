using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compukit_UK101_UWP
{
    public partial class BasicProg
    {
        public string[] LoadAndRunTest = new string[] {
            "10 REM THIS IS A TEST TO SET WINDOW SIZE.",
            "15 POKE 547, 12 : POKE 549, 160",
            "20 REM PRESS ENTER WHEN DONE.",
            "30 INPUT Q$",
            "999 PRINT \"BYE!\"",
            "RUN"
        };
    }
}
