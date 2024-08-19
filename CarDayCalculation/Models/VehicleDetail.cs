namespace CarDayCalculation.Models
{
    public class VehicleDetail
    {
        public int VehicleID { get; set; }
        public string VIN { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsUnassigned { get; set; }
        public int LocationID { get; set; }
        public Vehicle LocationName { get; set; }
        public DateTime LocationEffectiveDate { get; set; }
        public DateTime? LocationExpiryDate { get; set; }
        public string CoverageName { get; set; }
        public DateTime CoverageEffectiveDate { get; set; }
        public DateTime? CoverageExpiryDate { get; set; }
    }
}