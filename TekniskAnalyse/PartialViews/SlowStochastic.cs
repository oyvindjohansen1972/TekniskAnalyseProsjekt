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
using TekniskAnalyse.DataModels;

namespace TekniskAnalyse
{
    partial class Form1
    {        
        
        private void DrawValgtAksjeSlowStochasticGraph()
        {       
            ValgtAksje_SlowStochasticArray_K_Value.Clear();
            ValgtAksje_SlowStochasticArray_D_Value.Clear();
            //kalkulere SlowStochastic
            int period = 14;
            Decimal K = 0;

            for (int Today = 0; Today < numberOfDaysShown + period; Today++)
            {
                K = ((ValgtAksjeCandleStickListe[Today].close - GetLowestLowSlowStochastic(period, Today, ValgtAksjeCandleStickListe)) / (GetHighestHighSlowStochastic(period, Today, ValgtAksjeCandleStickListe) - GetLowestLowSlowStochastic(period, Today, ValgtAksjeCandleStickListe))) * 100;
                ValgtAksje_SlowStochasticArray_K_Value.Add(K);
            }
            FindMovingAvegageSlowStochastic(3, ValgtAksje_SlowStochasticArray_D_Value, ValgtAksje_SlowStochasticArray_K_Value);

            FindMovingAvegageSlowStochastic(3, ValgtAksje_SlowStochasticArray_K_Value, ValgtAksje_SlowStochasticArray_D_Value);

            DrawSlowStochasticValues();
            DrawSlowStochasticMonthsLines();

            DrawValgtAksjeSlowStochastic();
        }



        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //******************************************   DRAWING   ************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************                

        
        //Tegner opp linjer og verdier i SlowStochastic vinduet før vi tegner grafene
        private void DrawSlowStochasticValues()
        {
            //clear the screen
            g_ValgtAksje_indikator.Clear(SystemColors.Window);

            // create  a  pen object with which to draw
            Pen LightGray = new Pen(Color.OldLace, 1);  // draw the line      
            Pen Black = new Pen(Color.Black, 1);  // draw the line     
            LightGray.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            SolidBrush myBrush = new SolidBrush(Color.Black);
            Font myFont = new Font("Times New Roman", 8, FontStyle.Regular);
            
            int YZeroLine = (DrawValgtAksjePriceIndicatorY_Height / 5) + 3 ;
            g_ValgtAksje_indikator.DrawLine(LightGray, 0, YZeroLine, MainGraphRightLine + 6, YZeroLine);
            g_ValgtAksje_indikator.DrawLine(Black, MainGraphRightLine + 3, YZeroLine, MainGraphRightLine + 7, YZeroLine);
            g_ValgtAksje_indikator.DrawString("80", myFont, myBrush, MainGraphRightLine + 13, YZeroLine - 7);

            YZeroLine = ((DrawValgtAksjePriceIndicatorY_Height / 5) * 4) - 3;
            g_ValgtAksje_indikator.DrawLine(LightGray, 0, YZeroLine, MainGraphRightLine + 6, YZeroLine);
            g_ValgtAksje_indikator.DrawLine(Black, MainGraphRightLine + 3, YZeroLine, MainGraphRightLine + 7, YZeroLine);
            g_ValgtAksje_indikator.DrawString("20", myFont, myBrush, MainGraphRightLine + 13, YZeroLine - 7);
            g_ValgtAksje_indikator.DrawLine(Black, MainGraphRightLine + 5, 110, MainGraphRightLine + 5, 10);
        }


        //Tegner vertikale linjer for hver mnd langs SlowStochastic grafen
        private void DrawSlowStochasticMonthsLines()
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



        private void DrawValgtAksjeSlowStochastic()
        { 
            Double Range = 100;                
            Double PixlesPrUnit = (DrawValgtAksjePriceIndicatorY_Height - 10) / Range;

            int distanceBetweenCandles = WidthCandle + 1;
            int Xpos = MainGraphRightLine - distanceBetweenCandles;

            for (int i = 0; i <= numberOfDaysShown; i++)
            {
                DrawONE_ValgtAksjeSlowStochastic(
                                                      Xpos,
                                                      (double)(ValgtAksje_SlowStochasticArray_K_Value[i] * (decimal)PixlesPrUnit),
                                                      (double)(ValgtAksje_SlowStochasticArray_D_Value[i] * (decimal)PixlesPrUnit),
                                                      i
                                                  );
                Xpos -= distanceBetweenCandles;
            }
        }
        
        
        private void DrawONE_ValgtAksjeSlowStochastic(int Xpos, Double SlowStochasticArray_K_Value, Double SlowStochasticArray_D_Value, int i)
        {           
            Pen blue = new Pen(Color.Blue, 1);
            Pen red = new Pen(Color.Red, 1);                     
            int XposShadow = Xpos + ((WidthCandle - 1) / 2);    
            int tempVar = 1;       

            Double Today = SlowStochasticArray_K_Value;         
            if (i == 0)
            {
                YesterdayMACD = Today;
                tempVar = 0; 
            }
            g_ValgtAksje_indikator.DrawLine(blue, XposShadow, (int)(DrawValgtAksjePriceIndicatorY_Height - Today - 5), XposShadow + (tempVar * WidthCandle), (int)(DrawValgtAksjePriceIndicatorY_Height - YesterdayMACD- 5));
            YesterdayMACD = Today;


            Today = SlowStochasticArray_D_Value;
            if (i == 0)
            {
                YesterdayMACDSignalLine = Today;               
            }
            g_ValgtAksje_indikator.DrawLine(red, XposShadow, (int)(DrawValgtAksjePriceIndicatorY_Height - Today - 5), XposShadow + (tempVar * WidthCandle), (int)(DrawValgtAksjePriceIndicatorY_Height - YesterdayMACDSignalLine)- 5);
            YesterdayMACDSignalLine = Today;    
        }



        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //******************************************   CALCULATIONS   ************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************
        //************************************************************************************************************************                


        private void FindMovingAvegageSlowStochastic(int MA_Days, List<Decimal> SlowStochasticArray_D_Value, List<Decimal> SlowStochasticArray_K_Value)
        {
            SlowStochasticArray_D_Value.Clear();
            int limit = 0;
            Decimal total = 0;
            Decimal MovingAverageValue = 0;
            int days = SlowStochasticArray_K_Value.Count - MA_Days;
            for (int start = 0; start < days; start++)
            {
                total = 0;
                limit = start + MA_Days;
                for (int index = start; index < limit; index++)
                {
                    total += SlowStochasticArray_K_Value[index];
                }
                MovingAverageValue = total / MA_Days;
                SlowStochasticArray_D_Value.Add(MovingAverageValue);
            }
        }              
        

        private Decimal GetHighestHighSlowStochastic(int period, int Today, List<CandleStick> CandleStickListe)
        {
            Decimal max = Decimal.MinValue;
            Decimal high;
            for (int i = Today; i < (period + Today); i++)
            {
                high = CandleStickListe[i].high;
                if (high > max)
                {
                    max = high;
                }
            }
            return max;
        }

             
        private Decimal GetLowestLowSlowStochastic(int period, int Today, List<CandleStick> CandleStickListe)
        {
            Decimal min = Decimal.MaxValue;
            Decimal low;
            for (int i = Today; i < (period + Today); i++)
            {
                low = CandleStickListe[i].low;
                if (low < min)
                {
                    min = low;
                }
            }
            return min;
        }
    }
}
