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
        private void DrawValgtAksjeVolumGraph()
        {
            //Skriver opp verdier vertikalt langs volum grafen 
            int YposMin = DrawVolumPrice();

            ////Tegner vertikale linjer for hver mnd langs ValgtAksje grafen før candlesticks tegnes opp
          DrawVolumMonthsLines(YposMin);

            ////Opptegning av selve volum grafen med Candlesticks
          DrawVolumeBars(YposMin);
        }

        //Skriver opp verdier vertikalt langs volumgrafen (til valgt aksje)
        private int DrawVolumPrice()
        {
            int IndikatorY_Height = 110; // This is the height used for showing indicators for the individual stocks charts(each is 110 pixels high)
            g_ValgtAksje_indikator.Clear(SystemColors.Window);
            int HighestVolumInPartOfArray = (int)Utilities.Utilities.GetMaxVolum(numberOfDaysShown, ValgtAksjeCandleStickListe);
            // create  a  pen object with which to draw
            Pen p = new Pen(Color.OldLace, 1);  // draw the line  
            Pen black = new Pen(Color.Black, 1);  // draw the line  
            Pen p2 = new Pen(Color.OldLace, 1);  // draw the line  
            p2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            SolidBrush myBrush = new SolidBrush(Color.Black);
            Font myFont = new Font("Times New Roman", 8, FontStyle.Regular);
            Decimal RangeInKroner = HighestVolumInPartOfArray;
            //g.DrawString("High Volume= " + HighestVolumInPartOfArray.ToString("f"), myFont, myBrush, MainGraphWinSurroundingRectangleLeft, 430);
            int KronerPrPixel = HighestVolumInPartOfArray / (IndikatorY_Height - 6);
            string Unit= " M";
            if (HighestVolumInPartOfArray < 1000000)
            {
                Unit = " K";
            }
            int firstQuarterValue = (int)(0.25 * HighestVolumInPartOfArray / KronerPrPixel);
            int Ypos = 0;
            double i = .25;
            int step = (IndikatorY_Height - 10) / 4;
            for (int index = 0; index < (IndikatorY_Height); index += step)
            {
                Ypos = IndikatorY_Height - index;
                g_ValgtAksje_indikator.DrawLine(p2, 0, Ypos, MainGraphRightLine + 6, Ypos);
                g_ValgtAksje_indikator.DrawLine(black, MainGraphRightLine + 5, Ypos, MainGraphRightLine + 9, Ypos);
                double rr = ((double)(HighestVolumInPartOfArray) / (double)100000000) * index;

                g_ValgtAksje_indikator.DrawString(rr.ToString("f") + Unit, myFont, myBrush, MainGraphRightLine + 13, Ypos - 7);
              
              
                i += .25;
            }
            g_ValgtAksje_indikator.DrawLine(black, MainGraphRightLine + 7, IndikatorY_Height, MainGraphRightLine + 7, Ypos);
            return Ypos;
        }

        //Tegner vertikale linjer for hver mnd langs Volum grafen før candlesticks tegnes opp
        private void DrawVolumMonthsLines(int YposMin)
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
                        g_ValgtAksje_indikator.DrawLine(p3, Xpos, YposMin, Xpos, IndikatorY_Height);
                    }
                }
                Xpos -= distanceBetweenCandles;
                monthLast = monthToday;
            }
        }
        
        //Tegner opp Volum for valgt aksje 
        private void DrawVolumeBars(int XX)
        {
            //Decimal Volum, int Xpos, int WidthCandle
            int IndikatorY_Height = 110; // This is the height used for showing indicators for the individual stocks charts(each is 110 pixels high)
            //tegner candlestics for en periode          
            int distanceBetweenCandles = WidthCandle + 1;
            int Xpos = MainGraphRightLine - distanceBetweenCandles + 4;
            int HighestVolumInPartOfArray = Utilities.Utilities.GetMaxVolum(numberOfDaysShown, ValgtAksjeCandleStickListe);
            Pen p = new Pen(Color.Black, WidthCandle);
            int Ypos = IndikatorY_Height;
            int Volum = 0;
            int XposShadow = Xpos + (WidthCandle / 2);
            
            Decimal PixelsPrVolumeUnit = (Decimal)(IndikatorY_Height - XX) / (Decimal)HighestVolumInPartOfArray;
            Decimal pp = 0;
            int ThicknessBars = WidthCandle / 2;
            for (int i = 0; i < numberOfDaysShown; i++)
            {
                if (ValgtAksjeCandleStickListe[i].value != "")
                {
                    pp = Decimal.Parse(ValgtAksjeCandleStickListe[i].value.Replace(".", ","));
                }
                else
                {
                    pp = 0;
                }
                Volum = (int)(pp * PixelsPrVolumeUnit);
                //linjen fra Ypos til Volum  
                g_ValgtAksje_indikator.DrawLine(p, XposShadow - ThicknessBars, (int)(Ypos - Volum), XposShadow - ThicknessBars, (int)(Ypos));
                XposShadow -= distanceBetweenCandles;
            }
        }
    }
}
