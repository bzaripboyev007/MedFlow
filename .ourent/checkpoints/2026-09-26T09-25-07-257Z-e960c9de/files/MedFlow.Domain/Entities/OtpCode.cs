using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

/// <summary>
/// Telefon raqamini tasdiqlash uchun Telegram bot orqali yuborilgan bir martalik kod.
/// </summary>
[Table("OTP_CODES", Schema = "IDM")]
public partial class OtpCode : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PHONE_NUMBER")]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = null!;

    [Column("CODE")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("EXPIRES_AT")]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("IS_USED")]
    public bool IsUsed { get; set; }

    [Column("VERIFIED_AT")]
    public DateTimeOffset? VerifiedAt { get; set; }

    [Column("ATTEMPT_COUNT")]
    public int AttemptCount { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }
}
