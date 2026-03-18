using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOEEApp.Models
{
    public class ManageServicesViewModel
    {
        public Project Project { get; set; }
        public List<ServiceType> AllServices { get; set; }
        public List<ProjectServiceMap> ExistingServices { get; set; }

        public int SelectedServiceTypeID { get; set; }
    }

}