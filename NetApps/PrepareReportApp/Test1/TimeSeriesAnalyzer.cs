using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Test1
{
/*
    public class FinancialInstrument
    {
        public string Name;
        public string ShortName;
        private ObservationContainer observationSet;
        public InstrumentPerformance Performance = new InstrumentPerformance();

        public FinancialInstrument()
        {
            observationSet = new ObservationContainer();
        }

        internal void AddObservation(DateTime dtNow, double observationVal)
        {
            observationSet.AddObservation(dtNow, observationVal);
        }

        internal void AnalyzeFromTo(DateTime dtStartToConsider, DateTime dtNow)
        {
            Performance.DtFrom = dtStartToConsider;
            Performance.DtTo = dtNow;

            var observationsToAnalyze = observationSet.GetSetOfObservations(dtStartToConsider, dtNow);
            int iLong = -1;
            int iMiddle = -1;
            int iShort = -1;

            int secBeforeShort = TimeSeriesAnalyzer.smallSpanInSec;
            int secBeforeMiddle = TimeSeriesAnalyzer.smallSpanInSec * TimeSeriesAnalyzer.nSmallSpansInMiddle;
            int secBeforeLong = secBeforeMiddle * TimeSeriesAnalyzer.nMiddleSpansInLong;
            DateTime lastObservation = observationsToAnalyze[observationsToAnalyze.Count - 1].crtDateTime;
            DateTime dtShort = lastObservation.AddSeconds(-secBeforeShort);
            DateTime dtMiddle = lastObservation.AddSeconds(-secBeforeMiddle);
            DateTime dtLong = lastObservation.AddSeconds(-secBeforeLong);
            for (int i = 0; i< observationsToAnalyze.Count; i++)
            {
                var obs = observationsToAnalyze[observationsToAnalyze.Count - 1 - i];
                if (obs.crtDateTime <= dtShort && iShort == -1)
                {
                    iShort = i;
                }
                else if (obs.crtDateTime <= dtMiddle && iMiddle == -1)
                {
                    iMiddle = i;
                }
                else if (obs.crtDateTime <= dtLong && iLong == -1)
                {
                    iLong = i;
                    break;
                }
                Performance.SetDefault(observationsToAnalyze[observationsToAnalyze.Count - 1].val);
                if(iShort >= 0)
                    Performance.lastPeriodPrice = observationsToAnalyze[iShort].val;
                if(iMiddle >= 0)
                    Performance.middlePeriodPrice = observationsToAnalyze[iMiddle].val;
                if (iLong >= 0)
                    Performance.longPeriodPrice = observationsToAnalyze[iLong].val;
            }

        }
    }


    public class TimeSeriesAnalyzer
    {
        Timer _timer; // From System.Timers
        static public string observationFilePath = "";
        static public string observationFilePattern = "%CryptoName%";

        static public int smallSpanInSec = 3;              // how often the values are taken in sec
        static public int nSmallSpansInMiddle = 10;         // how many small spans are contained in middle set
        static public int nMiddleSpansInLong = 10;         // how many small spans are contained in middle set

        public int analysisFrequencyInSmallSpans = 10;     // how many small spans should elapse to start the analysis

        public int nPlus = 3;       // number of going up in a row to consider grow
        public int nMinus = 5;      // number of going down in a row to consider fall
        public double vPlusPerc = 5.0; // percentage of essential grow
        public double vMinusPerc = 5.0;         // percentage of essential down
        public double vMinDeltaPerc = 0.01;     // percentage of minimal difference
        public string ErrorMsg = string.Empty;
        private DateTime lastTimeAnalyzed = DateTime.MinValue;

        private Dictionary<string, FinancialInstrument> instruments = new Dictionary<string, FinancialInstrument>();

        // the ideas to consider values:
        // dv+(i) - average plus or minus in value for middle span as set of short spans values
        // DV+(i) - average plus or minus in value for middle span as set of short spans values, 
        //                  but excluding lowest and highest
        // usage of Analyzer:
        // new...
        // by timer: Analyze() -> returns or generates ready-to-plot observation sets

        TimeSeriesAnalyzer(List<string> instrumentNames)
        {
            InitiateObservations();
        }

        private void InitiateObservations()
        {
            if (instruments.Keys.Count == 0)
            {
                InitializeInstruments();
            }
            _timer = new Timer(smallSpanInSec); // Set up the timer for X seconds
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true; // Enable it    
            lastTimeAnalyzed = DateTime.Now;
        }

        private void InitializeInstruments()
        {
            string names = Properties.Settings.Default.Instruments;
            char[] delimiterChars = { ' ', ','};
            List<string> instrNames = names.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var instrName in instrNames)
            {
                string instrNameUpper = instrName.ToUpper();
                instruments[instrNameUpper] = new FinancialInstrument() { Name = instrNameUpper, ShortName = instrNameUpper };
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            // get all currencies rates
            GetAllRates(dtNow);
            // add to observations for all active currancies
            // check if analysis time -> analyze
            TimeSpan timePassed = dtNow - lastTimeAnalyzed;
            if (timePassed.TotalSeconds < analysisFrequencyInSmallSpans * smallSpanInSec)
            {
                return;
            }
            Analize(dtNow);
        }


        private void GetAllRates(DateTime dtNow)
        {
            ErrorMsg = string.Empty;
            Dictionary<string, double> rates = new Dictionary<string, double>();
            string strBTC = "https://www.eobot.com/api.aspx?supportedcoins=true&currency=USD";

            string content = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strBTC);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 58.0.3029.110 Safari / 537.36";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMsg = $"Failed to request the rates: {e.Message}";
                return;
            }
            List<string> replies = content.Split(new char[] { ';' }).ToList();
            foreach (var str in replies)
            {
                Tuple<string, double> res = GetInstrumentCodeValue(str);
                if (res == null)
                    continue;

                FinancialInstrument fi = null;
                if (!instruments.TryGetValue(res.Item1, out fi))
                    continue;
                fi.AddObservation(dtNow, res.Item2);
            }

        }

        private Tuple<string, double> GetInstrumentCodeValue(string str)
        {
            var lst = str.Split(new char[] { ',' }).ToList();
            if (lst.Count < 3)
                return null;
            string code = lst[0].ToUpper();
            if (!instruments.ContainsKey(code))
                return null;
            double val = GetDouble(lst[2], -1.0);
            if (val <= 0)
                return null;
            var res = new Tuple<string, double>(lst[0].ToUpper(), val);
            return res;
        }

        public void Analize(DateTime dtNow)
        {
            // Get longest period of analysis
            int secToObserve = smallSpanInSec * nSmallSpansInMiddle * nMiddleSpansInLong;
            // get starting time
            DateTime dtStartToConsider = dtNow.AddSeconds(-secToObserve);
            // in a loop for all instruments make an analysis
            foreach (var fi in instruments.Values)
            {
                fi.AnalyzeFromTo(dtStartToConsider, dtNow);
            }
        }


        /// <summary>
        /// Utilities area
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetDouble(string value, double defaultValue)
        {
            double result;

            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }

            return result;
        }
    }
*/
}
