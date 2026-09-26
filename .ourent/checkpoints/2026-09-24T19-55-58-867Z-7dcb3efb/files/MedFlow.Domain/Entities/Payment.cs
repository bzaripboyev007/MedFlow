using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("PAYMENTS", Schema = "PAYMENT")]
public partial class Payment : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PATIENT_ID")]
    [Precision(19)]
    public long PatientId { get; set; }

    [Column("COURSE_ID")]
    [Precision(19)]
    public long CourseId { get; set; }

    [Column("SERVICE_AMOUNT")]
    [Precision(18, 2)]
    public decimal ServiceAmount { get; set; }

    [Column("MEDICINE_AMOUNT")]
    [Precision(18, 2)]
    public decimal MedicineAmount { get; set; }

    [Column("TRANSPORT_AMOUNT")]
    [Precision(18, 2)]
    public decimal TransportAmount { get; set; }

    [Column("TOTAL_AMOUNT")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Column("METHOD")]
    public PaymentMethod Method { get; set; }

    [Column("PAID_AT")]
    public DateTimeOffset PaidAt { get; set; }

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual TreatmentCourse Course { get; set; }
}
