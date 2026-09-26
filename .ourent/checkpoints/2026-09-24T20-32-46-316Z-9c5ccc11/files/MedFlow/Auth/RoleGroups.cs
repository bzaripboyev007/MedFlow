namespace MedFlow.Auth;

/// <summary>[Authorize(Roles=...)] uchun qayta ishlatiladigan rol guruhlari (vergul bilan ajratilgan, ASP.NET Core talabi).</summary>
public static class RoleGroups
{
    public const string Admin = "Admin";
    public const string Nurse = "PediatricNurse,AdultNurse";
    public const string Staff = "Admin,PediatricNurse,AdultNurse";
    public const string Patient = "Patient";
    public const string AnyAuthenticated = "Patient,PediatricNurse,AdultNurse,Admin";
}
