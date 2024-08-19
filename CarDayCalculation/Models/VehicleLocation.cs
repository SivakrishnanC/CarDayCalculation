namespace CarDayCalculation.Models
{
    public class VehicleLocation
    {
        public int VehicleLocationID { get; set; }
        public int VehicleID { get; set; }
        public int LocationID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
