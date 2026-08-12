using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("SERVICE_TYPES", Schema = "PAYMENT")]
public partial class ServiceType : IHaveIdProp<int>
{
    public ServiceType()
    {
        Courses = new HashSet<TreatmentCourse>();
    }

    [Key]
    [Column("ID")]
    [Precision(10)]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(200)]
    public string Name { get; set; }

    [Column("PRICE_PER_SESSION")]
    [Precision(18, 2)]
    public decimal PricePerSession { get; set; }

    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; }

    [InverseProperty(nameof(TreatmentCourse.ServiceType))]
    public virtual ICollection<TreatmentCourse> Courses { get; set; }
}
