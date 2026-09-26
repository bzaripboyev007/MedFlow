using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("NURSES", Schema = "HRM")]
public partial class Nurse : IHaveIdProp<int>
{
    public Nurse()
    {
        PrimaryCourses = new HashSet<TreatmentCourse>();
        Sessions = new HashSet<Session>();
    }

    [Key]
    [Column("ID")]
    [Precision(10)]
    public int Id { get; set; }

    [Column("FULL_NAME")]
    [StringLength(200)]
    public string FullName { get; set; }

    [Column("TYPE")]
    public NurseType Type { get; set; }

    [Column("GENDER")]
    public Gender? Gender { get; set; }

    [Column("PHONE")]
    [StringLength(30)]
    public string Phone { get; set; }

    [Column("SPECIALIZATION")]
    [StringLength(200)]
    public string Specialization { get; set; }

    [Column("EXPERIENCE_START_DATE", TypeName = "DATE")]
    public DateTime ExperienceStartDate { get; set; }

    [Column("LOGIN")]
    [StringLength(100)]
    public string Login { get; set; }

    [Column("RATING")]
    [Precision(3, 2)]
    public decimal Rating { get; set; }

    /// <summary>Bitta tashrif (muolaja) uchun hamshiralik xizmati narxi.</summary>
    [Column("PRICE_PER_VISIT")]
    [Precision(18, 2)]
    public decimal PricePerVisit { get; set; }

    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; }

    [Column("HIRED_AT")]
    public DateTimeOffset HiredAt { get; set; }

    [InverseProperty(nameof(TreatmentCourse.PrimaryNurse))]
    public virtual ICollection<TreatmentCourse> PrimaryCourses { get; set; }
    [InverseProperty(nameof(Session.Nurse))]
    public virtual ICollection<Session> Sessions { get; set; }
}
