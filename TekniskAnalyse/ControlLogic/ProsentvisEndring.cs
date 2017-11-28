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
    public class ProsentvisEndring
    {
        List<string> ListContainingAllStockTickersFromFile_Alle_aksjer = new List<string>();                
        List<Double> sluttkursArray = new List<double>();
        List<string> datoArray = new List<string>();
        List<string> navnArray = new List<string>();

        //lagring av aksjeinfo objekter for sortering
        List<AksjeInfo> aksje = new List<AksjeInfo>();

        TextWriter tw = null;    
        bool firstTime = true;
        int antallDagerTilbake = 0;

        String PathAksjeKurser = ConfigurationManager.AppSettings["PathAksjeKurser"];
        string SAVEPATH = "";
        string Path_Alle_aksjer_Liste = ConfigurationManager.AppSettings["Path_Alle_aksjer_Liste"];

        public void InitProsentvisEndring(int antallDagerTilbake)
        {
            this.antallDagerTilbake = antallDagerTilbake;
            ListContainingAllStockTickersFromFile_Alle_aksjer = new List<string>();

            sluttkursArray = new List<double>();
            datoArray = new List<string>();
            navnArray = new List<string>();

            //lagring av aksjeinfo objekter for sortering
            aksje = new List<AksjeInfo>();

            tw = null;    
            firstTime = true;
          //  antallDagerTilbake = 7;
                      
            SAVEPATH = @"C:\Kurser siste " + antallDagerTilbake + @" dager\";            
                      
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
            tw.WriteLine("<H1>Kurser siste " + antallDagerTilbake + " dager</H1><br/><br/>");
            tw.WriteLine("Sist oppdatert " + DateTime.Now.ToString());            
            tw.WriteLine("</head>");
            tw.WriteLine("<body>");
            tw.WriteLine("<table width=\"100%\" border=\"1\">");          
          
            int antallAksjer = ListContainingAllStockTickersFromFile_Alle_aksjer.Count;

            //går igjennom alle aksjene 
            for (int i = 0; i < antallAksjer; i++)
            {
                HenteData(ListContainingAllStockTickersFromFile_Alle_aksjer[i]);
            }           
            addHtmlFromAksje();
           
            tw.WriteLine("</table>");
            tw.WriteLine("<body>");
            tw.WriteLine("</body>");
            tw.WriteLine("</html>");
            // close the stream
            tw.Close();

            //starte nettleser - vise oversikt over aksjer med kjøps salgs signaler            
            System.Diagnostics.Process.Start(SAVEPATH);
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
        

        void HenteData(string ticker)
        {
            if (ticker == "") return;         
            ticker = ticker.Replace("+", "-");
            sluttkursArray = new List<double>();
            datoArray = new List<string>();
            navnArray = new List<string>();

            if (ticker == "SAS") ticker = "SAS-NOK";
            TextReader tr = new StreamReader(PathAksjeKurser + ticker + ".txt");

            string sLine = "";
            string dato = "";

            List<Double> tempSluttkursArray = new List<double>();
            List<string> tempDatoArray = new List<string>();

            string[] splittetStreng;
          
            double omsetning = 0;
            int teller = 0;
           // double variasjonkurs = 0;

            bool emptyFile = false;
            
            while ((sLine != null)&& (emptyFile == false) && (teller < antallDagerTilbake))
            {
                sLine = tr.ReadLine();
                if (sLine != null)
                {
                    //quote_date,paper,exch,open,high,low,close,volume,value
                    splittetStreng = sLine.Split(',');
                    omsetning += (double.Parse(splittetStreng[8].Replace('.', ',')));
                    dato = splittetStreng[0];
                    dato = dato.Substring(6, 2) + "." + dato.Substring(4, 2) + "." + dato.Substring(2, 2);
                    tempDatoArray.Add(dato);
                    tempSluttkursArray.Add(double.Parse(splittetStreng[6].Replace('.', ',')));
                }
                else
                {
                    emptyFile = true;
                }
                    
                teller++;
            }

            //nå er array fylt med sluttkurser og datoer
            tr.Close();

            //snur rekkefølgen i array
            int size = tempSluttkursArray.Count;
            int counter = 0;
            for (int i = 0; i < size; i++)
            {
                counter = size - 1 - i;
                sluttkursArray.Add(tempSluttkursArray[counter]);
                datoArray.Add(tempDatoArray[counter]);
            }
            //nå er alle aksjene analysert og salgs og kjøps aksjer ligger lagreti i tabeller
            addHtml(ticker, omsetning/antallDagerTilbake);
        }


        double finneProsentvisendring(double laveste, double hoyeste)
        {
            return ((hoyeste - laveste) / laveste) * 100;
        }
        

        void addHtmlFromAksje()
        {
            int antallAksjer = aksje.Count;
            for (int teller = 0; teller < antallAksjer; teller++)
            {
             //   double volatilitet = aksje[teller].VOLATILITET;
                double omsetning = aksje[teller].OMSETNING;
                string ticker = aksje[teller].TICKER;
                
                //ticker
                tw.WriteLine("<tr>");
                tw.WriteLine("<td ALIGN = LEFT>");
                tw.WriteLine("<a href=http://bigcharts.marketwatch.com/advchart/frames/frames.asp?symb=no%3A" + ticker + "&time=7&freq=1>");
                tw.WriteLine("<FONT COLOR=\"blue\">" + ticker + "</FONT>");
                tw.WriteLine("</a href></td>");
              
                sluttkursArray = new List<double>();
                int kapasitet = aksje[teller].sluttkursArray.Count;
                //setter opp sluttkurser for hver dag
                for (int i = 0; i < kapasitet; i++)
                {
                    if (aksje[teller].PROSENTVISSTIGNING5dg > 15)
                    {
                        tw.WriteLine("<td ALIGN = RIGHT BGCOLOR=\"red\">");
                        tw.WriteLine(aksje[teller].sluttkursArray[i].ToString("F2"));
                        tw.WriteLine("</td>");
                    }
                    else
                    {
                        tw.WriteLine("<td ALIGN = RIGHT>");
                        tw.WriteLine(aksje[teller].sluttkursArray[i].ToString("F2"));
                        tw.WriteLine("</td>");
                    }
                }
                //5 dager
                if (aksje[teller].PROSENTVISSTIGNING5dg > 0)
                {
                    tw.WriteLine("<td ALIGN = RIGHT>");
                    tw.WriteLine("<FONT COLOR=\"green\">" + aksje[teller].PROSENTVISSTIGNING5dg.ToString("F2") + "%" + "</FONT>");
                    tw.WriteLine("</td>");
                }
                else
                {
                    tw.WriteLine("<td ALIGN = RIGHT>");
                    tw.WriteLine("<FONT COLOR=\"red\">" + aksje[teller].PROSENTVISSTIGNING5dg.ToString("F2") + "%" + "</FONT>");
                    tw.WriteLine("</td>");
                }
                           

                if (omsetning < 500000)
                {
                    tw.WriteLine("<td ALIGN = RIGHT>");
                    tw.WriteLine("<FONT  COLOR=\"red\">" + omsetning.ToString("C") + "</FONT>");
                    tw.WriteLine("</td>");
                }
                else
                {
                    tw.WriteLine("<td ALIGN = RIGHT>");
                    tw.WriteLine("<FONT COLOR=\"blue\">" + omsetning.ToString("C") + "</FONT>");
                    tw.WriteLine("</td>");
                }
                tw.WriteLine("</tr>");
            }
        }
        

        void addHtml(string ticker, double omsetning)
        {
            int kapasitet = sluttkursArray.Count;

            if (kapasitet > 0)
            {
                //setter opp datoer kun første gang 
                if (firstTime)
                {
                    tw.WriteLine("<tr>");
                    tw.WriteLine("<td>");
                    tw.WriteLine("Navn");
                    tw.WriteLine("</td>");

                    for (int i = 0; i < kapasitet; i++)
                    {
                        tw.WriteLine("<td>");
                        tw.WriteLine(datoArray[i].ToString());
                        tw.WriteLine("</td>");
                    }                                     
                    tw.WriteLine("<td>");
                    tw.WriteLine("<center> +/- " + antallDagerTilbake + "dg </center>");
                    tw.WriteLine("</td>");                                    
                    
                    tw.WriteLine("<td>");
                    tw.WriteLine("Snitt omsetning siste " + antallDagerTilbake + " dager");                   
                    tw.WriteLine("</td>");

                    tw.WriteLine("</tr>");
                    firstTime = false;
                }

                //ny rad inneholder ticker og sluttkurser prosentvisstigning og omsetning
                AksjeInfo valgtAksje = new AksjeInfo();
                valgtAksje.OMSETNING = omsetning;
                int sisteIndex = sluttkursArray.Count - 1;

                if (sluttkursArray.Count > (antallDagerTilbake - 1))
                {
                    valgtAksje.PROSENTVISSTIGNING5dg = finneProsentvisendring(sluttkursArray[sisteIndex - (antallDagerTilbake - 1)], sluttkursArray[sisteIndex]);
                }               
                valgtAksje.TICKER = ticker;              

                //setter opp sluttkurser for hver dag
                for (int i = 0; i < kapasitet; i++)
                {
                    valgtAksje.sluttkursArray.Add(sluttkursArray[i]);
                }
                aksje.Add(valgtAksje);
                aksje.Sort(delegate(AksjeInfo a1, AksjeInfo a2)
                    {
                        return a1.PROSENTVISSTIGNING5dg.CompareTo(a2.PROSENTVISSTIGNING5dg);
                    });
                aksje.Reverse();
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
    }        
}
