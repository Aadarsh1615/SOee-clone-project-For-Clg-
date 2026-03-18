using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public class ServiceCostSlab
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Min Amount")]
        public decimal MinAmount { get; set; }

        [Display(Name = "Max Amount (Leave blank for unlimited)")]
        public decimal? MaxAmount { get; set; }

        public bool IsActive { get; set; }

        public ServiceCostSlab()
        {
            IsActive = true;
        }
    }
}
