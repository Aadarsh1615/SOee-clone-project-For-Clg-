namespace SOEEApp.Models
{
    public class ServiceTypeSlabMatrixRow
    {
        public string ServiceType { get; set; }
        public decimal? Range0To25Lac { get; set; }
        public decimal? Range25LacTo1Cr { get; set; }
        public decimal? Range1CrPlus { get; set; }
    }
}
