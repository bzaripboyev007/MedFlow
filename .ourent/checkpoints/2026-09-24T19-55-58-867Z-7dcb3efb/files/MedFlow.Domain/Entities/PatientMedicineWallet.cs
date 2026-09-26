using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("PATIENT_MEDICINE_WALLETS", Schema = "PHARMACY")]
public partial class PatientMedicineWallet : IHaveIdProp<long>
{
    [Key]
    [Column("ID")]
    [Precision(19)]
    public long Id { get; set; }

    [Column("PATIENT_ID")]
    [Precision(19)]
    public long PatientId { get; set; }

    [Column("MEDICINE_ID")]
    [Precision(10)]
    public int MedicineId { get; set; }

    [Column("PURCHASED_QTY")]
    public int PurchasedQty { get; set; }

    [Column("REMAINING_QTY")]
    public int RemainingQty { get; set; }

    [Column("SOURCE")]
    public WalletSource Source { get; set; }

    [Column("PURCHASED_AT")]
    public DateTimeOffset PurchasedAt { get; set; }

    [ForeignKey(nameof(PatientId))]
    public virtual Patient Patient { get; set; }

    [ForeignKey(nameof(MedicineId))]
    public virtual Medicine Medicine { get; set; }
}
