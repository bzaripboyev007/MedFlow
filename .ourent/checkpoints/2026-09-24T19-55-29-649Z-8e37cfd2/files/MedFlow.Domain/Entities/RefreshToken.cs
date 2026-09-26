using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("REFRESH_TOKENS", Schema = "IDM")]
public partial class RefreshToken : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("USER_ID")]
    [Precision(19)]
    public long UserId { get; set; }

    [Column("TOKEN_HASH")]
    [StringLength(200)]
    public string TokenHash { get; set; }

    [Column("EXPIRES_AT")]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("REVOKED_AT")]
    public DateTimeOffset? RevokedAt { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
}
