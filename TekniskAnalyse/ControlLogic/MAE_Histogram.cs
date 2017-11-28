using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using TekniskAnalyse.Utilities;
using TekniskAnalyse.DataModels;
using System.Configuration;

namespace TekniskAnalyse.ControlLogic
{
    public class MAE_Histogram
    {
        public double[] ListeinneholderProsentvisMAE_ForEnAksje_SortertStigende = new double[500];   
        public List<double> ListeinneholderProsentvisMAE_ForEnAksje = new List<double>();

        public double[] ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje_SortertStigende = new double[500];
        public List<double> ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje = new List<double>();        

        public int antallAksjer = 10;      
        TextWriter tw = null;   
        string SAVEPATH = "";

        public void LageMAE_Histogram(List<CandleStick> ValgtAksjeCandleStickListe, string TICKER, int mae, string S_L_Oppgang)
        {  
            tw = null;
            SAVEPATH = ConfigurationManager.AppSettings["SAVEPATH"];
     
            // create a writer and open the file           
            if (!Directory.Exists(SAVEPATH))
            {
                Directory.CreateDirectory(SAVEPATH);
            }
            SAVEPATH += "AlleAksjer.htm";
            tw = new StreamWriter(SAVEPATH);

            // write a line of text to the file
            tw.WriteLine("<html>");
            tw.WriteLine("<head>");
            tw.WriteLine("<meta http-equiv=content-type content=\"text/html; charset=UTF-8\">");
            tw.WriteLine("<H1>Kurser siste " + antallAksjer + " dager for " + TICKER + "</H1><br/>");            
            tw.WriteLine("Sist oppdatert " + DateTime.Now.ToString());
            tw.WriteLine("<br/></head>");
            tw.WriteLine("<body>");

            double open = 0;
            double close = 0;
            double low = 0;
            double high = 0;

            double average_high = double.Parse(S_L_Oppgang);

            int antall_Candlestick_Opp = 0;
            int antall_Candlestick_Ned = 0;

            int antall_Candlestick_gevinst = 0;
            int antall_Candlestick_tap = 0;
            int antall_Candlestick_OpenLikClose = 0;

            int antallUforandret = 0;
            double akumulertOppgang = 0;
            double akumulertNedgang = 0;
            double akumulertOppgang_utenSL = 0;
            double akumulertNedgang_utenSL = 0;                  
                            
            for (int i = 0; i < antallAksjer; i++)
            {
                open = (double)ValgtAksjeCandleStickListe[i].open;
                close = (double)ValgtAksjeCandleStickListe[i].close;
                low = (double)ValgtAksjeCandleStickListe[i].low;
                high = (double)ValgtAksjeCandleStickListe[i].high;
               
                if (close > open) // CANLDESTICK : OPP
                {
                    antall_Candlestick_Opp++;

                    //regner som om vi hadde solgt uten S.L
                    akumulertOppgang_utenSL += ((close - open) / open) * 100;
                    akumulertOppgang += ((close - open) / open) * 100;                                 
                    
                    low = (double)ValgtAksjeCandleStickListe[i].low;
                    ListeinneholderProsentvisMAE_ForEnAksje.Add(finneProsentvisendring(low, open));
                }

                else if (open > close) // CANLDESTICK : NED
                {
                    antall_Candlestick_Ned++;

                    //regner som om vi hadde solgt uten S.L
                    akumulertNedgang_utenSL += ((close - open) / open) * 100;

                    double nedgangIkroner = open * (mae / 100);
                    if (low <= (open - nedgangIkroner)) // aksjen gikk lenger ned enn MAE
                    {
                        antall_Candlestick_tap++;
                        //antar at vi kjøper tilbake aksjen og den selges ut 2 ganger i SNITT på hver aksje
                        akumulertNedgang += mae; // aksjen blir solgt ut med et tap på MAE
                        akumulertNedgang += mae; // aksjen blir solgt ut med et tap på MAE
                    }
                    else //nedgangen er ikke så stor som mae%
                    {
                        antall_Candlestick_tap++;
                        double d = ((open - close) / open ) * 100;
                        akumulertNedgang += d; // solgt ut på close                       
                    }                   
                }

                else // CANLDESTICK : UFORANDRET (open = close)
                {
                    antallUforandret++;                                                         
                    double nedgangIkroner = open * (mae / 100);
                    if (low <= (open - nedgangIkroner)) // aksjen gikk lenger ned enn MAE
                    {
                        antall_Candlestick_tap++;
                        akumulertNedgang += mae; // aksjen blir solgt ut med et tap på MAE
                    }
                    else //nedgangen er 0% (open = close) solgt på close
                    {
                        antall_Candlestick_OpenLikClose++;
                        double d = ((open - close) / open) * 100;
                        akumulertNedgang += d; // solgt ut på close  = 0 % pga (open = close) 
                    }         
                }
            }
            tw.WriteLine("<br/>Akkumulert prosentvis oppgang/nedgang (UTEN bruk av stopp-loss) : " + (akumulertOppgang_utenSL + akumulertNedgang_utenSL).ToString("f2") + " %");
            tw.WriteLine("<br/>Akkumulert prosentvis oppgang/nedgang (ved bruk av stopp-loss) : " + (akumulertOppgang - akumulertNedgang).ToString("f2") + " %");
            tw.WriteLine("<br/><br/>MAE snitt brukt: " + mae);

            tw.WriteLine("<br/><br/>Antall candlestick som går opp: " + antall_Candlestick_Opp);
            tw.WriteLine("<br/>Antall candlestick som går ned: " + antall_Candlestick_Ned);
            tw.WriteLine("<br/>Antall candlestick hvor [open = close]: " + antallUforandret);

            tw.WriteLine("<br/><br/>Antall candlestick som gir gevinst (ved bruk av stopp-loss): " + antall_Candlestick_gevinst);
            tw.WriteLine("<br/>Antall candlestick som gir tap (ved bruk av stopp-loss): " + antall_Candlestick_tap);
            tw.WriteLine("<br/>Antall candlestick hvor [open = close] (ved bruk av stopp-loss): " + antall_Candlestick_OpenLikClose);
                 
            double maxValue = GetMax();
            double minValue = GetMin();
            
            maxValue = double.Parse(maxValue.ToString("F1"));

            for (int i = 0; i < ListeinneholderProsentvisMAE_ForEnAksje.Count; i++)
            {
                int index = (int)( (ListeinneholderProsentvisMAE_ForEnAksje[i] / 0.1));
                ListeinneholderProsentvisMAE_ForEnAksje_SortertStigende[index]++;             
            }
            tw.WriteLine("<br/><br/>");                             
                        
            // close the stream
            tw.Close();

            //starte nettleser - vise oversikt over alle aksjene            
            System.Diagnostics.Process.Start(SAVEPATH);
        }        


        //Finne høyeste verdi av alle numberOfDays for opptegning av aksjegraf
        private double GetMax_()
        {
            double max = double.MinValue;
            double high;
            for (int i = 0; i < ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje.Count; i++)
            {
                high = ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje[i];
                if (high > max)
                {
                    max = high;
                }
            }
            return max;
        }


        //Finne laveste verdi av alle numberOfDays for opptegning av aksjegraf
        private double GetMin_()
        {
            double min = double.MaxValue;
            double low;
            for (int i = 0; i < ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje.Count; i++)
            {
                low = ListeinneholderProsentvisOppgangFraOpenTilClose_ForEnAksje[i];
                if (low < min)
                {
                    min = low;
                }
            }
            return min;
        }              


        //Finne høyeste verdi av alle numberOfDays for opptegning av aksjegraf
        private double GetMax()
        {
            double max = double.MinValue;
            double high;
            for (int i = 0; i < ListeinneholderProsentvisMAE_ForEnAksje.Count; i++)
            {
                high = ListeinneholderProsentvisMAE_ForEnAksje[i];
                if (high > max)
                {
                    max = high;
                }
            }
            return max;
        }


        //Finne laveste verdi av alle numberOfDays for opptegning av aksjegraf
        private double GetMin()
        {
            double min = double.MaxValue;
            double low;
            for (int i = 0; i < ListeinneholderProsentvisMAE_ForEnAksje.Count; i++)
            {
                low = ListeinneholderProsentvisMAE_ForEnAksje[i];
                if (low < min)
                {
                    min = low;
                }
            }
            return min;
        }

        
        double finneProsentvisendring(double laveste, double hoyeste)
        {
            return ((hoyeste - laveste) / laveste) * 100;
        }              
    }
}


