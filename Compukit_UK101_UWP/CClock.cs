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

        private Int32 cycles;
        private MainPage mainPage;

        public CClock(MainPage mainPage)
        {
            this.mainPage = mainPage;
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 1); // 1 ms
            //Timer.Interval = new TimeSpan(100); // 10 us
            Timer.Tick += Timer_Tick;
            cycles = 0;
        }

        private void Timer_Tick(object sender, object e)
        {
            while (cycles < 20000)
            {
                if (!Hold)
                {
                    cycles += mainPage.CSignetic6502.SingleStep();
                }
            }
            cycles -= 20000;
        }
    }
}
