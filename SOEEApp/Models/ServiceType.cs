using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public class ServiceType
    {
        [Key]
        public int ServiceTypeID { get; set; }
        public string ServiceName { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<ProjectServiceMap> ProjectServiceMaps { get; set; }
    }
}
