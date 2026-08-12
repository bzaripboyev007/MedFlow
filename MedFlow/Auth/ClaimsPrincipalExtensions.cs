using System.Security.Claims;

namespace MedFlow.Auth;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal user)
        => long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static long? GetLinkedPatientId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("linkedPatientId");
        return value is null ? null : long.Parse(value);
    }

    public static int? GetLinkedNurseId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("linkedNurseId");
        return value is null ? null : int.Parse(value);
    }

    public static bool IsPatient(this ClaimsPrincipal user) => user.IsInRole("Patient");

    public static bool IsNurse(this ClaimsPrincipal user) => user.IsInRole("PediatricNurse") || user.IsInRole("AdultNurse");

    public static bool IsStaff(this ClaimsPrincipal user) => user.IsInRole("Admin") || user.IsNurse();
}
