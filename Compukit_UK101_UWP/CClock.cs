using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace Compukit_UK101_UWP
{
    public class CClock
    {
        public DispatcherTimer Timer { get; set; }
        public Boolean Hold { get; set; }

        public Int32 ProcessorCycles;
        private MainPage mainPage;

        public CClock(MainPage mainPage)
        {
            this.mainPage = mainPage;
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 1); // 1 ms
            //Timer.Interval = new TimeSpan(100); // 10 us
            Timer.Tick += Timer_Tick;
            ProcessorCycles = 0;
        }

        private void Timer_Tick(object sender, object e)
        {
            while (ProcessorCycles < 20000)
            {
                if (!Hold)
                {
                    ProcessorCycles += mainPage.CSignetic6502.SingleStep();
                }
            }
            ProcessorCycles -= 20000;
        }
    }
}
