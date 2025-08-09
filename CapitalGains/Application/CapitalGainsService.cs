using CapitalGains.Domain;

namespace CapitalGains.Application;

public class CapitalGainsService
{
    public List<TaxResult> ProcessOperations(List<Operation> operations)
    {
        var position = new Position();
        var results = new List<TaxResult>();

        foreach (var operation in operations)
        {
            decimal tax = 0;

            if (operation.IsBuy)
            {
                position.Buy(operation.Quantity, operation.UnitCost);
            }
            else if (operation.IsSell)
            {
                tax = position.Sell(operation.Quantity, operation.UnitCost);
            }

            results.Add(new TaxResult(tax));
        }

        return results;
    }
}