using System.ComponentModel.DataAnnotations;

namespace pinq.api.Filters;

public class AtLeastOneRequiredAttribute(params string[] propertyNames) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = validationContext.ObjectType.GetProperty(propertyName);
            if (property is null)
                return new ValidationResult($"Unknown property: {propertyName}");

            var propertyValue = property.GetValue(validationContext.ObjectInstance);
            if (propertyValue is string strValue && !string.IsNullOrEmpty(strValue))
                return ValidationResult.Success;
        }

        return new ValidationResult($"At least one of the properties ({string.Join(", ", propertyNames)}) must be provided.");
    }
}
