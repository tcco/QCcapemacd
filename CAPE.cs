using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect {

    /*
    Summary
    CAPE Ratio for S&P500: PE Ratio for avg inflation adjusted earnings for previous ten years
    Custom Data from DropBox
    Original Data from: http://www.econ.yale.edu/~shiller/data.htm
    */
    public class CAPE : BaseData
    {
        public decimal Cape = 0;
        string format = "yyyy-MM";
        CultureInfo provider = CultureInfo.InvariantCulture;

        public CAPE()
        {
            this.Symbol = "CAPE";
        }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.dropbox.com/s/ggt6blmib54q36e/CAPE.csv?dl=1";
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            CAPE index = new CAPE();

            try
            {
                //Example File Format:
                //Date   |  Price |  Div  | Earning | CPI  | FractionalDate | Interest Rate | RealPrice | RealDiv | RealEarnings | CAPE
                //2014.06  1947.09  37.38   103.12   238.343    2014.37          2.6           1923.95     36.94        101.89     25.55
                string[] data = line.Split(',');
                //Dates must be in the format YYYY-MM-DD. If your data source does not have this format, you must use
                //DateTime.ParseExact() and explicit declare the format your data source has.
                string dateString = data[0];
                index.Time = DateTime.ParseExact(dateString, format, provider);
                index.Cape = Convert.ToDecimal(data[10]);
                index.Symbol = "CAPE";
                index.Value = index.Cape;
            }
            catch
            {

            }

            return index;
        }
    }
}