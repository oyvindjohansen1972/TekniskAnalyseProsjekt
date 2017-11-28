using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;

namespace TekniskAnalyse
{
    partial class Form1
    {
        private void DrawValgtAksjeMACDGraph()
        {           
            ValgtAksje_ExpMovingAverageArrayLowValue.Clear();
            ValgtAksje_ExpMovingAverageArrayHighValue.Clear();
            MACDSignalLine.Clear();
            MACDLine.Clear();

            //kalkulere EMA
            CalculateEMA(12, ValgtAksje_ExpMovingAverageArrayLowValue);
            CalculateEMA(26, ValgtAksje_ExpMovingAverageArrayHighValue);

            for (int i = 0; i <= numberOfDaysShown + 50; i++)
            {
                MACDLine.Add((Double)(ValgtAksje_ExpMovingAverageArrayLowValue[i] - ValgtAksje_ExpMovingAverageArrayHighValue[i]));
            }                     

            FindMovingAvegageMACDLine(9, MACDSignalLine, MACDLine);                  

             //Skriver opp verdier vertikalt langs volum grafen 
            DrawMACDValues();

            ////Tegner vertikale linjer for hver mnd langs ValgtAksje grafen før candlesticks tegnes opp
            DrawMACDMonthsLines();
            DrawValgtAksjeMACDLine();
        }
             

        //Skriver opp verdier vertikalt langs MACD (til valgt aksje)
        private void DrawMACDValues()
        {
            //clear the screen
            g_ValgtAksje_indikator.Clear(SystemColors.Window);           
           
            // lager pen object for å tegne med
            Pen black = new Pen(Color.Black, 1);  // tegner line      
            Pen pink = new Pen(Color.Pink, 1);  // tegner line      
                              
            int YZeroLine = DrawValgtAksjePriceIndicatorY_Height / 2;
            g_ValgtAksje_indikator.DrawLine(pink, 0, YZeroLine, MainGraphRightLine + 5, YZeroLine);
            g_ValgtAksje_indikator.DrawLine(black, MainGraphRightLine + 5, 110, MainGraphRightLine + 5, 10);
        }


        //Tegner vertikale linjer for hver mnd langs MACD grafen
        private void DrawMACDMonthsLines()
        {
            int IndikatorY_Height = 110; // This is the height used for showing indicators for the individual stocks charts(each is 110 pixels high)
            int distanceBetweenCandles = WidthCandle + 1;
            string monthToday = "";
            string monthLast = ValgtAksjeCandleStickListe[1].date.Substring(4, 2);
            int Xpos = MainGraphRightLine - distanceBetweenCandles + 4;
            for (int i = 0; i < numberOfDaysShown; i++)
            {
                monthToday = ValgtAksjeCandleStickListe[i + 1].date.Substring(4, 2);
                if (monthToday != monthLast)
                {
                    if (Xpos < 1180)
                    {
                        int monthIndex = int.Parse(monthToday);
                        monthIndex++;
                        if (monthIndex > 12)
                        {
                            monthIndex = 1;
                        }
                        Font myFont = new Font("Times New Roman", 10);
                        SolidBrush myBrush = new SolidBrush(Color.Black);
                        g_ValgtAksje_indikator.DrawString(Months[monthIndex].ToString(), myFont, myBrush, Xpos - 10, 109);
                        Pen p3 = new Pen(Color.OldLace, 1);  //create  a  pen object with which to draw 
                        p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g_ValgtAksje_indikator.DrawLine(p3, Xpos, 0, Xpos, IndikatorY_Height);
                    }
                }
                Xpos -= distanceBetweenCandles;
                monthLast = monthToday;
            }
        }

        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************                
                
        private void DrawValgtAksjeMACDLine()
        {            
            Double HighestValueInPartOfArray = GetMaxMACD();
            Double LowestValueInPartOfArray = GetMinMACD();

            Double Range = 0;

            if(Math.Abs(HighestValueInPartOfArray) > Math.Abs(LowestValueInPartOfArray))
            {
                Range = Math.Abs(HighestValueInPartOfArray) * 2;
            }
            else
            {
                Range = Math.Abs(LowestValueInPartOfArray) * 2;
            }

            Double PixlesPrUnit = (DrawValgtAksjePriceIndicatorY_Height - 10) / Range;  
      
                   
            int distanceBetweenCandles = WidthCandle + 1;
            int Xpos = MainGraphRightLine - distanceBetweenCandles + 4;

            for (int i = 0; i <= numberOfDaysShown; i++)
            {
                DrawONE_ValgtAksjeMACDLine(                              
                              Xpos,
                              (MACDLine[i]* PixlesPrUnit),
                              (MACDSignalLine[i] * PixlesPrUnit), 
                              i                              
                          );
                Xpos -= distanceBetweenCandles;
            }
        }
        

        private void DrawONE_ValgtAksjeMACDLine(int Xpos, Double MACDLineValue, Double MACDSignalLineValue, int i)
        {           
            Pen blue = new Pen(Color.Blue, 1);
            Pen red = new Pen(Color.Red, 1);
            int YZeroLine = (DrawValgtAksjePriceIndicatorY_Height / 2);            
            int XposShadow = Xpos + ((WidthCandle - 1) / 2);
            int tempVar = 1;      

            Double Today = MACDLineValue;
            if (i == 0)
            {
                YesterdayMACD = Today;
                tempVar = 0;
            }
            g_ValgtAksje_indikator.DrawLine(blue, XposShadow, (int)(YZeroLine - Today) + 5, XposShadow + (tempVar * WidthCandle), (int)(YZeroLine - YesterdayMACD) + 5);
            YesterdayMACD = Today;


            Today = MACDSignalLineValue;
            if (i == 0)
            {
                YesterdayMACDSignalLine = Today;                
            }
            g_ValgtAksje_indikator.DrawLine(red, XposShadow, (int)(YZeroLine - Today) + 5, XposShadow + (tempVar * WidthCandle), (int)(YZeroLine - YesterdayMACDSignalLine) + 5);
            YesterdayMACDSignalLine = Today;           
        }


        //Finne høyeste MACD verdi
        private Double GetMaxMACD()
        {
            Double max = Double.MinValue;
            Double high;
            for (int i = 0; i <= numberOfDaysShown; i++)
            {
                high = MACDLine[i];
                if (high > max)
                {
                    max = high;
                }
            }
            return max;
        }


        //Finne laveste MACD verdi
        private Double GetMinMACD()
        {
            Double min = Double.MaxValue;
            Double low;
            for (int i = 0; i <= numberOfDaysShown; i++)
            {
                low = MACDLine[i];
                if (low < min)
                {
                    min = low;
                }
            }
            return min;
        }
                

        public void CalculateEMA(int NumberOfDays, List<Decimal> ValgtAksje_ExpMovingAverageArrayValue)
        {
            //finne første glidende gjennomsnittsverdi          

            Decimal initialValue = findSimpleMovingAverage(NumberOfDays);

            Decimal k = ((Decimal)2 / (Decimal)(NumberOfDays + 1));

            Decimal EMAYesterday = initialValue;
            Decimal priceToday = 0;

            for (int i = numberOfDaysShown + 50; i > -1; i--)
            {
                priceToday = ValgtAksjeCandleStickListe[i].close;
                EMAYesterday = findEMA(k, priceToday, EMAYesterday);
                ValgtAksje_ExpMovingAverageArrayValue.Add(EMAYesterday);
            }
            ValgtAksje_ExpMovingAverageArrayValue.Reverse();
        }


        public Decimal findEMA(Decimal k, Decimal priceToday, Decimal EMAYesterday)
        {
            Decimal p1 = (priceToday * k);
            Decimal p2 = (((Decimal)1 - (Decimal)k));
            Decimal p3 = EMAYesterday * p2;
            return p1 + p3;
        }


        private Decimal findSimpleMovingAverage(int MA_Days)
        {
            Decimal total = 0;
            Decimal MovingAverageValue = 0;
            for (int start = numberOfDaysShown + 1; start < (numberOfDaysShown + 1 + MA_Days); start++)
            {
                total += ValgtAksjeCandleStickListe[start].close;
            }
            return MovingAverageValue = total / MA_Days;
        }


        private void FindMovingAvegageMACDLine(int MA_Days, List<Double> MACDSignalLine, List<Double> MACDLine)
        {
            MACDSignalLine.Clear();
            int limit = 0;
            Double total = 0;
            Double MovingAverageValue = 0;
            int days = MACDLine.Count - MA_Days;
            for (int start = 0; start < days; start++)
            {
                total = 0;
                limit = start + MA_Days;
                for (int index = start; index < limit; index++)
                {
                    total += MACDLine[index];
                }
                MovingAverageValue = total / MA_Days;
                MACDSignalLine.Add(MovingAverageValue);
            }
        }              


    }
}
