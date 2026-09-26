namespace MedFlow.Domain.Enums;

public enum NurseType
{
    Pediatric = 1,
    Adult = 2
}

public enum Gender
{
    Male = 1,
    Female = 2
}

public enum LocationType
{
    Clinic = 1,
    Home = 2
}

public enum CourseStatus
{
    Pending = 1,
    Active = 2,
    Paused = 3,
    Done = 4,
    Cancelled = 5
}

public enum SessionStatus
{
    Pending = 1,
    Arrived = 2,
    InProgress = 3,
    Done = 4,
    Missed = 5,
    Rescheduled = 6
}

public enum WalletSource
{
    Pharmacy = 1,
    External = 2
}

public enum IssueSource
{
    PatientWallet = 1,
    Pharmacy = 2
}

public enum AssignmentMode
{
    Auto = 1,
    Manual = 2
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    Click = 3,
    Payme = 4
}

public enum UserRole
{
    Patient = 1,
    PediatricNurse = 2,
    AdultNurse = 3,
    Admin = 4
}
