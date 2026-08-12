using System.Security.Claims;
using MedFlow.Domain.Enums;

namespace MedFlow.Application.Common.Permissions;

/// <summary>
/// Rol -> ruxsatlar statik xaritasi (hozircha DB'da alohida jadval/UI yo'q - bu avvalgi
/// RoleGroups.Admin/RoleGroups.Staff qoidalarining permission-kod shaklidagi aynan tarjimasi).
/// </summary>
public static class RolePermissions
{
    private static readonly string[] StaffCodes =
    {
        PermissionCode.PatientEdit,
        PermissionCode.PrescriptionVerify,
        PermissionCode.CourseEdit,
        PermissionCode.SessionArrive,
        PermissionCode.SessionComplete,
        PermissionCode.SessionMarkMissed,
        PermissionCode.SessionIssueMedicine,
        PermissionCode.MedicineManage,
        PermissionCode.WalletPurchase,
        PermissionCode.PaymentCreate
    };

    private static readonly string[] AdminOnlyCodes =
    {
        PermissionCode.NurseCreate,
        PermissionCode.NurseEdit,
        PermissionCode.ServiceTypeCreate
    };

    private static readonly IReadOnlyDictionary<string, HashSet<string>> RoleMap = new Dictionary<string, HashSet<string>>
    {
        [nameof(UserRole.Admin)] = new HashSet<string>(StaffCodes.Concat(AdminOnlyCodes)),
        [nameof(UserRole.PediatricNurse)] = new HashSet<string>(StaffCodes),
        [nameof(UserRole.AdultNurse)] = new HashSet<string>(StaffCodes),
        [nameof(UserRole.Patient)] = new HashSet<string>()
    };

    public static bool Has(ClaimsPrincipal user, string code)
    {
        foreach (var (role, codes) in RoleMap)
        {
            if (user.IsInRole(role) && codes.Contains(code))
            {
                return true;
            }
        }

        return false;
    }
}
