using CapitalGains.Application;
using CapitalGains.Domain;

namespace CapitalGains.Tests.Unit;

public class CapitalGainsServiceTests
{
    private readonly CapitalGainsService _service = new();

    [Fact]
    public void ProcessOperations_WithEmptyList_ShouldReturnEmptyList()
    {
        var operations = new List<Operation>();

        var results = _service.ProcessOperations(operations);

        Assert.Empty(results);
    }

    [Fact]
    public void ProcessOperations_WithOnlyBuyOperations_ShouldReturnZeroTaxes()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 15.00m, Quantity = 200 },
            new() { OperationType = "buy", UnitCost = 20.00m, Quantity = 50 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.Equal(0m, r.Tax));
    }

    [Fact]
    public void ProcessOperations_Case1_ExemptSales_ShouldReturnZeroTaxes()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 50 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 50 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(0m, results[1].Tax);
        Assert.Equal(0m, results[2].Tax);
    }

    [Fact]
    public void ProcessOperations_Case2_ProfitThenLoss_ShouldCalculateCorrectly()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 10000 },
            new() { OperationType = "sell", UnitCost = 20.00m, Quantity = 5000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 5000 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(10000m, results[1].Tax);
        Assert.Equal(0m, results[2].Tax);
    }

    [Fact]
    public void ProcessOperations_Case3_PartialLossDeduction_ShouldReduceTax()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 10000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 5000 },
            new() { OperationType = "sell", UnitCost = 20.00m, Quantity = 3000 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(0m, results[1].Tax);
        Assert.Equal(1000m, results[2].Tax);
    }

    [Fact]
    public void ProcessOperations_Case4_WeightedAverageWithoutProfitLoss_ShouldReturnZeroTax()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 25.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(4, results.Count);
        Assert.All(results, r => Assert.Equal(0m, r.Tax));
    }

    [Fact]
    public void ProcessOperations_Case5_SaleWithoutProfitThenSaleWithProfit_ShouldCalculateCorrectly()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 25.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 25.00m, Quantity = 100 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(4, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(0m, results[1].Tax);
        Assert.Equal(0m, results[2].Tax);
        Assert.Equal(0m, results[3].Tax);
    }

    [Fact]
    public void ProcessOperations_WithMultipleExemptSalesFollowedByLargeProfitSale_ShouldPreserveLosses()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 1000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 30.00m, Quantity = 800 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(4, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(0m, results[1].Tax);
        Assert.Equal(0m, results[2].Tax);
        Assert.Equal(3000m, results[3].Tax);
    }

    [Fact]
    public void ProcessOperations_WithLargeNumbers_ShouldHandlePrecisionCorrectly()
    {
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 1000.00m, Quantity = 100000 },
            new() { OperationType = "sell", UnitCost = 1500.00m, Quantity = 50000 }
        };

        var results = _service.ProcessOperations(operations);

        Assert.Equal(2, results.Count);
        Assert.Equal(0m, results[0].Tax);
        Assert.Equal(5000000m, results[1].Tax);
    }

    [Fact]
    public void ProcessOperations_StateIndependence_ShouldNotAffectSubsequentCalls()
    {
        var operations1 = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 1000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 500 }
        };

        var operations2 = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 20.00m, Quantity = 2000 },
            new() { OperationType = "sell", UnitCost = 30.00m, Quantity = 1000 }
        };

        var results1 = _service.ProcessOperations(operations1);
        var results2 = _service.ProcessOperations(operations2);

        Assert.Equal(0m, results1[1].Tax);

        Assert.Equal(2000m, results2[1].Tax);
    }
}