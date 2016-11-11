using System;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;
using QuantConnect.Indicators;

namespace QuantConnect
{
    /*
    Summary:
    Based on a macroeconomic indicator(CAPE Ratio), we are looking for entry/exit points for momentum stocks
        CAPE data: January 1990 - December 2014
    Goals:
    Capitalize in overvalued markets by generating returns with momentum and selling before the crash
    Capitalize in undervalued markets by purchasing stocks at bottom of trough
    */
    public partial class Bubble : QCAlgorithm
    {
        decimal currCape;
        decimal[] c = new decimal[4];
        decimal[] cCopy = new decimal[4];
        bool newLow = false;
        int counter = 0;
        int counter2 = 0;
        MovingAverageConvergenceDivergence macd;
        RelativeStrengthIndex rsi = new RelativeStrengthIndex(14);
        ArrayList symbols = new ArrayList();

        Dictionary <string, RelativeStrengthIndex> rsiDic = new Dictionary<string, RelativeStrengthIndex>();
        Dictionary <string, MovingAverageConvergenceDivergence> macdDic = new Dictionary<string, MovingAverageConvergenceDivergence>();


        public override void Initialize()
        {
            SetCash(100000);
            symbols.Add("SPY");
            SetStartDate(1998,1,1);
            SetEndDate(2014,12,1);

            //Present Social Media Stocks:
            // symbols.Add("FB");symbols.Add("LNKD");symbols.Add("GRPN");symbols.Add("TWTR");
            // SetStartDate(2011, 1, 1);
            // SetEndDate(2014, 12, 1);

            //2008 Financials:
            // symbols.Add("C");symbols.Add("AIG");symbols.Add("BAC");symbols.Add("HBOS");
            // SetStartDate(2003, 1, 1);
            // SetEndDate(2011, 1, 1);

            //2000 Dot.com:
            // symbols.Add("IPET");symbols.Add("WBVN");symbols.Add("GCTY");
            // SetStartDate(1998, 1, 1);
            // SetEndDate(2000, 1, 1);

            //CAPE data
            AddData<CAPE>("CAPE");

            foreach (string stock in symbols)
            {
                AddSecurity(SecurityType.Equity, stock, Resolution.Minute);

                macd = MACD(stock, 12, 26, 9, MovingAverageType.Exponential, Resolution.Daily);
                macdDic.Add(stock, macd);
                rsi = RSI(stock, 14, MovingAverageType.Exponential, Resolution.Daily);
                rsiDic.Add(stock, rsi);
            }
        }

        //Trying to find if current Cape is the lowest Cape in three months to indicate selling period
        public void OnData(CAPE data)
        {
            newLow = false;
            //Adds first four Cape Ratios to array c
            currCape = data.Cape;
            if( counter < 4)
            {
                c[counter++] = currCape;
            }
            //Replaces oldest Cape with current Cape
            //Checks to see if current Cape is lowest in the previous quarter
            //Indicating a sell off
            else
            {
                Array.Copy(c, cCopy, 4);
                Array.Sort(cCopy);
                if(cCopy[0] > currCape) newLow = true;
                c[counter2++] = currCape;
                if(counter2 == 4) counter2 = 0;
            }

            Debug("Current Cape: "+ currCape + " on "+ data.Time);
            if(newLow) Debug("New Low has been hit on " + data.Time);
        }
        public void OnData(TradeBars data)
        {
            try
            {
                //Bubble territory
                if(currCape > 20 && newLow == false)
                {
                    foreach(string stock in symbols)
                    {
                        //Order stock based on MACD
                        //During market hours, stock is trading, and sufficient cash
                        if (Securities[stock].Holdings.Quantity == 0 && rsiDic[stock] < 70
                            &&Securities[stock].Price != 0 && Portfolio.Cash >Securities[stock].Price*100
                            && Time.Hour== 9 && Time.Minute==30)
                        {
                            buy(stock);
                        }
                        //Utilize RSI for overbought territories and liquidate that stock
                        if(rsiDic[stock] > 70 && Securities[stock].Holdings.Quantity > 0
                                && Time.Hour== 9 && Time.Minute==30)
                        {
                            sell(stock);
                        }
                    }
                }

                // Undervalued territory
                else if(newLow == true)
                {
                    foreach(string stock in symbols)
                    {

                        //Sell stock based on MACD
                        if(Securities[stock].Holdings.Quantity >0 && rsiDic[stock] > 30
                            && Time.Hour== 9 && Time.Minute==30)
                        {
                            sell(stock);
                        }
                        //Utilize RSI and MACD to understand oversold territories
                        else if(Securities[stock].Holdings.Quantity == 0 && rsiDic[stock] < 30
                            &&Securities[stock].Price != 0 && Portfolio.Cash >Securities[stock].Price*100
                            && Time.Hour== 9 && Time.Minute==30)
                        {
                            buy(stock);
                        }
                    }

                }
                // Cape Ratio is missing from orignial data
                // Most recent cape data is most likely to be missing
                else if(currCape == 0)
                {
                    Debug("Exiting due to no CAPE!");
                    Quit("CAPE ratio not supplied in data, exiting.");
                }
            }
            catch(Exception err)
            {
                Error(err.Message);
            }
        }
    }
}