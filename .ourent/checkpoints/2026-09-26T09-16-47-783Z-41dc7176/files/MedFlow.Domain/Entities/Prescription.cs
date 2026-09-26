using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("PRESCRIPTIONS", Schema = "CLINIC")]
public partial class Prescription : IHaveIdProp<long>
{
    public Prescription()
    {
        Courses = new HashSet<TreatmentCourse>();
    }

    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PATIENT_ID")]
    [Precision(19)]
    public long PatientId { get; set; }

    [Column("FILE_URL")]
    [StringLength(500)]
    public string FileUrl { get; set; }

    [Column("RECOGNIZED_TEXT")]
    public string RecognizedText { get; set; }

    [Column("VERIFIED_BY_NURSE_ID")]
    [Precision(10)]
    public int? VerifiedByNurseId { get; set; }

    [Column("VERIFIED_AT")]
    public DateTimeOffset? VerifiedAt { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; }

    [ForeignKey(nameof(VerifiedByNurseId))]
    public virtual Nurse VerifiedByNurse { get; set; }

    [InverseProperty(nameof(TreatmentCourse.Prescription))]
    public virtual ICollection<TreatmentCourse> Courses { get; set; }
}
