using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("PATIENTS", Schema = "ADM")]
public partial class Patient : IHaveIdProp<long>
{
    public Patient()
    {
        Children = new HashSet<Patient>();
        Prescriptions = new HashSet<Prescription>();
        Courses = new HashSet<TreatmentCourse>();
        WalletEntries = new HashSet<PatientMedicineWallet>();
    }

    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("FULL_NAME")]
    [StringLength(200)]
    public string FullName { get; set; }

    [Column("BIRTH_DATE", TypeName = "DATE")]
    public DateTime BirthDate { get; set; }

    [Column("PHONE")]
    [StringLength(30)]
    public string Phone { get; set; }

    [Column("PARENT_ID")]
    [Precision(19)]
    public long? ParentId { get; set; }

    [Column("IS_CHILD")]
    public bool IsChild { get; set; }

    [Column("ALLERGIES")]
    [StringLength(1000)]
    public string Allergies { get; set; }

    [Column("CREATED_AT")]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(ParentId))]
    public virtual Patient Parent { get; set; }

    [InverseProperty(nameof(Patient.Parent))]
    public virtual ICollection<Patient> Children { get; set; }

    [InverseProperty(nameof(Prescription.Patient))]
    public virtual ICollection<Prescription> Prescriptions { get; set; }

    [InverseProperty(nameof(TreatmentCourse.Patient))]
    public virtual ICollection<TreatmentCourse> Courses { get; set; }

    [InverseProperty(nameof(PatientMedicineWallet.Patient))]
    public virtual ICollection<PatientMedicineWallet> WalletEntries { get; set; }
}
