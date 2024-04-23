using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.Extensions;
public class MaxFileCountAttribute : ValidationAttribute
{
    private readonly int _maxCount;
    public MaxFileCountAttribute(int maxCount)
    {
        _maxCount = maxCount;
    }
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var files = value as IFormFileCollection;
        if (files?.Count > _maxCount)
        {
            return new ValidationResult($"Максимально возможное количество файлов: {_maxCount}");
        }
        return ValidationResult.Success;
    }
}
