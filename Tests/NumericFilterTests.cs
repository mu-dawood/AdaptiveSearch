using System;
using AdaptiveSearch.Filters;

namespace Tests;

public class NumericFilterTests
{
    public class Product
    {
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }

    [Fact]
    public void Filter_Equal_Works_For_Int()
    {
        // Arrange
        var filter = new ComparableFilter<int> { Equal = 50 };
        var data = new List<Product>
        {
            new() { Stock = 30, Price = 100 },
            new() { Stock = 50, Price = 200 },
            new() { Stock = 60, Price = 300 }
        }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Stock, filter).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(50, result[0].Stock);
    }

    [Fact]
    public void Filter_GreaterThan_LessThanOrEqual_Works_For_Int()
    {
        // Arrange
        var filter = new ComparableFilter<int>
        {
            GreaterThan = 10,
            LessThanOrEqual = 50
        };

        var data = new List<Product>
        {
            new() { Stock = 5, Price = 90 },
            new() { Stock = 20, Price = 150 },
            new() { Stock = 50, Price = 199.99m },
            new() { Stock = 60, Price = 250 }
        }.AsQueryable();

        // Act
         var result = data.AdaptiveSearch(x => x.Stock, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Stock == 20);
        Assert.Contains(result, p => p.Stock == 50);
    }

    [Fact]
    public void Filter_NotEqual_Works_For_Decimal()
    {
        // Arrange
        var filter = new ComparableFilter<decimal> { NotEqual = 150.75m };

        var data = new List<Product>
        {
            new() { Stock = 10, Price = 150.75m },
            new() { Stock = 20, Price = 199.99m },
            new() { Stock = 30, Price = 100.50m }
        }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Price, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Price == 150.75m);
    }

    [Fact]
    public void Filter_LessThanOrEqual_Works_For_Decimal()
    {
        // Arrange
        var filter = new ComparableFilter<decimal> { LessThanOrEqual = 200.00m };

        var data = new List<Product>
        {
            new() { Stock = 10, Price = 90.00m },
            new() { Stock = 20, Price = 150.75m },
            new() { Stock = 30, Price = 250.00m }
        }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Price, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Price == 90.00m);
        Assert.Contains(result, p => p.Price == 150.75m);
    }

    [Fact]
    public void Filter_Combination_Works_For_Int()
    {
        // Arrange
        var filter = new ComparableFilter<int>
        {
            GreaterThan = 10,
            LessThanOrEqual = 60,
            NotEqual = 50
        };

        var data = new List<Product>
        {
            new() { Stock = 10, Price = 90.0m },
            new() { Stock = 20, Price = 150.75m },
            new() { Stock = 50, Price = 199.99m },
            new() { Stock = 60, Price = 250.0m }
        }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Stock, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Stock == 20);
        Assert.Contains(result, p => p.Stock == 60);
    }
}
