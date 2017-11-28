using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TekniskAnalyse.DataModels
{
    class AksjeInfo
    {
        public string TICKER;
        public List<Double> sluttkursArray = new List<double>();       
        public double PROSENTVISSTIGNING5dg;
        public double OMSETNING;
    }
}
