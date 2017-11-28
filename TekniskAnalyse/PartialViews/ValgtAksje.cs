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
using TekniskAnalyse;

namespace TekniskAnalyse
{
    partial class Form1
    {
        private void DrawValgtAksjeGraph()
        {
            labeClose.Text = "Close: " + ValgtAksjeCandleStickListe[0].close;
            labelOpen.Text = "Open: " + ValgtAksjeCandleStickListe[0].open;
            labelHigh.Text = "High: " + ValgtAksjeCandleStickListe[0].high;
            labelLow.Text = "Low: " + ValgtAksjeCandleStickListe[0].low;
            labelVolume.Text = "Volume: " + ValgtAksjeCandleStickListe[0].volume.ToString();
            labelValue.Text = "Verdi: " + ValgtAksjeCandleStickListe[0].value.ToString() + " kr";

            decimal result2 = 0;

            decimal result1 = ValgtAksjeCandleStickListe[0].close - ValgtAksjeCandleStickListe[0].open;
            if (ValgtAksjeCandleStickListe[0].open < 0)
            {
                result2 = (result1 / ValgtAksjeCandleStickListe[0].open) * 100;
            }                   
            //Skriver opp verdier vertikalt langs ValgtAksje grafen 
            string value = DrawValgtAksjePrice();

            //Tegner vertikale linjer for hver mnd langs ValgtAksje grafen før candlesticks tegnes opp
            string[] sv = value.Split('|');
            int YposMin = int.Parse(sv[3]);
            DrawValgtAksjeMonthsLines(YposMin);

            //Opptegning av selve ValgtAksje grafen med Candlesticks
            DrawValgtAksjeCandlesticks(value);
        }


        //Skriver opp verdier vertikalt langs ValgtAksje grafen 
        private string DrawValgtAksjePrice()
        {
            int GraphY_Height = DrawValgtAksjePriceGraphY_Height;
            Decimal HighestValueInPartOfArray = Utilities.Utilities.GetMax(numberOfDaysShown, ValgtAksjeCandleStickListe);
            Decimal LowestValueInPartOfArray = Utilities.Utilities.GetMin(numberOfDaysShown, ValgtAksjeCandleStickListe);
            g_ValgtAksje.Clear(SystemColors.Window);
            // create  a  pen object with which to draw
            Pen p = new Pen(Color.Black, 1);  // draw the line  
            Pen p2 = new Pen(Color.OldLace, 1);  // draw the line  
            p2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            SolidBrush myBrush = new SolidBrush(Color.Black);
            Font myFont = new Font("Times New Roman", 10);
            Decimal RangeInKroner = HighestValueInPartOfArray - LowestValueInPartOfArray;
            string value = GetRoundedLowValue(LowestValueInPartOfArray, RangeInKroner);
            string[] sv = value.Split('|');
            Decimal startValue = Decimal.Parse(sv[0]);
            Decimal divisionsValue = Decimal.Parse(sv[1]);
            Decimal endValue = (Decimal)HighestValueInPartOfArray;
            //finne hvor mange indelinger vi må bruke for å komme over høyeste verdi i valgt del av array
            Decimal startValue2 = startValue;
            int counter = 0;
            while (startValue2 < endValue)
            {
                startValue2 = startValue2 + divisionsValue;
                counter++;
            }
            //nå vet vi hvor mange indelinger vi trenger    
            if (counter == 0) { counter = 1; }
            Decimal PixelsPrDivision = (GraphY_Height - 10) / counter;
            int Ypos1 = GraphY_Height;
            int Ypos = 0;
            int i;
            for (i = 0; i <= counter; i++)
            {
                Ypos = Ypos1 - (int)(i * PixelsPrDivision);
                g_ValgtAksje.DrawLine(p2, 0, Ypos, MainGraphRightLine + 3, Ypos);
                g_ValgtAksje.DrawLine(p, MainGraphRightLine + 5, Ypos, MainGraphRightLine + 9, Ypos);
                g_ValgtAksje.DrawString((startValue + (i * divisionsValue)).ToString("f"), myFont, myBrush, MainGraphRightLine + 13, Ypos - 8);
            }
            //vertikal linje for grafen
            g_ValgtAksje.DrawLine(p, MainGraphRightLine + 7, Ypos, MainGraphRightLine + 7, GraphY_Height);
            decimal PixelsPrKrone = PixelsPrDivision / divisionsValue;
            return startValue.ToString() + "|" + PixelsPrKrone.ToString() + "|" + Ypos1.ToString() + "|" + Ypos.ToString();
        }


        //Tegner vertikale linjer for hver mnd langs ValgtAksje grafen før candlesticks tegnes opp
        private void DrawValgtAksjeMonthsLines(int YposMin)
        {
            int GraphY_Height = DrawValgtAksjePriceGraphY_Height;             
            int distanceBetweenCandles = WidthCandle + 1;
            string monthToday = "";
            string monthLast =  ValgtAksjeCandleStickListe[1].date.Substring(4, 2);
            int Xpos = MainGraphRightLine - distanceBetweenCandles + 4;
            for (int i = 0; i < numberOfDaysShown; i++)
            {
                monthToday = ValgtAksjeCandleStickListe[i + 1].date.Substring(4, 2);
                if (monthToday != monthLast)
                {
                    if (Xpos < 1180)
                    {
                        int monthIndex = int.Parse(monthToday);
                        Pen p3 = new Pen(Color.OldLace, 1);  //create  a  pen object with which to draw 
                        p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g_ValgtAksje.DrawLine(p3, Xpos, YposMin, Xpos, GraphY_Height);
                    }
                }
                Xpos -= distanceBetweenCandles;
                monthLast = monthToday;
            }
        }


        //Opptegning av selve ValgtAksje grafen med Candlesticks
        private void DrawValgtAksjeCandlesticks(string returVal)
        {
            Decimal HighestVolumInPartOfArray = Utilities.Utilities.GetMaxVolum(numberOfDaysShown, ValgtAksjeCandleStickListe);
            Decimal HighestValueInPartOfArray = Utilities.Utilities.GetMax(numberOfDaysShown, ValgtAksjeCandleStickListe);
            Decimal LowestValueInPartOfArray = Utilities.Utilities.GetMin(numberOfDaysShown, ValgtAksjeCandleStickListe);

            //Oppdaterer globale variabler med info om datasettet for opptegning
            string[] sp2 = new string[3];
            sp2 = returVal.Split('|');
            startValue_ = Decimal.Parse(sp2[0]); //Den laveste verdien som vises på grafen
            PixelsPrKrone_ = Decimal.Parse(sp2[1]);
            Ypos1_ = Decimal.Parse(sp2[2]);
            //tegner candlestics for en periode
           
            int distanceBetweenCandles = WidthCandle + 1;
            int Xpos = MainGraphRightLine - distanceBetweenCandles + 4;
            //bool printOrangeLine = false;
            AntallDagerOpp = 0;
            AntallDagerNed = 0;      
            akumulertOppgang = 0;
            akumulertNedgang = 0;
            akumulertMAE = 0;                   
                      
            for (int i = 0; i < numberOfDaysShown; i++)
            {                      
               
                DrawONE_ValgtAksjeCandle(
                              ValgtAksjeCandleStickListe[i].open,
                              ValgtAksjeCandleStickListe[i].high,
                              ValgtAksjeCandleStickListe[i].low,
                              ValgtAksjeCandleStickListe[i].close,
                              Xpos,
                              ValgtAksje_MovingAverageArrayLowValue[i],
                              ValgtAksje_MovingAverageArrayHighValue[i],
                              ValgtAksje_LongMovingAverageArray[i],
                              i
                          );
                Xpos -= distanceBetweenCandles;
            }            
        }


        private Decimal GetAverageDailyRange()
        {
            Decimal totalValue = 0;
            int days = 22;
           
            for (int i = 0; i < days; i++)
            {
                totalValue += (ValgtAksjeCandleStickListe[i].high - ValgtAksjeCandleStickListe[i].low);
            }
            return totalValue / days;
        }


        //Tegner opp candlesticks for valgt aksje
        private void DrawONE_ValgtAksjeCandle(Decimal open, Decimal high, Decimal low, Decimal close, int Xpos, Decimal MovingAverageLowValue, Decimal MovingAverageHighValue, Decimal LongMovingAverage, int i)
        {
            int GraphY_Height = DrawValgtAksjePriceGraphY_Height; ;
            Color my_blue = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            Color my_red = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            Pen red = new Pen(Color.Red, 1);
            SolidBrush myBrush = new SolidBrush(Color.Black);
            Pen blue = new Pen(my_blue, 1);
            Pen black = new Pen(Color.Black, 1);
            Pen green = new Pen(Color.Green, 1);
            Pen gold = new Pen(Color.Gold, 1);
            Pen pink = new Pen(Color.HotPink, 1);

            Pen gray = new Pen(Color.OldLace, 1);
            decimal tempOpen = open;
            decimal tempClose = close;
            decimal tempLow = low;
           
            int Ypos = GraphY_Height;
            int XposShadow = Xpos + ((WidthCandle - 1) / 2);
            int XposFront = Xpos - ((WidthCandle - 1) / 2);
            open = (open - startValue_) * PixelsPrKrone_;
            high = (high - startValue_) * PixelsPrKrone_;
            low = (low - startValue_) * PixelsPrKrone_;
            close = (close - startValue_) * PixelsPrKrone_;

            //tegne linje som viser når aksjen ble lagt i favoritter
            if (ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].dato == ValgtAksjeCandleStickListe[i].date)
            {             
                Pen p2 = new Pen(Color.Green, 1);
                float[] f2 = { 5, 5, 5, 5 };
                p2.DashPattern = f2;
                g_ValgtAksje.DrawLine(p2, Xpos, (int)(Ypos1_ - close), (int)1250, (int)(Ypos1_ - close));
            }

            //Tegne glidende snitt - LowValue
            int tempVar = 1;
            Decimal d = close;
            Decimal Today = (MovingAverageLowValue - startValue_) * PixelsPrKrone_;
            if (i == 0)
            {
                Yesterday = Today;
                tempVar = 0;
            }

            g_ValgtAksje.DrawLine(gold, XposShadow, (int)(Ypos1_ - Today), XposShadow + (tempVar * WidthCandle), (int)(Ypos1_ - Yesterday));
            Yesterday = Today;

            //Tegne glidende snitt - HighValue
            Decimal ddd = close;
            Decimal Today3 = (MovingAverageHighValue - startValue_) * PixelsPrKrone_;
            if (i == 0)
            {
                Yesterday3 = Today3;
            }

            g_ValgtAksje.DrawLine(pink, XposShadow, (int)(Ypos1_ - Today3), XposShadow + (tempVar * WidthCandle), (int)(Ypos1_ - Yesterday3));
            Yesterday3 = Today3;

            //Tegne langt glidende snitt
            Decimal dd = close;
            Decimal Today2 = (LongMovingAverage - startValue_) * PixelsPrKrone_;
            if (i == 0)
            {
                Yesterday2 = Today2;
            }

            g_ValgtAksje.DrawLine(green, XposShadow, (int)(Ypos1_ - Today2), XposShadow + (tempVar * WidthCandle), (int)(Ypos1_ - Yesterday2));
            Yesterday2 = Today2;            

            if (close > open) //GJENNOMSIKTIG CANLDESTICK : OPP
            {  
                g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - close), Xpos, (int)(Ypos1_ - high));//linjen fra low til open
                g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - open), Xpos, (int)(Ypos1_ - low));//linjen fra close til high             
                g_ValgtAksje.DrawRectangle(black, XposFront + 1, (int)(Ypos1_ - close), WidthCandle - 3, (int)(close - open));//linjen fra open til close  
                AntallDagerOpp++;
                akumulertOppgang += ((tempClose - tempOpen) / tempOpen) * 100;
                akumulertMAE += ((tempOpen - tempLow) / tempOpen) * 100;
            }

            else if (close < open) //HELFARGET CANDLESTICK : NED
            {
                g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - low), Xpos, (int)(Ypos1_ - high));//linjen fra close til high
                //g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - close), Xpos, (int)(Ypos1_ - low));//linjen fra low til open
                //g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - open), Xpos, (int)(Ypos1_ - high));//linjen fra close til high
                g_ValgtAksje.FillRectangle(myBrush, (int)XposFront + 1, (int)(Ypos1_ - open), (int)WidthCandle - 2, (int)(open - close + 1));//linjen fra open til close
                AntallDagerNed++;
                akumulertNedgang += ((tempClose - tempOpen) / tempOpen) * 100;
            }

             else // ingen forskjell mellom close og open
            {               
                g_ValgtAksje.DrawLine(black, XposFront + 1, (int)(Ypos1_ - close), XposFront + WidthCandle - 2, (int)(Ypos1_ - close));//linjen fra open til close                 
                g_ValgtAksje.DrawLine(black, Xpos, (int)(Ypos1_ - low), Xpos, (int)(Ypos1_ - high));//linjen fra close til high
            }                       
        }
    }
}
