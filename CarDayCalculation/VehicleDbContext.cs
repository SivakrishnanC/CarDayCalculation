using CarDayCalculation.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CarDayCalculation
{
    public class VehicleDbContext
    {
        //for transaction/applying or rollbacking changes
        private List<Vehicle> _stagedVehicles;
        private List<Location> _stagedLocations;
        private List<VehicleLocation> _stagedVehicleLocations;
        private List<VehicleCoverage> _stagedVehicleCoverages;

        public List<Vehicle> Vehicles { get; set; }
        public List<Location> Locations { get; set; }
        public List<CoverageType> CoverageTypes { get; set; }
        public List<VehicleLocation> VehicleLocations { get; set; }
        public List<VehicleCoverage> VehicleCoverages { get; set; }

        public VehicleDbContext()
        {
            // Load JSON data from files and populate the collections
            Vehicles = LoadDataFromFile<List<Vehicle>>("Vehicles.json");
            Locations = LoadDataFromFile<List<Location>>("Locations.json");
            CoverageTypes = LoadDataFromFile<List<CoverageType>>("CoverageTypes.json");
            VehicleLocations = LoadDataFromFile<List<VehicleLocation>>("VehicleLocations.json");
            VehicleCoverages = LoadDataFromFile<List<VehicleCoverage>>("VehicleCoverages.json");

            // Initialize staging areas
            _stagedVehicles = Vehicles.ToList();
            _stagedLocations = Locations.ToList();
            _stagedVehicleLocations = VehicleLocations.ToList();
            _stagedVehicleCoverages = VehicleCoverages.ToList();
        }

        private T LoadDataFromFile<T>(string filePath)
        {
            var appBasePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.ToString();
            var completeFilePath = $"{appBasePath}\\Data\\{filePath}";

            if (File.Exists(completeFilePath))
            {
                var jsonData = File.ReadAllText(completeFilePath);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }

            return default;
        }

        private void SaveDataToFile<T>(string filePath, T data)
        {
            var appBasePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.ToString();
            var completeFilePath = $"{appBasePath}\\Data\\{filePath}";

            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(completeFilePath, jsonData);
        }

        public void BeginTransaction()
        {
            // Initialize staging areas for transactions
            _stagedVehicles = Vehicles.ToList();
            _stagedLocations = Locations.ToList();
            _stagedVehicleLocations = VehicleLocations.ToList();
            _stagedVehicleCoverages = VehicleCoverages.ToList();
        }

        public void CommitTransaction()
        {
            // Commit staged data to the main collections
            Vehicles = _stagedVehicles;
            Locations = _stagedLocations;
            VehicleLocations = _stagedVehicleLocations;
            VehicleCoverages = _stagedVehicleCoverages;

            // Persist data to JSON files
            SaveChanges();
        }

        public void RollbackTransaction()
        {
            // Discard staged data (reverting to original state)
            _stagedVehicles = Vehicles.ToList();
            _stagedLocations = Locations.ToList();
            _stagedVehicleLocations = VehicleLocations.ToList();
            _stagedVehicleCoverages = VehicleCoverages.ToList();
        }

        public void SaveChanges()
        {
            SaveDataToFile("Vehicles.json", Vehicles);
            SaveDataToFile("Locations.json", Locations);
            SaveDataToFile("VehicleLocations.json", VehicleLocations);
            SaveDataToFile("VehicleCoverages.json", VehicleCoverages);
        }

        public List<VehicleDetail> GetVehicleDetails()
        {
            return _stagedVehicles
                 .Join(_stagedVehicleLocations, v => v.VehicleID, vl => vl.VehicleID, (vehicle, vehicleLocation) => new
                 {
                     Vehicle = vehicle,
                     VehicleLocation = vehicleLocation
                 })
                 .Join(_stagedVehicleCoverages, v => new { v.Vehicle.VehicleID, v.VehicleLocation.LocationID }, vl => new { vl.VehicleID, vl.LocationID }, (vehicle, vehicleCoverage) => new
                 {
                     vehicle.Vehicle,
                     vehicle.VehicleLocation,
                     VehicleCoverage = vehicleCoverage
                 }
                 )
                 .Join(_stagedLocations, vl => vl.VehicleLocation.LocationID, l => l.LocationID, (vehicle, location) => new
                 {
                     vehicle.Vehicle,
                     vehicle.VehicleLocation,
                     vehicle.VehicleCoverage,
                     location.LocationName

                 })
                 .Join(CoverageTypes, c => c.VehicleCoverage.CoverageTypeID, c => c.CoverageTypeID, (vehicle, coverage) => new
                 {
                     vehicle.Vehicle,
                     vehicle.VehicleLocation,
                     vehicle.VehicleCoverage,
                     vehicle.LocationName,
                     coverage.CoverageName
                 })
                 .Select(v => new VehicleDetail
                 {
                     VehicleID = v.Vehicle.VehicleID,
                     VIN = v.Vehicle.VIN,
                     AddDate = v.Vehicle.AddDate,
                     ExpiryDate = v.Vehicle.ExpiryDate,
                     IsUnassigned = v.Vehicle.IsUnassigned,
                     LocationID = v.VehicleLocation.LocationID,
                     LocationName = v.Vehicle,
                     LocationEffectiveDate = v.VehicleLocation.EffectiveDate,
                     LocationExpiryDate = v.VehicleLocation.ExpiryDate,
                     CoverageName = v.CoverageName,
                     CoverageEffectiveDate = v.VehicleCoverage.EffectiveDate,
                     CoverageExpiryDate = v.VehicleCoverage.ExpiryDate
                 }).ToList();
        }

        // Transactional add methods
        public void AddVehicle(Vehicle vehicle)
        {
            _stagedVehicles.Add(vehicle);
        }

        public void AddLocation(Location location)
        {
            _stagedLocations.Add(location);
        }

        public void AddVehicleLocation(VehicleLocation vehicleLocation)
        {
            _stagedVehicleLocations.Add(vehicleLocation);
        }

        public void AddVehicleCoverage(VehicleCoverage vehicleCoverage)
        {
            _stagedVehicleCoverages.Add(vehicleCoverage);
        }

        public void DeleteVehicleLocation(int vehicleId, int locationId, DateTime transactionDate)
        {
            _stagedVehicleLocations
                .Where(x => x.VehicleID == vehicleId && x.LocationID == locationId)
                .ToList()
                .ForEach(x => x.ExpiryDate = transactionDate);

            _stagedVehicles
                .Where(x => x.VehicleID == vehicleId)
                .ToList()
                .ForEach(x => x.IsUnassigned = true);
        }

        public void DeleteVehicleCoverage(int vehicleId, int coverageTypeId, DateTime transactionDate)
        {
            _stagedVehicleCoverages
                .Where(x => x.VehicleID == vehicleId && x.CoverageTypeID == coverageTypeId)
                .ToList()
                .ForEach(x => x.ExpiryDate = transactionDate);
        }

        //Out of sequence changes
        public void AddOrUpdateVehicleLocationOutOfSequence(VehicleLocation newLocation)
        {
            BeginTransaction();

            try
            {
                // Find overlapping location records for the same vehicle
                var overlappingRecords = _stagedVehicleLocations
                    .Where(vl => vl.VehicleID == newLocation.VehicleID
                                 && vl.EffectiveDate <= newLocation.EffectiveDate
                                 && (vl.ExpiryDate == null || vl.ExpiryDate >= newLocation.EffectiveDate))
                    .ToList();

                // Adjust expiry dates of overlapping records
                foreach (var record in overlappingRecords)
                {
                    if (record.EffectiveDate < newLocation.EffectiveDate)
                    {
                        record.ExpiryDate = newLocation.EffectiveDate;//.AddDays(-1);
                    }
                    else
                    {
                        _stagedVehicleLocations.Remove(record); // Remove fully overlapping records
                    }
                }

                // Adjust subsequent records if they exist
                var subsequentRecords = _stagedVehicleLocations
                    .Where(vl => vl.VehicleID == newLocation.VehicleID
                                 && vl.EffectiveDate > newLocation.EffectiveDate)
                    .ToList();

                if (subsequentRecords.Any())
                {
                    var firstSubsequentRecord = subsequentRecords.OrderBy(vl => vl.EffectiveDate).First();
                    newLocation.ExpiryDate = firstSubsequentRecord.EffectiveDate;//.AddDays(-1);
                }

                // Add the new location record
                _stagedVehicleLocations.Add(newLocation);

                // Commit the transaction
                CommitTransaction();
            }
            catch (Exception ex)
            {
                // Rollback transaction if something goes wrong
                RollbackTransaction();
                Console.WriteLine($"Transaction rolled back due to an error: {ex.Message}");
            }
        }

        public void AddOrUpdateVehicleCoverageOutOfSequence(VehicleCoverage newCoverage)
        {
            BeginTransaction();

            try
            {
                // Find overlapping coverage records for the same vehicle and location
                var overlappingRecords = _stagedVehicleCoverages
                    .Where(vc => vc.VehicleID == newCoverage.VehicleID
                                 && vc.LocationID == newCoverage.LocationID
                                 && vc.EffectiveDate <= newCoverage.EffectiveDate
                                 && (vc.ExpiryDate == null || vc.ExpiryDate >= newCoverage.EffectiveDate))
                    .ToList();

                // Adjust expiry dates of overlapping records
                foreach (var record in overlappingRecords)
                {
                    if (record.EffectiveDate < newCoverage.EffectiveDate)
                    {
                        record.ExpiryDate = newCoverage.EffectiveDate;//.AddDays(-1);
                    }
                    else
                    {
                        _stagedVehicleCoverages.Remove(record); // Remove fully overlapping records
                    }
                }

                // Adjust subsequent records if they exist
                var subsequentRecords = _stagedVehicleCoverages
                    .Where(vc => vc.VehicleID == newCoverage.VehicleID
                                 && vc.LocationID == newCoverage.LocationID
                                 && vc.EffectiveDate > newCoverage.EffectiveDate)
                    .ToList();

                if (subsequentRecords.Any())
                {
                    var firstSubsequentRecord = subsequentRecords.OrderBy(vc => vc.EffectiveDate).First();
                    newCoverage.ExpiryDate = firstSubsequentRecord.EffectiveDate;//.AddDays(-1);
                }

                // Add the new coverage record
                _stagedVehicleCoverages.Add(newCoverage);

                // Commit the transaction
                CommitTransaction();
            }
            catch (Exception ex)
            {
                // Rollback transaction if something goes wrong
                RollbackTransaction();
                Console.WriteLine($"Transaction rolled back due to an error: {ex.Message}");
            }
        }

    }
}