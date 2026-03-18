using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public class SOEEItem
    {
        [Key]
        public int SOEEItemID { get; set; }

        [Required]
        public int SOEEID { get; set; }
        public virtual SOEE SOEE { get; set; }

        [Required]
        public int ServiceTypeID { get; set; }
        public virtual ServiceType ServiceType { get; set; }
        [Required]
        public string DescriptionOfWork { get; set; }
        public decimal Quantity { get; set; }
        public decimal Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ServiceChargePercent { get; set; }   // <--- NEW COLUMN
        public decimal ServiceCharge { get; set; }
        public decimal SubTotal { get; set; }   // STORE
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Total { get; set; }      // STORE
        public bool IsDeleted { get; set; } = false;
    }
}
