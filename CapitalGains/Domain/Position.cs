namespace CapitalGains.Domain;

public class Position
{
    public int Quantity { get; private set; }
    public decimal WeightedAveragePrice { get; private set; }
    public decimal AccumulatedLoss { get; private set; }

    public Position()
    {
        Quantity = 0;
        WeightedAveragePrice = 0;
        AccumulatedLoss = 0;
    }

    public void Buy(int quantity, decimal unitCost)
    {
        if (quantity <= 0) return;

        var totalCurrentValue = Quantity * WeightedAveragePrice;
        var totalBoughtValue = quantity * unitCost;
        var newTotalQuantity = Quantity + quantity;

        WeightedAveragePrice = (totalCurrentValue + totalBoughtValue) / newTotalQuantity;
        Quantity = newTotalQuantity;
        
        WeightedAveragePrice = Math.Round(WeightedAveragePrice, 2, MidpointRounding.AwayFromZero);
    }

    public decimal Sell(int quantity, decimal unitCost)
    {
        if (quantity <= 0 || quantity > Quantity) return 0;

        var totalSaleValue = quantity * unitCost;
        var isExempt = totalSaleValue <= 20000m;

        Quantity -= quantity;

        var costBasis = quantity * WeightedAveragePrice;
        var profitOrLoss = totalSaleValue - costBasis;

        if (isExempt)
        {
            if (profitOrLoss < 0)
            {
                AccumulatedLoss += Math.Abs(profitOrLoss);
            }
            return 0;
        }

        if (profitOrLoss <= 0)
        {
            AccumulatedLoss += Math.Abs(profitOrLoss);
            return 0;
        }

        var taxableProfit = profitOrLoss;
        if (AccumulatedLoss > 0)
        {
            var lossToDeduct = Math.Min(AccumulatedLoss, profitOrLoss);
            AccumulatedLoss -= lossToDeduct;
            taxableProfit = profitOrLoss - lossToDeduct;
        }

        var tax = taxableProfit * 0.20m;
        return Math.Round(tax, 2, MidpointRounding.AwayFromZero);
    }
}