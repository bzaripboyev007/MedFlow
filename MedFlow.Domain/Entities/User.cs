using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("USERS", Schema = "IDM")]
public partial class User : IHaveIdProp<long>
{
    public User()
    {
        RefreshTokens = new HashSet<RefreshToken>();
    }

    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("ROLE")]
    public UserRole Role { get; set; }

    [Column("FULL_NAME")]
    [StringLength(200)]
    public string FullName { get; set; }

    [Column("PHONE_NUMBER")]
    [StringLength(30)]
    public string PhoneNumber { get; set; }

    [Column("PASSWORD_HASH")]
    [StringLength(200)]
    public string PasswordHash { get; set; }

    [Column("LINKED_PATIENT_ID")]
    [Precision(19)]
    public long? LinkedPatientId { get; set; }

    [Column("LINKED_NURSE_ID")]
    [Precision(10)]
    public int? LinkedNurseId { get; set; }

    [Column("FAILED_LOGIN_COUNT")]
    public int FailedLoginCount { get; set; }

    [Column("LOCKED_UNTIL")]
    public DateTimeOffset? LockedUntil { get; set; }

    [Column("LAST_LOGIN_AT")]
    public DateTimeOffset? LastLoginAt { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(LinkedPatientId))]
    public virtual Patient LinkedPatient { get; set; }

    [ForeignKey(nameof(LinkedNurseId))]
    public virtual Nurse LinkedNurse { get; set; }

    [InverseProperty(nameof(RefreshToken.User))]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}
