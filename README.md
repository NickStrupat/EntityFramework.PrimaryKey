# EntityFramework.PrimaryKey
Retrieve the primary key (including composite keys) from any entity

NuGet package listed on nuget.org at https://www.nuget.org/packages/EntityFramework.PrimaryKey/

[![NuGet Status](http://img.shields.io/nuget/v/EntityFramework.PrimaryKey.svg?style=flat)](https://www.nuget.org/packages/EntityFramework.PrimaryKey/)

## Usage

```csharp
public class QuestionTag {
    [Key, Column(Order = 0)] public Guid QuestionId { get; set; }
    [Key, Column(Order = 1)] public Int64 TagId { get; set; }
    public DateTime Inserted { get; set; }
}

var primaryKey = questionTag.GetPrimaryKey();
var questionId = (Guid) primaryKey[nameof(QuestionTag.QuestionId)];
var tagId = (Int64) primaryKey[nameof(QuestionTag.TagId)];
```