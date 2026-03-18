using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public class ServiceTypeSlabMap
    {
        public int Id { get; set; }

        public int ServiceTypeID { get; set; }
        public virtual ServiceType ServiceType { get; set; }

        public int SlabID { get; set; }   // FOREIGN KEY to ServiceCostSlab.Id
        public virtual ServiceCostSlab Slab { get; set; }

        public decimal Percentage { get; set; }
    }

}
