using System;
using QuantConnect.Indicators;
using QuantConnect.Models;
using QuantConnect.Securities;

namespace QuantConnect
{
    /* <summary>
     Using MACD to signal when to buy and sell momentum stocks
    </summary>
    */
    public partial class Bubble : QCAlgorithm
    {
        public void buy(String symbol)
        {
            SecurityHolding s = Securities[symbol].Holdings;
            if (macdDic[symbol] > 0m)
            {
                SetHoldings(symbol, 1);

                Debug("Purchasing: "+ symbol+ "   MACD: "+ macdDic[symbol] +"   RSI: "+rsiDic[symbol]
                    + "   Price: " + Math.Round(Securities[symbol].Price, 2)+ "   Quantity: "+ s.Quantity);
            }
        }

        public void sell(String symbol)
        {
            SecurityHolding s = Securities[symbol].Holdings;
            if (s.Quantity > 0 && macdDic[symbol] < 0m)
            {
                Liquidate(symbol);

                Debug("Selling: "+symbol+ " at sell MACD: "+ macdDic[symbol]+"   RSI: "+rsiDic[symbol]
                    +"   Price: " +Math.Round(Securities[symbol].Price, 2)+"   Profit from sale: "+ s.LastTradeProfit);
            }
        }

    }
}