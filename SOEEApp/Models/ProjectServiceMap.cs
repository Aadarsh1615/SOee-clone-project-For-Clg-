using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOEEApp.Models
{
    public class ProjectServiceMap
    {
        [Key]
        public int ProjServMapID { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required]
        public int ServiceTypeID { get; set; }
        public bool IsActive { get; set; }
        public DateTime AddedOn { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [ForeignKey("ServiceTypeID")]
        public virtual ServiceType ServiceType { get; set; }
    }
}
