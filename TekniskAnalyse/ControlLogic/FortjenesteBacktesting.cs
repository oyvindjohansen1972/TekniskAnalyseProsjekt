using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using TekniskAnalyse.DataModels;
using System.Configuration;

namespace TekniskAnalyse.ControlLogic
{
    public class FortjenesteBacktesting
    {
        List<string> ListContainingAllStockTickersFromFile_Alle_aksjer = new List<string>();
        List<CandleStick> AksjeCandleStickForEnAksje = new List<CandleStick>();
        List<AksjeInfoBacktesting> ListeMedCalkulerteDataForAlleAksjer = new List<AksjeInfoBacktesting>();
        List<string> dagerTilbakeArray = new List<string>();
        List<string> navnArray = new List<string>();
        TextWriter tw = null;
                
        String PathAksjeKurser = ConfigurationManager.AppSettings["PathAksjeKurser"]; 
        string SAVEPATH = "";
        string Path_Alle_aksjer_Liste = ConfigurationManager.AppSettings["Path_Alle_aksjer_Liste"];

        public void FinneFortjenesteVedBacktesting()
        {            
            dagerTilbakeArray.Add("40 - 20 dager");          
            dagerTilbakeArray.Add("20 - 0 dager");            
            dagerTilbakeArray.Add("20 - 15 dager");
            dagerTilbakeArray.Add("15 - 10 dager");
            dagerTilbakeArray.Add("10 - 5 dager");
            dagerTilbakeArray.Add("5 - 0 dager");
      
            ListContainingAllStockTickersFromFile_Alle_aksjer = new List<string>();          
            //  dagerTilbakeArray = new List<string>();
            navnArray = new List<string>();

            //lagring av aksjeinfo objekter for sortering
            ListeMedCalkulerteDataForAlleAksjer = new List<AksjeInfoBacktesting>();

            tw = null;
            SAVEPATH = ConfigurationManager.AppSettings["SAVEPATH"];            

            // laste ned liste over alle tickers
            GetAllTickersFromFile();

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
            tw.WriteLine("<H1>Kurser siste 80 dager</H1><br/><br/>");
            tw.WriteLine("<br>Ved 100 000 kr investert hver dag må vi tjene minimum 2000 [2%] på 10 dager for å dekke inn kurtasjen i perioden.<br>");
            tw.WriteLine("Sist oppdatert " + DateTime.Now.ToString());
            tw.WriteLine("</head>");
            tw.WriteLine("<body>");
            tw.WriteLine("<table width=\"100%\" border=\"1\">");

            //setter opp perioder i overskriften på tabellen
            tw.WriteLine("<tr>");
            tw.WriteLine("<td ALIGN = CENTER>");
            tw.WriteLine("<b>Navn</b>");
            tw.WriteLine("</td>");

            for (int i = 0; i < dagerTilbakeArray.Count; i++)
            {
                tw.WriteLine("<td ALIGN = CENTER>");
                tw.WriteLine("<b>" + dagerTilbakeArray[i].ToString() + "</b>");
                tw.WriteLine("</td>");
            }

            tw.WriteLine("<td ALIGN = CENTER>");
            tw.WriteLine("<b>Sist omsatt (hele dagen)</b>");
            tw.WriteLine("</td>");


            tw.WriteLine("<td ALIGN = CENTER>");
            tw.WriteLine("<b>Svingning pr dag i snitt</b>");
            tw.WriteLine("</td>");

            tw.WriteLine("</tr>"); 

            int antallAksjer = ListContainingAllStockTickersFromFile_Alle_aksjer.Count;

            //går igjennom alle aksjene 
            for (int i = 0; i < antallAksjer; i++)
            {
                HenteData(ListContainingAllStockTickersFromFile_Alle_aksjer[i]);
            }

            //sorterer alle aksjer i ListeMedCalkulerteDataForAlleAksjer
            addHtml();  

            //legger ut alle aksjene som HTML i tabellen
            addHtmlFromAksje();
            tw.WriteLine("</table>");           
            tw.WriteLine("</body>");
            tw.WriteLine("</html>");

            // close the stream
            tw.Close();

            //starte nettleser - vise oversikt over alle aksjene            
            System.Diagnostics.Process.Start(SAVEPATH);
        }
              
  
        void HenteData(string ticker)
        {
            // create reader & open file                     
            AksjeCandleStickForEnAksje.Clear();
            if (ticker == "SAS") ticker = "SAS-NOK";
            TextReader tr = new StreamReader(PathAksjeKurser + ticker + ".txt");
            string[] sp = new string[9];
            string input = null;

            //laster inn kurser for hver dag og legger de i et CandleStick objekt som legges til i CandleStickListe lista
            int teller = 0;
            string date = "";
            Decimal open = 0;
            Decimal high = 0;
            Decimal low = 0;
            Decimal close = 0;
            int volume = 0;
            string value = "";
            while ((input = tr.ReadLine()) != null)
            {
                if (teller > 100) break; //laster inn aksjedata for 500 dager tilbake 
                sp = input.Split(',');
                date = sp[0].Trim();
                open = Decimal.Parse(sp[3].Replace('.', ','));
                high = Decimal.Parse(sp[4].Replace('.', ','));
                low = Decimal.Parse(sp[5].Replace('.', ','));
                close = Decimal.Parse(sp[6].Replace('.', ','));
                volume = int.Parse(sp[7]);
                value = sp[8];
                AksjeCandleStickForEnAksje.Add(new CandleStick(date, open, high, low, close, volume, value));
                teller++;
            }
            if (teller < 101)
            {
                for (int i = teller; i < 100; i++)
                {
                    AksjeCandleStickForEnAksje.Add(new CandleStick(date, low, low, low, low, 0, ""));
                }
            }
            tr.Close();                      

            AksjeInfoBacktesting currentAksje = new AksjeInfoBacktesting();
            currentAksje.TICKER = ticker;
            currentAksje.OMSETNING = double.Parse(AksjeCandleStickForEnAksje[0].value);
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(40, 0));
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(20, 0));
            
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(20, 15));
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(15, 10));
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(10, 5));
            currentAksje.ProsentAkumulert.Add(regnUtProsentAkumulert(5, 0));

            currentAksje.VOLATILITET = finnVolatiliteten();
           
            ListeMedCalkulerteDataForAlleAksjer.Add(currentAksje);           
        }


        double finnVolatiliteten()
        {
            double totalVolatilitet = 0;
            double tempVolatilitet = 0;
            for (int i = 0; i < 20; i++)
            {
                tempVolatilitet = (double) ( (AksjeCandleStickForEnAksje[i].high - AksjeCandleStickForEnAksje[i].low) / AksjeCandleStickForEnAksje[i].low) * 100;
                totalVolatilitet += tempVolatilitet;
            }

            totalVolatilitet = totalVolatilitet / 20;
            return totalVolatilitet;
        }


        void addHtml()
        {
            //ny rad inneholder ticker og akkumulertprosentvisstigning + omsetning         
            ListeMedCalkulerteDataForAlleAksjer.Sort(delegate(AksjeInfoBacktesting a1, AksjeInfoBacktesting a2)
            {
                return a1.ProsentAkumulert[5].CompareTo(a2.ProsentAkumulert[5]);
                //return a1.VOLATILITET.CompareTo(a2.VOLATILITET);
            });
            ListeMedCalkulerteDataForAlleAksjer.Reverse();
        }
        

        void addHtmlFromAksje()
        {
            int antallAksjer = ListeMedCalkulerteDataForAlleAksjer.Count;
            for (int i = 0; i < antallAksjer; i++)
            {
                double volatilitet = ListeMedCalkulerteDataForAlleAksjer[i].VOLATILITET;
                double omsetning = ListeMedCalkulerteDataForAlleAksjer[i].OMSETNING;
                string ticker = ListeMedCalkulerteDataForAlleAksjer[i].TICKER;
                int omsetn_grense = 1000000;
                if (omsetning < omsetn_grense)
                {
                    //ticker
                    tw.WriteLine("<tr>");
                    tw.WriteLine("<td ALIGN = CENTER>");
                    tw.WriteLine("<FONT COLOR=\"lightgray\"><del>" + ListeMedCalkulerteDataForAlleAksjer[i].TICKER + "</del></FONT>");
                    tw.WriteLine("</td>");
                }
                else
                {
                    //ticker
                    tw.WriteLine("<tr>");
                    tw.WriteLine("<td ALIGN = CENTER>");
                    tw.WriteLine("<FONT COLOR=\"black\">" + ListeMedCalkulerteDataForAlleAksjer[i].TICKER + "</FONT>");
                    tw.WriteLine("</td>");                  
                }  
              
                int antall = ListeMedCalkulerteDataForAlleAksjer[i].ProsentAkumulert.Count;

                for (int x = 0; x < antall; x++)
                {
                    if (omsetning < omsetn_grense)
                    {
                        tw.WriteLine("<td ALIGN = CENTER>");
                        tw.WriteLine("<FONT COLOR=\"lightgray\"><del>" + ListeMedCalkulerteDataForAlleAksjer[i].ProsentAkumulert[x].ToString("F2") + " %</del></FONT>");
                        tw.WriteLine("</td>");
                    }
                    else
                    {    
                        tw.WriteLine("<td ALIGN = CENTER>");
                        tw.WriteLine("<FONT COLOR=\"black\">" + ListeMedCalkulerteDataForAlleAksjer[i].ProsentAkumulert[x].ToString("F2") + " %</FONT>");
                        tw.WriteLine("</td>");
                    }                  
                }                

                if (omsetning < omsetn_grense)
                {
                    tw.WriteLine("<td ALIGN = CENTER>");
                    tw.WriteLine("<FONT  COLOR=\"lightgray\"><del>" + omsetning.ToString("C") + "</del></FONT>");
                    tw.WriteLine("</td>");
                }
                else
                {
                    tw.WriteLine("<td ALIGN = CENTER>");
                    tw.WriteLine("<FONT COLOR=\"black\">" + omsetning.ToString("C") + "</FONT>");
                    tw.WriteLine("</td>");

                    tw.WriteLine("<td ALIGN = CENTER>");
                    tw.WriteLine("<FONT COLOR=\"black\">" + volatilitet.ToString("f2") + "  %</FONT>");
                    tw.WriteLine("</td>");
                }
                tw.WriteLine("</tr>");
            }
        }
        
        
        //Finne høyeste verdi
        double GetMax(System.Collections.Generic.List<double> DoubleCollection)
        {
            double max = double.MinValue;
            foreach (double i in DoubleCollection)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            return max;
        }
        

        double finneProsentvisendring(double laveste, double hoyeste)
        {
            return ((hoyeste - laveste) / laveste) * 100;
        }
        

        void GetAllTickersFromFile()  //går igjennom alle aksjene og legger de i ListContainingAllStockTickersFromFile_Alle_aksjer
        {
            StreamReader re = File.OpenText(Path_Alle_aksjer_Liste);
            string[] splittetStreng = new string[4];
            string input = null;
            while ((input = re.ReadLine()) != null)
            {
                splittetStreng = input.Split('|');
                ListContainingAllStockTickersFromFile_Alle_aksjer.Add(splittetStreng[2].Trim());
            }
        }


        double regnUtProsentAkumulert(int fraDagHigh, int tilDagLow)
        {
            double akumulertOppgang = 0;
            double akumulertNedgang = 0;
            double open = 0;
            double close = 0;

            for (int i = tilDagLow; i < fraDagHigh; i++)
            {
                open = (double)AksjeCandleStickForEnAksje[i].open;
                close = (double)AksjeCandleStickForEnAksje[i].close;

                if (close > open) // CANLDESTICK : OPP
                {
                    akumulertOppgang += ((close - open) / open) * 100;
                }

                else if (close < open) // CANDLESTICK : NED
                {
                    akumulertNedgang += ((close - open) / open) * 100;
                }
            }
            return (akumulertOppgang + akumulertNedgang);
        }
    }
}
