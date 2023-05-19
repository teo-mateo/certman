using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace certman.Extensions;

public static class ModelStateDictionaryExtensions
{
    public static dynamic? GetErrorMessages(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return null;
        
        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        return errors.Any()
            ? new
            {   
                message = "Invalid model",
                errors
            }
            : null;
    }
}