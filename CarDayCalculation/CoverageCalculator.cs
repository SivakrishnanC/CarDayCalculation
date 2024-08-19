using CarDayCalculation.Models;

namespace CarDayCalculation
{
    public class CoverageCalculator
    {
        public Dictionary<string, int> CalculateCoverageDays(
            List<Vehicle> vehicles,
            List<VehicleLocation> vehicleLocations,
            List<VehicleCoverage> vehicleCoverages,
            DateTime startDate,
            DateTime endDate)
        {
            var coverageDays = new Dictionary<string, int>();

            foreach (var vehicle in vehicles)
            {
                var vehicleLocationsForVehicle = vehicleLocations
                    .Where(vl => vl.VehicleID == vehicle.VehicleID)
                    .ToList();

                foreach (var location in vehicleLocationsForVehicle)
                {
                    var coveragesForLocation = vehicleCoverages
                        .Where(vc => vc.VehicleID == vehicle.VehicleID && vc.LocationID == location.LocationID)
                        .ToList();

                    foreach (var coverage in coveragesForLocation)
                    {
                        var effectiveStartDate = coverage.EffectiveDate > startDate ? coverage.EffectiveDate : startDate;
                        var effectiveEndDate = coverage.ExpiryDate.HasValue && coverage.ExpiryDate.Value < endDate
                            ? coverage.ExpiryDate.Value
                            : endDate;

                        if (effectiveEndDate >= effectiveStartDate)
                        {
                            var daysCovered = (effectiveEndDate - effectiveStartDate).Days;
                            var key = $"Vehicle {vehicle.VehicleID}, Location {location.LocationID}, Coverage {coverage.CoverageTypeID}";

                            if (coverageDays.ContainsKey(key))
                            {
                                coverageDays[key] += daysCovered;
                            }
                            else
                            {
                                coverageDays[key] = daysCovered;
                            }
                        }
                    }
                }
            }

            return coverageDays;
        }
    }

}
