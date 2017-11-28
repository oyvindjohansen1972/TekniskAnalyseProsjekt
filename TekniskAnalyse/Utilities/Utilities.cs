using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using TekniskAnalyse.DataModels;

namespace TekniskAnalyse.Utilities
{
    public static class Utilities
    {
        public static bool IsNumber(this string aNumber)
        {
            BigInteger temp_big_int;
            var is_number = BigInteger.TryParse(aNumber, out temp_big_int);
            return is_number;
        }
        

        //Finne høyeste verdi av alle numberOfDays for opptegning av aksjegraf
        public static Decimal GetMax(int numberOfDays, List<CandleStick> CandleStickListe)
        {
            Decimal max = Decimal.MinValue;
            Decimal high;
            for (int i = 0; i < numberOfDays; i++)
            {
                high = CandleStickListe[i].high;
                if (high > max)
                {
                    max = high;
                }
            }
            return max;
        }


        //Finne laveste verdi av alle numberOfDays for opptegning av aksjegraf
        public static Decimal GetMin(int numberOfDays, List<CandleStick> CandleStickListe)
        {
            Decimal min = Decimal.MaxValue;
            Decimal low;
            for (int i = 0; i < numberOfDays; i++)
            {
                low = CandleStickListe[i].low;
                if (low < min)
                {
                    min = low;
                }
            }
            return min;
        }


        //Finne høyeste volum av alle numberOfDays for valgtaksje
        public static int GetMaxVolum(int numberOfDays, List<CandleStick> CandleStickListe)
        {
            int max = int.MinValue;
            int high;
            for (int i = 0; i < numberOfDays; i++)
            {
                if (CandleStickListe[i].value != "")
                {
                    high = (int)double.Parse(CandleStickListe[i].value.Replace(".", ","));
                }
                else
                {
                    high = 0;
                }

                if (high > max)
                {
                    max = high;
                }
            }
            if (max == 0) max = 1;
            return max;
        }
    }
}
