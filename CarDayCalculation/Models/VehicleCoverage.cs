namespace CarDayCalculation.Models
{
    public class VehicleCoverage
    {
        public int VehicleCoverageID { get; set; }
        public int VehicleID { get; set; }
        public int LocationID { get; set; }
        public int CoverageTypeID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
