using System;
using AdaptiveSearch;
using AdaptiveSearch.Attributes;
using AdaptiveSearch.Filters;
using AdaptiveSearch.Interfaces;

namespace Tests;

public class AdaptiveClass
{
    public class Entity
    {
        public string Name { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


    }

    public class Filters
    {
        [Skip]
        public int Skip { get; set; }
        [Take]
        public int Take { get; set; } = 20;
        public StringFilter? Name { get; set; }
        public IntegerFilter? Age { get; set; }
        public DateTimeFilter? CreatedAt { get; set; }
        public BooleanFilter? IsActive { get; set; }



    }

    private IQueryable<Entity> GetData()
    {
        return new List<Entity>
            {
                new() { Name = "John Doe",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 ,JobTitle="Software Engineer",Specialty="IT" },
                new() { Name = "Johnny",CreatedAt=new DateTime(2025,1,1),IsActive=true,Age=25,JobTitle="Software Tester",Specialty="IT" },
                new() { Name = "Jane",CreatedAt=new DateTime(2024,1,1),IsActive=false,Age=30,JobTitle="HR Manager",Specialty="HR" },
                new() {Name ="John",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=21,JobTitle="Software Qc",Specialty="IT" },
                new(){ Name="Tom",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20,JobTitle="Software Engineer",Specialty="IT" }
            }.AsQueryable();
    }


    [Fact]
    public void TestSingleFilter()
    {
        // Arrange
        var filter = new StringFilter { StartsWith = "John" };
        var data = GetData();
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
        var data = GetData();

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
        var data = GetData();

        // Act
        var result = data.AdaptiveSearch(filter, true).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "John");
    }

    [Fact]
    public void TestObjectSkip()
    {
        // Arrange
        var filter = new Filters
        {
            Name = new() { StartsWith = "John" },
            Skip = 1,

        };
        var data = GetData();

        // Act
        var result = data.AdaptiveSearch(filter).ApplyPaging().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "John");
        Assert.Contains(result, p => p.Name == "Johnny");
    }


    [Fact]
    public void TestObjectTake()
    {
        // Arrange
        var filter = new Filters
        {
            Name = new() { StartsWith = "John" },
            Take = 1,

        };
        var data = GetData();

        // Act
        var result = data.AdaptiveSearch(filter).ApplyPaging().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "John Doe");
    }

    [Fact]
    public void TestObjectSkipTake()
    {
        // Arrange
        Filters? filter = new()
        {
            Name = new() { StartsWith = "John" },
            Take = 1,
            Skip = 1,

        };
        var data = GetData();

        // Act
        var result = data.AdaptiveSearch(filter).ApplyPaging().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "Johnny");
    }



    [Fact]
    public void TestMultiFilter()
    {
        // Arrange
        var filter = new StringFilter { StartsWith = "S" };
        var data = GetData();
        data = data.Append(new() { Name = "Sally", CreatedAt = new DateTime(2024, 1, 1), IsActive = false, Age = 30, JobTitle = "HR Manager", Specialty = "HR" });

        // Act
        var result = data.AdaptiveSearch(filter, ApplyType.Or)
          .ApplyTo(c => c.Name)
          .ApplyTo(y => y.JobTitle)
          .ApplyTo(t => t.Specialty)
          .ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Contains(result, p => p.Name == "Sally");
    }

    [Fact]
    public void TestMultiFilter2()
    {
        // Arrange
        var filter = new StringFilter { NotStartsWith = "S" };
        var data = GetData();
        data = data.Append(new() { Name = "Sally", CreatedAt = new DateTime(2024, 1, 1), IsActive = false, Age = 30, JobTitle = "HR Manager", Specialty = "HR" });

        // Act
        var result = data.AdaptiveSearch(filter, ApplyType.Or)
          .ApplyTo(c => c.Name)
          .ApplyTo(y => y.JobTitle)
          .ApplyTo(t => t.Specialty)
          .ToList();

        // Assert
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public void TestMultiFilter3()
    {
        // Arrange
        var filter = new StringFilter { NotStartsWith = "S" };
        var data = GetData();
        data = data.Append(new() { Name = "Sally", CreatedAt = new DateTime(2024, 1, 1), IsActive = false, Age = 30, JobTitle = "HR Manager", Specialty = "HR" });

        // Act
        var result = data.AdaptiveSearch(filter, ApplyType.And)
          .ApplyTo(c => c.Name)
          .ApplyTo(y => y.JobTitle)
          .ApplyTo(t => t.Specialty)
          .ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Name == "Jane");

    }

}

