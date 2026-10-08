namespace MedFlow.Application.Common;

public static class AuthConstants
{
    public const int MaxFailedLoginAttempts = 5;
    public const int MinPasswordLength = 6;
    public const int LockoutMinutes = 15;

    public const int OtpCodeLength = 6;
    public const int OtpExpiryMinutes = 5;
    public const int OtpMaxVerifyAttempts = 5;
    public const int OtpResendCooldownSeconds = 60;
}
