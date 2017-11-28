using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;

namespace TekniskAnalyse.ControlLogic
{
    static class Nedlasting
    {
        static List<string> ListStocks = new List<string>();
        static Label L2;
        static String PathAksjeLister;
        static String PathAksjeKurser;
        static String PathAksjeGrafer;
    
        static List<string> ListGlSn = new List<string>();
        static List<string> TickerListe = new List<string>();


        public static void SetPathName(String Lister, String Kurser, String Grafer)
        {
            PathAksjeLister = Lister;
            PathAksjeKurser = Kurser;
            PathAksjeGrafer = Grafer;
        }
        public static void GetStocks(Label label2)
        {
            L2 = label2;
            StreamReader re = File.OpenText(PathAksjeLister);
            string[] splittetStreng = new string[2];
            string input = null;
            //går igjennom alle aksjene og legger de i en liste
            while ((input = re.ReadLine()) != null)
            {
                splittetStreng = input.Split('|');
                ListStocks.Add(splittetStreng[2].Trim());
            }

            //Henter bilde av OSEBX
            string filename = ConfigurationManager.AppSettings["UrlTotalindex"];
            WebRequest req = WebRequest.Create(filename);
            Stream stream = req.GetResponse().GetResponseStream();
            Bitmap img = new Bitmap(stream);
            img.Save(PathAksjeGrafer + "\\OSEBX.gif");
            img.Dispose();
            stream.Dispose();
            stream.Close();                              

            //Nå er vi sikker på at alle aksjene er lagret i listen
            //Vi går igjennom dem og henter ut kurser for hver dag
            foreach (string Ticker in ListStocks)
            {
                HenteData(Ticker);
                downloadAndSaveGraph(Ticker);
                downloadAndSaveLargeGraph(Ticker);
            }
            L2.Text = "Kurser hentet";                
        }
              

        static void downloadAndSaveGraph(string TICKER)
        {
            string EXCHANGE = "OSE";
            if (TICKER == "SAS-NOK") TICKER = "SASNOK";
            if (TICKER == "PLCS") EXCHANGE = "OAX";
            if (TICKER == "") return;
            TICKER = TICKER.Replace("+", "-");                              
            string filename = ConfigurationManager.AppSettings["UrlTickerImages"] + TICKER + "&exchange=" + EXCHANGE + "&from=20100605&to=20101221&period=5000&scale=linear&linewidth=1&height=100&width=235&p_EXPONENTIAL-MEAN.PERCENTAGE=" + 45 + "&p_FORMAT.FORMAT=lines&tas=EXPONENTIAL-MEAN,FORMAT,";
                      
            WebRequest req = WebRequest.Create(filename);
            Stream stream = req.GetResponse().GetResponseStream();
            Bitmap img = new Bitmap(stream);          
            img.Save(PathAksjeGrafer + "\\" + TICKER + ".gif");
            img.Dispose();
            stream.Dispose();
            stream.Close();            
        }


        static void downloadAndSaveLargeGraph(string TICKER)
        {
            string EXCHANGE = "OSE";
            if (TICKER == "SAS-NOK") TICKER = "SASNOK";
            if (TICKER == "PLCS") EXCHANGE = "OAX";
            if (TICKER == "") return;
            TICKER = TICKER.Replace("+", "-");

            string filename = ConfigurationManager.AppSettings["UrlTickerImages"] + TICKER + "&exchange=" + EXCHANGE + "&from=20100605&to=20101221&period=5000&scale=linear&linewidth=1&height=350&width=700&p_EXPONENTIAL-MEAN.PERCENTAGE=" + 45 + "&p_FORMAT.FORMAT=lines&tas=EXPONENTIAL-MEAN,FORMAT,";

            WebRequest req = WebRequest.Create(filename);
            Stream stream = req.GetResponse().GetResponseStream();
            Bitmap img = new Bitmap(stream);
            img.Save(PathAksjeGrafer + "\\" + TICKER + "_LARGE.gif");
            img.Dispose();
            stream.Dispose();
            stream.Close();
        }
                
        static void HenteData(string ticker)
        {
            if (ticker == "") return;
            L2.Text = "Henter " + ticker + "                     ";
            L2.Update();
            ticker = ticker.Replace("+", "-");
            string sURL = ConfigurationManager.AppSettings["UrlTickerImages"] + ticker + ".OSE&csv_format=csv";
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(sURL);
            WebProxy myProxy = new WebProxy("myproxy", 80);
            myProxy.BypassProxyOnLocal = true;
            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);

            StreamWriter sr = new StreamWriter(PathAksjeKurser + ticker + ".txt");              
                       
            string input = objReader.ReadLine();
            //går igjennom alle aksjene og legger de i en liste
            int teller = 0;
            while ((input = objReader.ReadLine()) != null)
            {
                if (teller > 500) break;
                sr.WriteLine(input);
                teller++;                
            }
                    
            //lukker koblinger
            sr.Close();
            objStream.Close();
            objReader.Close();                      
        }
    }        
}