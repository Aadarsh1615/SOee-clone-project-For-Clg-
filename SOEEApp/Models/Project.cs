using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOEEApp.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }
        public string Name { get; set; }
        // store selected OIC user id (FK to AspNetUsers / ApplicationUser)
        public string OICUserId { get; set; }

        // navigation (optional)
        public virtual ApplicationUser OICUser { get; set; }

        [Display(Name = "Project Cost (Rs.)")]
        public decimal ProjectCost { get; set; }

        public string Status { get; set; }

        public virtual ICollection<ProjectServiceMap> ProjectServiceMaps { get; set; }
    }
}
