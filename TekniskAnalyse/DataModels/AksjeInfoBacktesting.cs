using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TekniskAnalyse.DataModels
{
    class AksjeInfoBacktesting
    {
        public string TICKER;
        public List<Double> ProsentAkumulert = new List<double>();       
        public double VOLATILITET;
        public double OMSETNING;
    }
}
