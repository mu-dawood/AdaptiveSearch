# AdaptiveSearch

**AdaptiveSearch** is a lightweight and powerful library for dynamically building LINQ filtering expressions in C#. It supports dynamic filtering for **strings**, **numbers**, and **DateTime** properties, making it ideal for advanced search features, APIs, and reporting systems.

---

## Features

- **String Filtering**: Supports `Equal`, `NotEqual`, `StartsWith`, `EndsWith`, `Contains`, and their negative counterparts.
- **Numeric Filtering**: Compare values using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, and `LessThanOrEqual`.
- **DateTime Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **TimeSpan Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **IntegerFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **DoubleFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **FloatFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **DecimalFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **LongFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **ShortFilter Filtering**: Dynamically compare dates using `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, and similar conditions.
- **Type-Safe**: Avoids magic strings with **lambda expressions** for property selection.
- **Dynamic and Extensible**: Easily integrate and combine filters for complex queries.
- **LINQ Compatible**: Seamlessly integrates with LINQ-to-Objects, Entity Framework, and other query providers.

---

## Installation

Install **AdaptiveSearch** via NuGet:

```bash
dotnet add package AdaptiveSearch
```

---

## Usage

### 1. Single Filter

```c#
var stringFilter = new StringFilter
{
    StartsWith = "John",
    NotContains = "Doe"
};

var data = new List<Person>
{
    new Person { Name = "John Smith" },
    new Person { Name = "Johnny Doe" },
    new Person { Name = "Jane Doe" }
}.AsQueryable();

var result = stringFilter.Filter(data, x => x.Name);

foreach (var person in result)
{
    Console.WriteLine(person.Name);
}
```
Output:
```
John Smith
```
---

### 2. Nulti Filter

```c#

public class Filters : IAdaptiveSearchObject
{
    public StringFilter? Name { get; set; }
    public IntegerFilter? Age { get; set; }
    public DateTimeFilter? CreatedAt { get; set; }
    public BooleanFilter? IsActive { get; set; }
    }

/// generate filters instance
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

var data = new List<Person>
{
     new() { Name = "John Doe",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 },
     new() { Name = "Johnny",CreatedAt=new DateTime(2025,1,1),IsActive=true,Age=25 },
     new() { Name = "Jane",CreatedAt=new DateTime(2024,1,1),IsActive=false,Age=30 },
     new() {Name ="John",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=21 },
     new(){ Name="Tom",CreatedAt=new DateTime(2023,1,1),IsActive=true,Age=20 }
}.AsQueryable();

var result = stringFilter.Filter(data, x => x.Name);

foreach (var person in result)
{
    Console.WriteLine(person.Name);
}
```
Output:
```
John Smith
```



## API Documentation

### **StringFilter**

| Property       | Description                                      |
|----------------|--------------------------------------------------|
| `Equal`        | Checks if the value equals a given string.       |
| `NotEqual`     | Checks if the value does not equal a given string. |
| `StartsWith`   | Checks if the string starts with the specified value. |
| `EndsWith`     | Checks if the string ends with the specified value. |
| `Contains`     | Checks if the string contains the specified value. |
| `NotContains`  | Ensures the string does not contain the value.   |

---

### **ComparableFilter<T>**

| Property             | Description                                      |
|-----------------------|--------------------------------------------------|
| `Equal`              | Checks if the value equals a specified number.   |
| `NotEqual`           | Checks if the value does not equal a number.     |
| `GreaterThan`        | Checks if the value is greater than the specified number. |
| `LessThan`           | Checks if the value is less than the specified number.   |
| `GreaterThanOrEqual` | Ensures the value is greater than or equal to a number.  |
| `LessThanOrEqual`    | Ensures the value is less than or equal to a number.    |

> note you can use this with any type delivered from IComparable
---

### **BooleanFilter**

| Property             | Description                                       |
|-----------------------|---------------------------------------------------|
| `Equal`              | Filters for dates equal to the given value.       |
| `NotEqual`           | Filters out dates equal to the given value.       |

---

## Benefits

- **Flexible**: Supports dynamic queries for strings, numbers, and dates.
- **Type-Safe**: Leverages lambda expressions for strong typing.
- **Extensible**: Easily combine filters for complex search scenarios.
- **Framework-Compatible**: Works with LINQ and Entity Framework.

---

## Contributing

Contributions are welcome! If you find an issue or want to add a feature, please open an issue or submit a pull request.

---

## License

This project is licensed under the MIT License.
