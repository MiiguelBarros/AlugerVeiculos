using API.Models;
using API.Models.Enums;

namespace UnitTests.Infrastructure
{
    public static class TestData
    {
        public const string Password = "Strong123@";

        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        public static Vehicle CreateVehicle(string licensePlate = "AB-12-CD", RecordStatus status = RecordStatus.Active)
        {
            return new Vehicle
            {
                Brand = "Renault",
                Model = "Clio",
                LicensePlate = licensePlate,
                Year = 2021,
                FuelType = FuelType.Gasoline,
                Status = status
            };
        }

        public static Client CreateClient(string email = "ana@cliente.com", string driverLicenseNumber = "P-1234567", RecordStatus status = RecordStatus.Active)
        {
            return new Client
            {
                FullName = "Ana Ferreira",
                Email = email,
                Phone = "912345678",
                DriverLicenseNumber = driverLicenseNumber,
                Status = status
            };
        }

        public static User CreateUser(string email = "manager@test.com", Role role = Role.Manager, RecordStatus status = RecordStatus.Active)
        {
            return new User
            {
                Name = "Marta Sousa",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password, 4),
                Role = role,
                Status = status
            };
        }

        public static Contract CreateContract(Client client, Vehicle vehicle, int startInDays, int endInDays,
            int startMileage = 1000, int? returnedInDays = null, int? endMileage = null, bool cancelled = false, User? createdBy = null)
        {
            return new Contract
            {
                Client = client,
                Vehicle = vehicle,
                CreatedByUser = createdBy ?? CreateUser($"{Guid.NewGuid():N}@test.com", Role.Employee),
                StartDate = Today.AddDays(startInDays),
                EndDate = Today.AddDays(endInDays),
                StartMileage = startMileage,
                ReturnedAt = returnedInDays == null ? null : Today.AddDays(returnedInDays.Value),
                EndMileage = endMileage,
                CancelledAt = cancelled ? DateTime.UtcNow : null
            };
        }
    }
}
