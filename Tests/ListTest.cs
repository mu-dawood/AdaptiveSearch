using System;
using AdaptiveSearch.Filters;

namespace Tests;

public class ListTest
{
    public class Entity
    {
        public string? Name { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    [Fact]
    public void Test()
    {
        // Arrange
        var listFilter = new ListFilter<string>
        {
            ContainsAny = ["Java"],
            DoesNotContainAny = ["Outdated"],
            ContainsAll = ["Java", "Spring Boot"]
        };
        var data = new List<Entity>
        {
            new() { Name = "Project A", Tags = ["C#", "ASP.NET", "EF Core"] },
            new() { Name = "Project B", Tags = ["Java", "Spring Boot"] },
            new() { Name = "Project C", Tags = ["C#", "LINQ", "Outdated"] },
            new() { Name = "Project D", Tags = ["Python", "ML"] }
        }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Tags, listFilter).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Project B", result[0].Name);
    }


}
