using System;

namespace SOEEApp.Models
{
    public class SOEEActivity
    {
        public int SOEEActivityID { get; set; }

        public int SOEEID { get; set; }
        public virtual SOEE SOEE { get; set; }

        public SOEEStatus OldStatus { get; set; }
        public SOEEStatus NewStatus { get; set; }

        public string ActionBy { get; set; }
        public DateTime ActionOn { get; set; }
        public string Remarks { get; set; }
    }
}
