using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TekniskAnalyse.DataModels
{
    public class CandleStick
    {
        public CandleStick() { }
        public CandleStick(string date, Decimal open, Decimal high, Decimal low, Decimal close, int volume, string value)
        {
            this.date = date;
            this.open = open;
            this.high = high;
            this.low = low;
            this.close = close;
            this.volume = volume;
            this.value = value;
        }
        public string date;
        public Decimal open;
        public Decimal high;
        public Decimal low;
        public Decimal close;
        public int volume;
        public string value;
    }
}
