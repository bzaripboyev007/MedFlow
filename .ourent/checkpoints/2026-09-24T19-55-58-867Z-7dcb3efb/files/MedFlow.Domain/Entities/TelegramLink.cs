using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

/// <summary>
/// Foydalanuvchi telefon raqamini Telegram bot chatiga bog'laydi (bot orqali OTP yuborish uchun).
/// Foydalanuvchi botga /start bosib, kontaktini ulashganda yaratiladi.
/// </summary>
[Table("TELEGRAM_LINKS", Schema = "IDM")]
public partial class TelegramLink : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PHONE_NUMBER")]
    [StringLength(30)]
    public string PhoneNumber { get; set; } = null!;

    [Column("CHAT_ID")]
    [Precision(19)]
    public long ChatId { get; set; }

    [Column("TELEGRAM_USERNAME")]
    [StringLength(100)]
    public string? TelegramUsername { get; set; }

    [Column("LINKED_AT")]
    public DateTimeOffset LinkedAt { get; set; }
}
