using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("SESSIONS", Schema = "CLINIC")]
public partial class Session : IHaveIdProp<long>
{
    public Session()
    {
        MedicineIssues = new HashSet<MedicineIssue>();
    }

    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("COURSE_ID")]
    [Precision(19)]
    public long CourseId { get; set; }

    [Column("SEQ_NO")]
    public int SeqNo { get; set; }

    [Column("SCHEDULED_AT")]
    public DateTimeOffset ScheduledAt { get; set; }

    [Column("ACTUAL_START_AT")]
    public DateTimeOffset? ActualStartAt { get; set; }

    [Column("ACTUAL_END_AT")]
    public DateTimeOffset? ActualEndAt { get; set; }

    [Column("NURSE_ID")]
    [Precision(10)]
    public int NurseId { get; set; }

    [Column("STATUS")]
    public SessionStatus Status { get; set; }

    [Column("NOTES")]
    [StringLength(1000)]
    public string Notes { get; set; }

    [ForeignKey(nameof(CourseId))]
    public virtual TreatmentCourse Course { get; set; }

    [ForeignKey(nameof(NurseId))]
    public virtual Nurse Nurse { get; set; }

    [InverseProperty(nameof(MedicineIssue.Session))]
    public virtual ICollection<MedicineIssue> MedicineIssues { get; set; }
}
