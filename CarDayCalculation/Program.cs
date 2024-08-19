using CarDayCalculation;
using CarDayCalculation.Models;

Console.WriteLine("Hello, World!");
var dbContext = new VehicleDbContext();

try
{


    dbContext.BeginTransaction();

    dbContext.AddVehicle(new Vehicle
    {
        VehicleID = 4,
        VIN = new Random().Next(10000, 99999).ToString(),
        AddDate = new DateTime(2024, 07, 01),
        ExpiryDate = new DateTime(2024, 10, 31)
    });

    //dbContext.AddLocation(new Location { LocationID = 3, LocationName = "Location 3" });
    //dbContext.AddLocation(new Location { LocationID = 4, LocationName = "Location 4" });

    dbContext.AddVehicleLocation(new VehicleLocation
    {
        VehicleLocationID = 7,
        VehicleID = 4,
        LocationID = 4,
        EffectiveDate = new DateTime(2024, 07, 01),
        ExpiryDate = new DateTime(2024, 10, 31)
    });

    //dbContext.AddVehicleLocation(new VehicleLocation
    //{
    //    VehicleLocationID = 6,
    //    VehicleID = 3,
    //    LocationID = 4,
    //    EffectiveDate = new DateTime(2024, 08, 10),
    //    ExpiryDate = new DateTime(2024, 09, 30)
    //});

    //dbContext.AddVehicleCoverage(new VehicleCoverage
    //{
    //    VehicleCoverageID = 19,
    //    CoverageTypeID = 1,
    //    VehicleID = 4,
    //    LocationID = 3,
    //    EffectiveDate = new DateTime(2024, 07, 31),
    //    ExpiryDate = new DateTime(2024, 08, 10)
    //});

    dbContext.AddVehicleCoverage(new VehicleCoverage
    {
        VehicleCoverageID = 20,
        CoverageTypeID = 1,
        VehicleID = 3,
        LocationID = 4,
        EffectiveDate = new DateTime(2024, 07, 01),
        ExpiryDate = new DateTime(2024, 10, 31)
    });

    //dbContext.CommitTransaction();
    dbContext.RollbackTransaction();
    Console.WriteLine("Transaction committed successfully.");
}
catch (Exception ex)
{
    // Rollback the transaction if an error occurs
    dbContext.RollbackTransaction();
    Console.WriteLine($"Transaction rolled back due to an error: {ex.Message}");
}

var allVehicles = dbContext.GetVehicleDetails();
foreach (var vehicle in allVehicles)
{
    Console.WriteLine($"VehicleID:{vehicle.VehicleID}: VIN:{vehicle.VIN}, AddDate:{vehicle.AddDate},ExpiryDate:{vehicle.ExpiryDate}," +
        $"IsUnassigned:{vehicle.IsUnassigned}, LocationID:{vehicle.LocationID}, LocationName:{vehicle.LocationName}, LocationEffectiveDate:{vehicle.LocationEffectiveDate}, LocationExpiryDate:{vehicle.LocationExpiryDate}," +
        $"CoverageName:{vehicle.CoverageName}, CoverageEffectiveDate:{vehicle.CoverageEffectiveDate}, CoverageExpiryDate:{vehicle.CoverageExpiryDate}");
    Console.WriteLine("______________________________________________________________________________________________________________________");
}

var calculator = new CoverageCalculator();
var startDate = new DateTime(2024, 8, 1);
var endDate = new DateTime(2024, 9, 1);

var vehicles = dbContext.Vehicles
    .Where(x => x.ExpiryDate == null || x.ExpiryDate > startDate).ToList();

var vehicleLocations = dbContext.VehicleLocations
    .Where(x => x.EffectiveDate <= endDate && x.ExpiryDate >= startDate).ToList();

var vehicleCoverages = dbContext.VehicleCoverages
    .Where(x => x.EffectiveDate <= endDate && x.ExpiryDate >= startDate).ToList();

var coverageDays = calculator.CalculateCoverageDays(vehicles,
    vehicleLocations, vehicleCoverages, startDate, endDate);

foreach (var entry in coverageDays)
{
    Console.WriteLine($"{entry.Key}: {entry.Value} days");
}