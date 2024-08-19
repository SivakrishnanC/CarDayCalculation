namespace CarDayCalculation.Models
{
    public class Vehicle
    {
        public int VehicleID { get; set; }
        public string VIN { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsUnassigned { get; set; }
    }

}
