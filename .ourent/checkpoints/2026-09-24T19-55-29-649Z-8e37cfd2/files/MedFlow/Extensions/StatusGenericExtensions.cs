using Microsoft.AspNetCore.Mvc.ModelBinding;
using StatusGeneric;

namespace MedFlow.Extensions;

public static class StatusGenericExtensions
{
    public static void CopyErrorsToModelState(this IStatusGeneric status, ModelStateDictionary modelState)
    {
        foreach (var error in status.Errors)
        {
            var memberNames = error.ErrorResult.MemberNames.ToList();
            if (memberNames.Count == 0)
            {
                modelState.AddModelError(string.Empty, error.ErrorResult.ErrorMessage ?? "Xatolik yuz berdi.");
                continue;
            }

            foreach (var member in memberNames)
            {
                modelState.AddModelError(member, error.ErrorResult.ErrorMessage ?? "Xatolik yuz berdi.");
            }
        }
    }
}
