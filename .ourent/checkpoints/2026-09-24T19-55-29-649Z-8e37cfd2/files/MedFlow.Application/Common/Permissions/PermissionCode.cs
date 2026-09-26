namespace MedFlow.Application.Common.Permissions;

public static class PermissionCode
{
    public const string PatientEdit = "Patient.Edit";
    public const string NurseCreate = "Nurse.Create";
    public const string NurseEdit = "Nurse.Edit";
    public const string PrescriptionVerify = "Prescription.Verify";
    public const string CourseEdit = "Course.Edit";
    public const string SessionArrive = "Session.Arrive";
    public const string SessionComplete = "Session.Complete";
    public const string SessionMarkMissed = "Session.MarkMissed";
    public const string SessionIssueMedicine = "Session.IssueMedicine";
    public const string MedicineManage = "Medicine.Manage";
    public const string WalletPurchase = "Wallet.Purchase";
    public const string ServiceTypeCreate = "ServiceType.Create";
    public const string PaymentCreate = "Payment.Create";

    public static readonly IReadOnlyList<string> All = new[]
    {
        PatientEdit,
        NurseCreate,
        NurseEdit,
        PrescriptionVerify,
        CourseEdit,
        SessionArrive,
        SessionComplete,
        SessionMarkMissed,
        SessionIssueMedicine,
        MedicineManage,
        WalletPurchase,
        ServiceTypeCreate,
        PaymentCreate
    };
}
