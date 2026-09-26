using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("TREATMENT_COURSES", Schema = "CLINIC")]
public partial class TreatmentCourse : IHaveIdProp<long>
{
    public TreatmentCourse()
    {
        Sessions = new HashSet<Session>();
    }

    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PATIENT_ID")]
    [Precision(19)]
    public long PatientId { get; set; }

    [Column("PRESCRIPTION_ID")]
    [Precision(19)]
    public long PrescriptionId { get; set; }

    [Column("PRIMARY_NURSE_ID")]
    [Precision(10)]
    public int PrimaryNurseId { get; set; }

    [Column("SERVICE_TYPE_ID")]
    [Precision(10)]
    public int ServiceTypeId { get; set; }

    [Column("LOCATION_TYPE")]
    public LocationType LocationType { get; set; }

    [Column("ADDRESS")]
    [StringLength(500)]
    public string Address { get; set; }

    [Column("DURATION_DAYS")]
    public int DurationDays { get; set; }

    [Column("TIMES_PER_DAY")]
    public int TimesPerDay { get; set; }

    [Column("PREFERRED_TIMES_MINUTES")]
    public int[] PreferredTimesMinutes { get; set; } = Array.Empty<int>();

    [NotMapped]
    public IEnumerable<TimeSpan> PreferredTimes => PreferredTimesMinutes.Select(m => TimeSpan.FromMinutes(m));

    [Column("START_DATE", TypeName = "DATE")]
    public DateTime StartDate { get; set; }

    [Column("TOTAL_SESSIONS")]
    public int TotalSessions { get; set; }

    [Column("COMPLETED_SESSIONS")]
    public int CompletedSessions { get; set; }

    [Column("STATUS")]
    public CourseStatus Status { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; }

    [ForeignKey(nameof(PrescriptionId))]
    public virtual Prescription Prescription { get; set; }

    [ForeignKey(nameof(PrimaryNurseId))]
    public virtual Nurse PrimaryNurse { get; set; }

    [ForeignKey(nameof(ServiceTypeId))]
    public virtual ServiceType ServiceType { get; set; }

    [InverseProperty(nameof(Session.Course))]
    public virtual ICollection<Session> Sessions { get; set; }
}
