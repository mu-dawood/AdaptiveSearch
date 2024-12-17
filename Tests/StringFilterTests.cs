using System;
using AdaptiveSearch;
using AdaptiveSearch.Filters;

namespace Tests;

public class StringFilterTests
{
    public class Person
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void Filter_StartsWith_Works()
    {
        // Arrange
        var filter = new StringFilter { StartsWith = "John" };
        var data = new List<Person>
            {
                new() { Name = "John Doe" },
                new() { Name = "Johnny" },
                new() { Name = "Jane" }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Name, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "John Doe");
        Assert.Contains(result, p => p.Name == "Johnny");
    }

    [Fact]
    public void Filter_IsNullOrEmpty_Works()
    {
        // Arrange
        var filter = new StringFilter { IsNullOrEmpty = true };
        var data = new List<Person>
            {
                new() { Name = "John" },
                new() { Name = "" },
                new() { Name = null }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Name, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => string.IsNullOrEmpty(p.Name));
    }

    [Fact]
    public void Filter_NotEqual_Works()
    {
        // Arrange
        var filter = new StringFilter { NotEqual = "Jane" };
        var data = new List<Person>
            {
                new() { Name = "John" },
                new() { Name = "Jane" },
                new() { Name = "Johnny" }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Name, filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Name == "Jane");
    }
}

