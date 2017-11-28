using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TekniskAnalyse.DataModels
{
    public class LinjeMedAksjeInfo
    {
        public LinjeMedAksjeInfo() { }
        public LinjeMedAksjeInfo(string checkedForFavourite, string name, string ticker, int movingaverage1, int movingaverage2, string dato)
        {
            this.checkedForFavourite = checkedForFavourite;
            this.name = name;
            this.ticker = ticker;
            this.movingaverageLow = movingaverage1;
            this.movingaverageHigh = movingaverage2;
            this.dato = dato;
        }

        public string checkedForFavourite;
        public string name;
        public string ticker;
        public int movingaverageLow;
        public int movingaverageHigh;
        public string dato;
    }
}
