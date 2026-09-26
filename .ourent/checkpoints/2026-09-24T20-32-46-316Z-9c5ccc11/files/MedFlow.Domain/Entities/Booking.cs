using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

/// <summary>
/// Bemorning "Hamshira xizmatini bron qilish" formasi orqali yuborgan so'rovi.
/// Ro'yxatdan o'tmagan mehmon ham yubora oladi (onlayn qabul).
/// </summary>
[Table("BOOKINGS", Schema = "CLINIC")]
public partial class Booking : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("FULL_NAME")]
    [StringLength(200)]
    public string FullName { get; set; } = null!;

    [Column("PHONE")]
    [StringLength(30)]
    public string Phone { get; set; } = null!;

    /// <summary>"adult" yoki "child".</summary>
    [Column("PATIENT_TYPE")]
    [StringLength(20)]
    public string PatientType { get; set; } = null!;

    /// <summary>"home" (uyga chaqirish) yoki "clinic" (klinikaga kelish).</summary>
    [Column("SERVICE_TYPE")]
    [StringLength(20)]
    public string ServiceType { get; set; } = null!;

    [Column("ADDRESS")]
    [StringLength(500)]
    public string? Address { get; set; }

    [Column("LANDMARK")]
    [StringLength(300)]
    public string? Landmark { get; set; }

    [Column("NURSE_ID")]
    [Precision(10)]
    public int NurseId { get; set; }

    /// <summary>"yyyy-MM-dd" ko'rinishida (frontend date input qiymati).</summary>
    [Column("BOOKING_DATE")]
    [StringLength(10)]
    public string BookingDate { get; set; } = null!;

    /// <summary>"HH:mm" ko'rinishida (frontend time input qiymati).</summary>
    [Column("BOOKING_TIME")]
    [StringLength(5)]
    public string BookingTime { get; set; } = null!;

    [Column("PRESCRIPTION_FILE")]
    [StringLength(300)]
    public string? PrescriptionFile { get; set; }

    /// <summary>Retsept rasmi Base64 data URL sifatida (MVP; keyinchalik fayl xranilishiga ko'chirish mumkin).</summary>
    [Column("PRESCRIPTION_URL", TypeName = "text")]
    public string? PrescriptionUrl { get; set; }

    /// <summary>"Yangi" | "Kelgan" | "Jarayonda" | "Yakunlandi" | "Bekor qilindi".</summary>
    [Column("STATUS")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("TOTAL_PRICE")]
    [Precision(18, 2)]
    public decimal TotalPrice { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(NurseId))]
    public virtual Nurse Nurse { get; set; } = null!;
}
