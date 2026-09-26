using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("MEDICINE_ISSUES", Schema = "PHARMACY")]
public partial class MedicineIssue : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("MEDICINE_ID")]
    [Precision(10)]
    public int MedicineId { get; set; }

    [Column("PATIENT_ID")]
    [Precision(19)]
    public long PatientId { get; set; }

    [Column("NURSE_ID")]
    [Precision(10)]
    public int NurseId { get; set; }

    [Column("SESSION_ID")]
    [Precision(19)]
    public long SessionId { get; set; }

    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [Column("SOURCE")]
    public IssueSource Source { get; set; }

    [Column("PRICE_AT_ISSUE")]
    [Precision(18, 2)]
    public decimal PriceAtIssue { get; set; }

    [Column("ISSUED_AT")]
    public DateTimeOffset IssuedAt { get; set; }

    [ForeignKey(nameof(MedicineId))]
    public virtual Medicine Medicine { get; set; }

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; }

    [ForeignKey(nameof(NurseId))]
    public virtual Nurse Nurse { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }
}
