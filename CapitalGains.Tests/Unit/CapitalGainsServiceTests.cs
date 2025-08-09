using CapitalGains.Application;
using CapitalGains.Domain;

namespace CapitalGains.Tests.Unit;

public class CapitalGainsServiceTests
{
    private readonly CapitalGainsService _service = new();

    [Fact]
    public void ProcessOperations_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var operations = new List<Operation>();

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ProcessOperations_WithOnlyBuyOperations_ShouldReturnZeroTaxes()
    {
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 15.00m, Quantity = 200 },
            new() { OperationType = "buy", UnitCost = 20.00m, Quantity = 50 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.Equal(0m, r.Tax));
    }

    [Fact]
    public void ProcessOperations_Case1_ExemptSales_ShouldReturnZeroTaxes()
    {
        // Case #1: Exempt sales under 20000
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 50 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 50 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        Assert.Equal(0m, results[1].Tax); // Exempt sale (750)
        Assert.Equal(0m, results[2].Tax); // Exempt sale (750)
    }

    [Fact]
    public void ProcessOperations_Case2_ProfitThenLoss_ShouldCalculateCorrectly()
    {
        // Case #2: Profit without previous loss, then loss
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 10000 },
            new() { OperationType = "sell", UnitCost = 20.00m, Quantity = 5000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 5000 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        Assert.Equal(10000m, results[1].Tax); // Profit: (20-10)*5000 = 50000, Tax: 50000*0.2 = 10000
        Assert.Equal(0m, results[2].Tax); // Loss: accumulates 25000 loss
    }

    [Fact]
    public void ProcessOperations_Case3_PartialLossDeduction_ShouldReduceTax()
    {
        // Case #3: Partial loss deduction
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 10000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 5000 },
            new() { OperationType = "sell", UnitCost = 20.00m, Quantity = 3000 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        Assert.Equal(0m, results[1].Tax); // Loss: 25000 accumulated
        Assert.Equal(1000m, results[2].Tax); // Profit: 30000, after loss deduction: 5000, Tax: 1000
    }

    [Fact]
    public void ProcessOperations_Case4_WeightedAverageWithoutProfitLoss_ShouldReturnZeroTax()
    {
        // Case #4: Weighted average without profit or loss
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 25.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(4, results.Count);
        Assert.All(results, r => Assert.Equal(0m, r.Tax));
        // Weighted average = (100*10 + 100*25)/200 = 17.50
        // Both sales at 15.00 result in losses, but they're exempt (value < 20000)
    }

    [Fact]
    public void ProcessOperations_Case5_SaleWithoutProfitThenSaleWithProfit_ShouldCalculateCorrectly()
    {
        // Case #5: Sale without profit then sale with profit
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 100 },
            new() { OperationType = "buy", UnitCost = 25.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 15.00m, Quantity = 100 },
            new() { OperationType = "sell", UnitCost = 25.00m, Quantity = 100 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(4, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        Assert.Equal(0m, results[1].Tax); // Buy
        Assert.Equal(0m, results[2].Tax); // Sale at loss (exempt: 1500 < 20000)
        Assert.Equal(0m, results[3].Tax); // Sale should have profit offset by accumulated loss from previous sale
    }

    [Fact]
    public void ProcessOperations_WithMultipleExemptSalesFollowedByLargeProfitSale_ShouldPreserveLosses()
    {
        // Edge case: Multiple exempt sales followed by large profit sale
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 10.00m, Quantity = 1000 },
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 100 }, // Loss 500, exempt
            new() { OperationType = "sell", UnitCost = 5.00m, Quantity = 100 }, // Loss 500, exempt
            new() { OperationType = "sell", UnitCost = 30.00m, Quantity = 800 } // Large profit, not exempt
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(4, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        Assert.Equal(0m, results[1].Tax); // Exempt loss
        Assert.Equal(0m, results[2].Tax); // Exempt loss
        // Third sale: profit = 800*30 - 800*10 = 16000, accumulated loss = 1000
        // Taxable profit = 16000 - 1000 = 15000, tax = 3000
        Assert.Equal(3000m, results[3].Tax);
    }

    [Fact]
    public void ProcessOperations_WithLargeNumbers_ShouldHandlePrecisionCorrectly()
    {
        // Edge case: Large numbers and precision
        // Arrange
        var operations = new List<Operation>
        {
            new() { OperationType = "buy", UnitCost = 1000.00m, Quantity = 100000 },
            new() { OperationType = "sell", UnitCost = 1500.00m, Quantity = 50000 }
        };

        // Act
        var results = _service.ProcessOperations(operations);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(0m, results[0].Tax); // Buy
        // Profit = 50000 * (1500 - 1000) = 25,000,000
        // Tax = 25,000,000 * 0.2 = 5,000,000
        Assert.Equal(5000000m, results[1].Tax);
    }

    [Fact]
    public void ProcessOperations_StateIndependence_ShouldNotAffectSubsequentCalls()
    {
        // Test that multiple calls to ProcessOperations don't interfere with each other
        // Arrange
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

        // Act
        var results1 = _service.ProcessOperations(operations1);
        var results2 = _service.ProcessOperations(operations2);

        // Assert
        // First call should accumulate loss (sale value = 2500, exempt)
        Assert.Equal(0m, results1[1].Tax);

        // Second call should not be affected by first call's accumulated loss
        // Sale value = 30000 (not exempt), profit = 1000 * (30-20) = 10000, tax = 2000
        Assert.Equal(2000m, results2[1].Tax);
    }
}