using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedFlow.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Domain.Entities;

[Table("MEDICINES", Schema = "PHARMACY")]
public partial class Medicine : IHaveIdProp<int>
{
    public Medicine()
    {
        WalletEntries = new HashSet<PatientMedicineWallet>();
        Issues = new HashSet<MedicineIssue>();
    }

    [Key]
    [Column("ID")]
    [Precision(10)]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(200)]
    public string Name { get; set; }

    [Column("UNIT")]
    [StringLength(50)]
    public string Unit { get; set; }

    [Column("PRICE")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    [Column("STOCK_QTY")]
    public int StockQty { get; set; }

    [Column("MIN_STOCK_LEVEL")]
    public int MinStockLevel { get; set; }

    [Column("EXPIRY_DATE", TypeName = "DATE")]
    public DateTime ExpiryDate { get; set; }

    [InverseProperty(nameof(PatientMedicineWallet.Medicine))]
    public virtual ICollection<PatientMedicineWallet> WalletEntries { get; set; }
    [InverseProperty(nameof(MedicineIssue.Medicine))]
    public virtual ICollection<MedicineIssue> Issues { get; set; }
}
