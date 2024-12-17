using System;
using AdaptiveSearch.Filters;
using AdaptiveSearch.Interfaces;

namespace Tests;

public class AdaptiveClass
{
    public class Entity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class Filters 
    {
        public StringFilter? Name { get; set; }
        public IntegerFilter? Age { get; set; }
        public DateTimeFilter? CreatedAt { get; set; }
        public BooleanFilter? IsActive { get; set; }
    }

    [Fact]
    public void TestSingleFilter()
    {
        // Arrange
        var filter = new StringFilter { StartsWith = "John" };
        var data = new List<Entity>
            {
                new() { Name = "John Doe",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 },
                new() { Name = "Johnny",CreatedAt=new DateTime(2025,1,1),IsActive=true,Age=25 },
                new() { Name = "Jane",CreatedAt=new DateTime(2024,1,1),IsActive=false,Age=30 },
                new() {Name ="John",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 },
                new(){ Name="Tom",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(x => x.Name, filter).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.Name == "John Doe");
        Assert.Contains(result, p => p.Name == "Johnny");
        Assert.Contains(result, p => p.Name == "John");
    }

    [Fact]
    public void TestObjectFilter()
    {
        // Arrange
        var filter = new Filters
        {
            Name = new() { StartsWith = "John" },
            Age = new() { Equal = 20 },
            IsActive = new() { Equal = true },
            CreatedAt = new()
            {
                Equal = new DateTime(2023, 1, 1)
            }
        };
        var data = new List<Entity>
            {
                new() { Name = "John Doe",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 },
                new() { Name = "Johnny",CreatedAt=new DateTime(2025,1,1),IsActive=true,Age=25 },
                new() { Name = "Jane",CreatedAt=new DateTime(2024,1,1),IsActive=false,Age=30 },
                new() {Name ="John",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=21 },
                new(){ Name="Tom",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(filter).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "John Doe");
        Assert.DoesNotContain(result, p => p.Name == "Johnny");
        Assert.DoesNotContain(result, p => p.Name == "Jane");
        Assert.DoesNotContain(result, p => p.Name == "John");
        Assert.DoesNotContain(result, p => p.Name == "Tom");
    }

    [Fact]
    public void TestApplyAllProperties()
    {
        // Arrange
        var filter = new
        {
            Name = "John",
        };
        var data = new List<Entity>
            {
                new() { Name = "John Doe",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 },
                new() { Name = "Johnny",CreatedAt=new DateTime(2025,1,1),IsActive=true,Age=25 },
                new() { Name = "Jane",CreatedAt=new DateTime(2024,1,1),IsActive=false,Age=30 },
                new() {Name ="John",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=21 },
                new(){ Name="Tom",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 }
            }.AsQueryable();

        // Act
        var result = data.AdaptiveSearch(filter,true).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "John");
    }
}

