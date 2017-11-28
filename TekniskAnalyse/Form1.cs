using System;
using System.Diagnostics;
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
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using TekniskAnalyse.ControlLogic;
using TekniskAnalyse.DataModels;
using TekniskAnalyse;
using System.Configuration;

namespace TekniskAnalyse
{    
    public partial class Form1 : Form
    {           
        public List<CandleStick> AksjeCandleStickListe = new List<CandleStick>();
        public List<Decimal> SlowStochasticArray_K_Value = new List<Decimal>();
        public List<Decimal> SlowStochasticArray_D_Value = new List<Decimal>();

        public List<String> ListContainingAllStockNamesAndTickers = new List<String>();

        public List<LinjeMedAksjeInfo> ListContainingSameInfoAsFileAlle_Aksjer = new List<LinjeMedAksjeInfo>();

        public List<CandleStick> ValgtAksjeCandleStickListe = new List<CandleStick>();

        public List<Decimal> ValgtAksje_SlowStochasticArray_K_Value = new List<Decimal>();
        public List<Decimal> ValgtAksje_SlowStochasticArray_D_Value = new List<Decimal>();

        public List<Decimal> ValgtAksje_ExpMovingAverageArrayLowValue = new List<Decimal>();
        public List<Decimal> ValgtAksje_ExpMovingAverageArrayHighValue = new List<Decimal>();
        public List<Double> MACDLine = new List<Double>();
        public List<Double> MACDSignalLine = new List<Double>();        

        public List<Decimal> ValgtAksje_MovingAverageArrayLowValue = new List<Decimal>();
        public List<Decimal> ValgtAksje_MovingAverageArrayHighValue = new List<Decimal>();
        public List<Decimal> ValgtAksje_LongMovingAverageArray = new List<Decimal>();                     
        
        private string[] Months = { "XXX", "Jan", "Feb", "Mar", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Des" };
        private int numberOfDaysShown = 100; // 20 dager pr mnd   
        private int WidthCandle = 11;

        private int AntallDagerOpp = 0;
        private int AntallDagerNed = 0;
      
        private decimal akumulertOppgang = 0;
        private decimal akumulertNedgang = 0;

        private decimal akumulertMAE = 0;

        private string SistOppdaterteDato = "";
   
        private Double YesterdayMACD = 0;
        private Double YesterdayMACDSignalLine = 0;

        private Decimal Yesterday;
        private Decimal Yesterday2;
        private Decimal Yesterday3;
        //Main Window størrelse er satt i Properties i Form editor (1280; 760)  
     
        private int MainGraphRightLine = 1188; //x koordinat for å tegne opp linjen på høyre side med verdier 
        private int DrawValgtAksjePriceGraphY_Height = 420;
        private int DrawValgtAksjePriceIndicatorY_Height = 126;
       
        private Decimal startValue_ = 0M;
        private Decimal PixelsPrKrone_ = 0M;
        private Decimal Ypos1_ = 0M;
                         
        private int SelectedStockInList = 0;

        private String PathAksjeKurser = ConfigurationManager.AppSettings["PathAksjeKurser"];
        private String Path_Alle_aksjer_Liste = ConfigurationManager.AppSettings["Path_Alle_aksjer_Liste"];
        private String Path_Alle_aksjer_Grafer = ConfigurationManager.AppSettings["Path_Alle_aksjer_Grafer"];  
                     
        private int ValgtAksje_LongMovingAverage = 45;

        private Graphics g_OSEBX;
        private Graphics g_ValgtAksje;
        private Graphics g_ValgtAksje_indikator;
              

        private enum Indikator
        {
            Volum,
            MACD,
            SlowStochastics
        };

        private int VisibleIndikator = (int)Indikator.Volum;
   
        public Form1()
        {
            Nedlasting.SetPathName(Path_Alle_aksjer_Liste, PathAksjeKurser, Path_Alle_aksjer_Grafer);
            InitializeComponent();
           
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g_OSEBX = Graphics.FromImage(this.pictureBox1.Image);
         
            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            g_ValgtAksje = Graphics.FromImage(this.pictureBox2.Image);
            pictureBox3.Image = new Bitmap(pictureBox3.Width, pictureBox3.Height);
            g_ValgtAksje_indikator = Graphics.FromImage(this.pictureBox3.Image); 
            GetAllTickersFromFile();
            AddTickersToCombobox();            
         
            comboBox1.SelectedIndex = SelectedStockInList; //denne setter den riktige aksjen pga at den fyrer av eventen selectedindex changed
            showAllDataChart();
            showOSEBXChart();
            UpdateDate();
                                    
            //kobler metoden MainForm_Paint til Paint eventen slik at denne funksjonen blir utført hver gang det er behov for å tegne opp vinduet pånytt
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
        }


        private void MainForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawValgtAksjeGraph();

            switch ((Indikator)VisibleIndikator)
            {
                case Indikator.Volum:
                    {
                        DrawValgtAksjeVolumGraph();
                        break;
                    }

                case Indikator.MACD:
                    {
                        DrawValgtAksjeMACDGraph();
                        break;
                    }

                case Indikator.SlowStochastics:
                    {
                        DrawValgtAksjeSlowStochasticGraph();
                        break;
                    }
            }          
        }

  
        private void UpdateDate()
        {
            TextReader tr = new StreamReader(PathAksjeKurser + TickerWatchedNow() + ".txt");
            string[] sp = new string[9];
            string input = tr.ReadLine();            
            sp = input.Split(',');
            SistOppdaterteDato = sp[0].Trim();
            string SistOppdaterteDatoTemp = sp[0].Trim();
            string year = SistOppdaterteDatoTemp.Substring(0, 4);
            string month = SistOppdaterteDatoTemp.Substring(4, 2);
            string day = SistOppdaterteDatoTemp.Substring(6, 2);
            SistOppdaterteDatoTemp = day + "." + month + "." + year;
            tr.Close();
            label1.Text = "Oppdatert: " + SistOppdaterteDatoTemp;
        }

        private void GetAllTickersFromFile()  //går igjennom alle aksjene og legger de i ListStocks
        {
            ListContainingAllStockNamesAndTickers.Clear();
            ListContainingSameInfoAsFileAlle_Aksjer.Clear();

            StreamReader re = File.OpenText(Path_Alle_aksjer_Liste);
            string[] splittetStreng = new string[2];
            string input = null;
            string temp;
            while ((input = re.ReadLine()) != null)
            {
                temp = input;
                splittetStreng = input.Split('|');
                ListContainingAllStockNamesAndTickers.Add(splittetStreng[0].Trim() + " " + splittetStreng[2].Trim() + " - " + splittetStreng[1].Trim());
                          
               
                LinjeMedAksjeInfo Line = new LinjeMedAksjeInfo();
                Line.checkedForFavourite = splittetStreng[0].Trim();
                Line.name = splittetStreng[1].Trim();
                Line.ticker = splittetStreng[2].Trim();
                Line.movingaverageLow = int.Parse(splittetStreng[3].Trim());
                Line.movingaverageHigh = int.Parse(splittetStreng[4].Trim());
                Line.dato = splittetStreng[5].Trim();
                ListContainingSameInfoAsFileAlle_Aksjer.Add(Line);               
            }
            re.Close();
        }
      
        
        private void AddTickersToCombobox()
        {
            //Legger alle tickers + aksjenavn i en combobox slik at vi kan velge hvilken aksje som skal vises på skjermen
            foreach (String s in ListContainingAllStockNamesAndTickers)
            {               
                comboBox1.Items.Add(s);                
            }
        }


        private void LoadChartDataForTicker(string ticker, List<CandleStick> CandleStickListe)
        {    

            // create reader & open file                     
            CandleStickListe.Clear();          

            if (ticker == "SAS") ticker = "SAS-NOK";                      

            TextReader tr = new StreamReader(PathAksjeKurser + ticker + ".txt");
            string [] sp = new string [9];           
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
                if (teller > 500) break; //laster inn aksjedata for 500 dager tilbake kun 300 dager som vises på skjermen
                sp = input.Split(',');
                date = sp[0].Trim();               
                open = Decimal.Parse(sp[3].Replace('.', ','));
                high = Decimal.Parse(sp[4].Replace('.', ','));
                low = Decimal.Parse(sp[5].Replace('.', ','));
                close = Decimal.Parse(sp[6].Replace('.', ','));
                volume = int.Parse(sp[7]);
                value = sp[8];

                CandleStickListe.Add(new CandleStick(date, open, high, low, close, volume, value));
                teller++;        
            }
            if (teller < 501)
            {
                for (int i = teller; i < 500; i++)
                {
                    CandleStickListe.Add(new CandleStick(date, low, low, low, low, 0, ""));
                }
            }
            tr.Close();
        }              
                
        //Generer glidende gjennomsnitt
        private void FindMovingAvegage(int MA_Days, List<Decimal> MovingAverageArray, List<CandleStick> CandleStickListe)
        {           
            MovingAverageArray.Clear();
            int limit = 0;
            Decimal total = 0;
            Decimal MovingAverageValue = 0;
            for (int start = 0; start < numberOfDaysShown; start++)
            {
                total = 0;
                limit = start + MA_Days;
                for (int index = start; index < limit; index++)
                {
                    total += CandleStickListe[index].close;
                }
                MovingAverageValue = total / MA_Days;
                MovingAverageArray.Add(MovingAverageValue);
            }
        }              
                 
        
        //*************************************************************************************************************************************************
        //*************************************************************************************************************************************************
        //*************************************************************************************************************************************************
        //*************************************************************************************************************************************************
        //*************************************************************************************************************************************************
        //*************************************************************************************************************************************************
        

         //Finne nærmeste verdi under lav med riktig intervall og riktig avrunding
         private string GetRoundedLowValue(Decimal inputValue, Decimal range)
         {
             Decimal divisions = 0;
             Decimal rounding = 0;
             //0 - 1
             if ((range >= 0M) && (range < 1M))
             {
                 divisions = 0.05M;
             }
             //1 - 2,5
             else if ((range >= 1M) && (range < 2.5M))
             {
                 divisions = 0.1M;
             }
             //2,5 - 6
             else if ((range >= 2.5M) && (range < 6M))
             {
                 divisions = 0.25M;
             }
             //6 - 12
             else if ((range >= 6M) && (range < 12M))
             {
                 divisions = 0.5M;
             }
             //12 - 20
             else if ((range >= 12M) && (range < 20M))
             {
                 divisions = 1.0M;
             }
             //20 - 50
             else if ((range >= 20M) && (range < 50M))
             {
                 divisions = 2.0M;
             }
             //50 - 100
             else if ((range >= 50M) && (range < 100M))
             {
                 divisions = 5.0M;
             }
             //100 - 200
             else if ((range >= 100M) && (range < 200M))
             {
                 divisions = 10.0M;
             }
             //200 - 300
             else if ((range >= 200M) && (range < 300M))
             {
                 divisions = 15.0M;
             }
             //300 - 400
             else if ((range >= 300M) && (range < 400M))
             {
                 divisions = 20.0M;
             }
             //400 - 1000
             else if ((range >= 300M) && (range < 400M))
             {
                 divisions = 25.0M;
             }
             rounding = (Decimal)1M / divisions;
             int v = (int)(inputValue * rounding);
             Decimal s = (Decimal)v / rounding;
             return s.ToString("f") + "|" + divisions;
         }


         //*************************************************************************************************************************************************
         //*************************************************************************************************************************************************
         //************************** Keyboard interaction  ************************************************************************
         //*************************************************************************************************************************************************
         //*************************************************************************************************************************************************
         //*************************************************************************************************************************************************

                     

       
         private void button1_Click(object sender, EventArgs e)
         {
             Font myFont = new Font("Times New Roman", 10);
             SolidBrush myBrush = new SolidBrush(Color.Black);
             //g_OSEBX.DrawString("test", myFont, myBrush,200, 400);
             pictureBox1.Refresh();
             string date = String.Format("{0:d/MMM/yyyy}", DateTime.Now).Replace('.','-');
             //SaveAsBitmap(panel1, PathAksjeUtskrifter + TickerWatchedNow() + "  " + date + ".bmp");
         }

         public void SaveAsBitmap(Control control, string fileName)
         {
             //get the instance of the graphics from the control        
             //Graphics g = control.CreateGraphics();                
             //new bitmap object to save the image        
             Bitmap bmp = new Bitmap(control.Width, control.Height);

             //Drawing control to the bitmap        
             control.DrawToBitmap(bmp, new Rectangle(0, 0, control.Width, control.Height));
             bmp.Save(fileName);
             bmp.Dispose();
         }


         private void showOSEBXChart()
         {
             string f = Path_Alle_aksjer_Grafer + "\\OSEBX.gif";
             Bitmap imgTemp = new Bitmap(f);

             Rectangle bounds = new Rectangle(5, 15, 140, 65);
             pictureBox4.Image = cropImage(imgTemp, bounds);
         }


        private string getTicker(string Ticker)
        {           
             int index = Ticker.IndexOf("-");
             Ticker = Ticker.Substring(0, index);
             index = Ticker.IndexOf(" ");
             Ticker = Ticker.Substring(index, Ticker.Length - index);
             Ticker = Ticker.Trim();
             return Ticker;
        }


        private string TickerRemoveCheck(string Ticker)
        {          
            int index = Ticker.IndexOf(" ");
            if (index > 0)
            {
                Ticker = Ticker.Substring(index, Ticker.Length - index);
                Ticker = Ticker.Trim();
            }
            return Ticker;
        }

       
         private void showAllDataChart()
         {
             String Ticker = comboBox1.SelectedItem.ToString();
             Ticker = getTicker(Ticker);         
             string f = Path_Alle_aksjer_Grafer + "\\" + Ticker + ".gif";
             Bitmap imgTemp = new Bitmap(f);

             Rectangle bounds = new Rectangle(5, 17, 225, 63);
             pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;

             pictureBox1.Image = cropImage(imgTemp, bounds);              
        }


         private Image cropImage(Image img, Rectangle cropArea)
         {
             Bitmap bmpImage = new Bitmap(img);
             Bitmap bmpCrop = bmpImage.Clone(cropArea,
             bmpImage.PixelFormat);
             return (Image)(bmpCrop);
         }


         // Starter oppdatering av Combobox
         private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
         {
             UpdateADR();        
         }
        

         private void UpdateADR()
         {
             SelectedStockInList = comboBox1.SelectedIndex;
             comboBox1.Refresh();
             showAllDataChart();
             updateCharts();
             decimal res1 = ValgtAksjeCandleStickListe[0].close;

             label7.Text = "før vi kommer til " + getCrossingPrice((double)res1).ToString("f2") + " kr.";

             decimal result = (decimal)2 * (decimal)GetAverageDailyRange();
             result = decimal.Round(result, 2);
             decimal result2 = 0;
             if (res1 != 0)
             {
                 result2 = ((result / res1) * (decimal)100);
             }
             if (result2 > 10)
             {
                 label4.ForeColor = Color.Red;
             }
             else
             {
                 label4.ForeColor = Color.Black;
             }

             label4.Text = "2 x ADR " + result.ToString("f") + " kr (" + result2.ToString("f") + " %)";
             labelSLvalue.Text = "Stopp-Loss " + (res1 - result).ToString("f") + " kr";

             if (result <= 0)
             {
                 result = 1;
             }
            int maksTap = 0;

                       

            if (Utilities.Utilities.IsNumber(textBoxMaksTap.Text))
            {
                maksTap = int.Parse(textBoxMaksTap.Text);
                int antall = (int)((decimal)maksTap / result);
                int pris = (int)(antall * res1);
                label2.Text = "Totalpris " + pris.ToString("f") + " kr";
                label3.Text = "Antall " + antall;

                UpdateDate();

                updateTextBoxesWithMovingAverageValues();
                this.Refresh();
            }
            else
            {
                MessageBox.Show("Du må legge inn et heltall for Maks tap.", "Important Message");
            }                         
         }


         private void updateTextBoxesWithMovingAverageValues()
         {          
             textBoxGul.Text = ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow.ToString();
             textBoxRosa.Text =  ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh.ToString();       
         }
        
     
         private void updateCharts()
         {          
             LoadChartDataForTicker(TickerWatchedNow(), ValgtAksjeCandleStickListe);

             CalculateEMA(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow, ValgtAksje_MovingAverageArrayLowValue);
             //FindMovingAvegage(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow, ValgtAksje_MovingAverageArrayLowValue, ValgtAksjeCandleStickListe); 
             CalculateEMA(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh, ValgtAksje_MovingAverageArrayHighValue);
             //FindMovingAvegage(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh, ValgtAksje_MovingAverageArrayHighValue, ValgtAksjeCandleStickListe);
             FindMovingAvegage(ValgtAksje_LongMovingAverage, ValgtAksje_LongMovingAverageArray, ValgtAksjeCandleStickListe); 
         }
        


         private String TickerWatchedNow()
         {
             SelectedStockInList = comboBox1.SelectedIndex;
             String Ticker = comboBox1.SelectedItem.ToString();
             if (!Ticker.Contains("SAS-NOK"))
             {
                 int index = Ticker.IndexOf("-");
                 Ticker = Ticker.Substring(0, index);
                 Ticker = Ticker.Trim();
             }
             else
             {
                 Ticker = "SAS-NOK";
             }

             Ticker = TickerRemoveCheck(Ticker);

             return Ticker;
         }
        

         private void mndToolStripMenuItem_Click(object sender, EventArgs e)
         {
             WidthCandle = 3;
             numberOfDaysShown = 300;
         //    open_close_width = 1;
             showAllDataChart();
             updateCharts();
             this.Refresh();
         }
                   

         private void mndToolStripMenuItem1_Click(object sender, EventArgs e)
         {
             WidthCandle = 5;
             numberOfDaysShown = 200;
          //   open_close_width = 2;
             showAllDataChart();
             updateCharts();
             this.Refresh();
         }


         private void mndToolStripMenuItem2_Click(object sender, EventArgs e)
         {
             WidthCandle = 7;
             numberOfDaysShown = 150;
         //    open_close_width = 2;
             showAllDataChart();
             updateCharts();
             this.Refresh();
         }


         private void mndToolStripMenuItem3_Click(object sender, EventArgs e)
         {
            WidthCandle = 9;
            numberOfDaysShown = 120;
       //     open_close_width = 2;
            showAllDataChart();
            updateCharts();
            this.Refresh();
         }


         private void mndToolStripMenuItem4_Click(object sender, EventArgs e)
         {
            WidthCandle = 11;
            numberOfDaysShown = 100;
        //    open_close_width = 2;
            showAllDataChart();
            updateCharts();
            this.Refresh();
         }


         private void hentSisteKurserToolStripMenuItem_Click(object sender, EventArgs e)
         {
            Nedlasting.GetStocks(label1);
            //laster inn data for OSEBX på nytt etter at vi har lastet ned flere data          

            LoadChartDataForTicker(TickerWatchedNow(), ValgtAksjeCandleStickListe); //laster inn data for ValgtAksje  
            CalculateEMA(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow, ValgtAksje_MovingAverageArrayLowValue);
            //FindMovingAvegage(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow, ValgtAksje_MovingAverageArrayLowValue, ValgtAksjeCandleStickListe);
            CalculateEMA(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh, ValgtAksje_MovingAverageArrayHighValue);
            //FindMovingAvegage(ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh, ValgtAksje_MovingAverageArrayHighValue, ValgtAksjeCandleStickListe);
            FindMovingAvegage(ValgtAksje_LongMovingAverage, ValgtAksje_LongMovingAverageArray, ValgtAksjeCandleStickListe);

            UpdateDate();
            this.Refresh();
            panel1.Refresh();
        }


        private void lagreGrafSomBildeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font myFont = new Font("Times New Roman", 10);
            SolidBrush myBrush = new SolidBrush(Color.Black);       
            pictureBox1.Refresh();        
	        PrintDocument pd = new PrintDocument();
	        pd.PrintPage += new PrintPageEventHandler(PrintImage);
            pd.DefaultPageSettings.Landscape = true;
	        pd.Print();  
        }


        void PrintImage(object o, PrintPageEventArgs e)
        {
	        int x = SystemInformation.WorkingArea.X;
	        int y = SystemInformation.WorkingArea.Y;
	        int width = this.Width;
	        int height = this.Height; 
	        Rectangle bounds = new Rectangle(x, y, width, height); 
	        Bitmap img = new Bitmap(width, height);  
            panel1.DrawToBitmap(img, bounds);
	        Point p = new Point(0, 150);
            e.Graphics.DrawImage(resizeImage(img, new Size(1105, 876 )), p);    
        }
        

        private static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
            }
            else
            {
                nPercent = nPercentW;
            }
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (Image)b;
        }

       
         private void åpneIBigchartscomToolStripMenuItem_Click(object sender, EventArgs e)
         {
             viseHeleGrafen();
         }

         private void viseHeleGrafen()
         {
             string TICKER = TickerWatchedNow();
             string f = Path_Alle_aksjer_Grafer + "\\" + TICKER + "_LARGE.gif";
             Process p = new Process();
             p.StartInfo.FileName = "rundll32.exe";
             p.StartInfo.Arguments = @"C:\WINDOWS\System32\shimgvw.dll,ImageView_Fullscreen " + f;
             p.Start();
         }
        
        
         private void finnProsentvisEndringToolStripMenuItem_Click(object sender, EventArgs e)
         {
             // InputBox with value validation - first define validation delegate, which
             // returns empty string for valid values and error message for invalid values
             InputBoxValidation validation = delegate(string val)
             {
                 if (val == "")
                     return "Du må skrive inn en verdi.";
                 if (!Utilities.Utilities.IsNumber(val))
                     return "Dette er ikke er ikke en gyldig verdi.";
                 return "";
             };
             string value = "7";
             if (InputBox.Show("", "Skriv inn antall dager du ønsker", ref value, validation) == DialogResult.OK)
             {               
                 int antallDagerTilbake = int.Parse(value);

                 ProsentvisEndring p = new ProsentvisEndring();
                 p.InitProsentvisEndring(antallDagerTilbake);
             }
         }


         private void finnAkkumulertGevinstToolStripMenuItem_Click(object sender, EventArgs e)
         {
             FortjenesteBacktesting p = new FortjenesteBacktesting();
             p.FinneFortjenesteVedBacktesting();
         }

         private void button1Oppdater_Click_1(object sender, EventArgs e)       
         {
            int EMA_rosa = 0;
            int EMA_gul = 0;

            if (Utilities.Utilities.IsNumber(textBoxRosa.Text) && Utilities.Utilities.IsNumber(textBoxGul .Text))
            {
                EMA_rosa = int.Parse(textBoxRosa.Text);
                EMA_gul = int.Parse(textBoxGul.Text);
                ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageLow = EMA_gul;
                ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].movingaverageHigh = EMA_rosa;
                saveListeMedAlle_Aksjer();
                updateCharts();
                this.Refresh();
            }
            else
            {
                MessageBox.Show("Feltene EMA(rosa) og EMA(gul) må inneholde et heltall.", "Important Message");
            }            
         }       
                 
               
         private int findIndexForSelectedTickerInAlleAksjerListen(string tickerName)
         {
             int index = -1;
           
             for (int i = 0; i < ListContainingAllStockNamesAndTickers.Count; i++)
             {
                 if (ListContainingAllStockNamesAndTickers[i] == tickerName)
                 {
                     index = i;
                     break;
                 }
             }
             return index;
         }       
        

         private bool ConnectionExists()
         {
             try
             {
                 System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient("www.google.com", 80);
                 clnt.Close();
                 return true;
             }
             catch
             {
                 return false;
             }
         }


         private void verktøyToolStripMenuItem_Click(object sender, EventArgs e)
         {
             if (!ConnectionExists())
             {
                 hentSisteKurserToolStripMenuItem.Enabled = false;              
             }
             else //vi har forbindelse det må være mulig å koble til, vi kan laste ned data
             {
                 hentSisteKurserToolStripMenuItem.Enabled = true;            
             }
         }


         private void buttonLeggTil_Click(object sender, EventArgs e)
         {
             String Ticker = comboBox1.SelectedItem.ToString();      
             ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].checkedForFavourite = "\u221A";
             ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].dato = SistOppdaterteDato;
             saveListeMedAlle_Aksjer();
             GetAllTickersFromFile();
             comboBox1.Items.Clear();             
             AddTickersToCombobox();
             comboBox1.SelectedIndex = SelectedStockInList;
             comboBox1.Refresh();
            
            // StoreFavouriteStocksToFile();
         }


         private void saveListeMedAlle_Aksjer()
         {
             TextWriter tw = new StreamWriter(Path_Alle_aksjer_Liste);
             foreach (LinjeMedAksjeInfo Line in ListContainingSameInfoAsFileAlle_Aksjer)
             {
                 // write a line of text to the file
                 string r = Line.checkedForFavourite + "|" + Line.name + "|" + Line.ticker + "|" + Line.movingaverageLow + "|" + Line.movingaverageHigh + "|" + Line.dato;
                 tw.WriteLine(r);
             }
             tw.Close();
         }


         private void buttonFjern_Click(object sender, EventArgs e)
         {
             String Ticker = comboBox1.SelectedItem.ToString();                        
             ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].checkedForFavourite = "";
             ListContainingSameInfoAsFileAlle_Aksjer[SelectedStockInList].dato = "";
             saveListeMedAlle_Aksjer();
             GetAllTickersFromFile();
             comboBox1.Items.Clear();
             AddTickersToCombobox();
             comboBox1.SelectedIndex = SelectedStockInList;
             comboBox1.Refresh();
         }


         private void textBox1_TextChanged(object sender, EventArgs e)
         {
             UpdateADR();
         }

         private void pictureBox1_Click(object sender, EventArgs e)
         {
             viseHeleGrafen();
         }


         private double getCrossingPrice(double startPrice)
         {

             double startPriceFixed = startPrice;

             double added_ten_precent = 0;
             double one_third_of_price = 0;

             double increase = 0.01;
             int counter = 0;
             while (counter < 100000)
             {
                 startPrice += increase;

                 added_ten_precent = startPrice * 0.1;
                 one_third_of_price = (startPrice - startPriceFixed) / 3;

                 if ((added_ten_precent < one_third_of_price))
                 {

                     Console.WriteLine(+startPrice + " 10%: " + added_ten_precent.ToString("c") + " 1/3: " + one_third_of_price.ToString("c"));
                     break;
                 }

                 counter++;
             }        
             return startPrice;
        }
               

        private void volumToolStripMenuItem_Click(object sender, EventArgs e)
        {
        VisibleIndikator = (int)Indikator.Volum;
        this.Refresh();        
        }

        private void mACDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VisibleIndikator = (int)Indikator.MACD;
            this.Refresh();
        }

        private void slowStochasticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VisibleIndikator = (int)Indikator.SlowStochastics;
            this.Refresh();
        }      
            

        private void buttonBacktest_Click(object sender, EventArgs e)
        {
            MAE_Histogram p = new MAE_Histogram();
            int mae = 0;

            if (Utilities.Utilities.IsNumber(textBoxS_Lned.Text))
            {
                mae = int.Parse(textBoxS_Lned.Text);
                p.LageMAE_Histogram(ValgtAksjeCandleStickListe, TickerWatchedNow(), mae, textBox3.Text);
            }
            else
            {
                MessageBox.Show("Du må legge inn et heltall for S. L ned.", "Important Message");
            }
        }        
    }
}


  