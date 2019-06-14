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
            "10 REM THIS IS A TEST TO SEE IF KEYBOARD WORKS",
            "15 REM AFTER LOAD AND RUN",
            "20 REM TRY PRESSING KEYS FOLLOWED BY ENTER",
            "25 REM TYPE QUIT TO QUIT",
            "30 INPUT Q$",
            "40 IF Q$ = \"QUIT\" THEN GOTO 999",
            "50 PRINT \"YOU TYPED: \"Q$",
            "60 GOTO 30",
            "999 PRINT \"BYE!\"",
            "RUN"
        };
    }
}
