using MedFlow.Extensions;
using Microsoft.AspNetCore.Mvc;
using StatusGeneric;

namespace MedFlow.Controllers;

/// <summary>StatusGeneric natijasini tekshirib, xato bo'lsa ValidationProblem, aks holda Ok(result) qaytaradi.</summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<T> FromStatus<T>(T result, IStatusGeneric status)
    {
        if (!status.IsValid)
        {
            status.CopyErrorsToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        return Ok(result);
    }
}
