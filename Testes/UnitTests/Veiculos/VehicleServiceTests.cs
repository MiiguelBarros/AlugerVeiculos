using API.Models.Enums;
using API.Veiculos.DTOs;
using API.Veiculos.Exceptions;
using API.Veiculos.Services;
using UnitTests.Infrastructure;

namespace UnitTests.Veiculos
{
    public class VehicleServiceTests : IDisposable
    {
        private readonly TestDatabase database = new();

        private VehicleService CreateService() => new(database.CreateContext());

        private static VehicleRequestDTO CreateRequest(string licensePlate = "AB-12-CD", string model = "Clio")
        {
            return new VehicleRequestDTO
            {
                Brand = "Renault",
                Model = model,
                LicensePlate = licensePlate,
                Year = 2021,
                FuelType = FuelType.Gasoline
            };
        }

        public void Dispose() => database.Dispose();

        [Fact]
        public async Task CreateAsync_DuplicateLicensePlate_ThrowsLicensePlateAlreadyExistsException()
        {
            database.Seed(TestData.CreateVehicle("AB-12-CD"));

            await Assert.ThrowsAsync<LicensePlateAlreadyExistsException>(() => CreateService().CreateAsync(CreateRequest("AB-12-CD")));
        }

        [Fact]
        public async Task CreateAsync_LicensePlateWithDifferentFormatting_StoresNormalizedPlate()
        {
            var vehicle = await CreateService().CreateAsync(CreateRequest("ab 12 cd"));

            Assert.Equal("AB-12-CD", vehicle.LicensePlate);
        }

        [Fact]
        public async Task CreateAsync_DuplicateLicensePlateWithDifferentFormatting_ThrowsLicensePlateAlreadyExistsException()
        {
            database.Seed(TestData.CreateVehicle("AB-12-CD"));

            await Assert.ThrowsAsync<LicensePlateAlreadyExistsException>(() => CreateService().CreateAsync(CreateRequest("ab12cd")));
        }

        [Fact]
        public async Task UpdateAsync_KeepingOwnLicensePlate_Succeeds()
        {
            var existing = TestData.CreateVehicle("AB-12-CD");
            database.Seed(existing);

            var vehicle = await CreateService().UpdateAsync(existing.VehicleId, CreateRequest("AB-12-CD", model: "Megane"));

            Assert.Equal("Megane", vehicle.Model);
        }

        [Fact]
        public async Task UpdateAsync_LicensePlateOfAnotherVehicle_ThrowsLicensePlateAlreadyExistsException()
        {
            var other = TestData.CreateVehicle("CD-34-EF");
            database.Seed(TestData.CreateVehicle("AB-12-CD"), other);

            await Assert.ThrowsAsync<LicensePlateAlreadyExistsException>(() => CreateService().UpdateAsync(other.VehicleId, CreateRequest("AB-12-CD")));
        }

        [Fact]
        public async Task GetAllAsync_VehicleWithOngoingContract_ReturnsRented()
        {
            database.Seed(TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -1, 3));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO());

            Assert.Equal(VehicleAvailability.Rented, Assert.Single(vehicles).Availability);
        }

        [Fact]
        public async Task GetAllAsync_VehicleWithOnlyScheduledContract_ReturnsReserved()
        {
            database.Seed(TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), 2, 4));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO());

            Assert.Equal(VehicleAvailability.Reserved, Assert.Single(vehicles).Availability);
        }

        [Fact]
        public async Task GetAllAsync_AfterContractReturned_ReturnsAvailable()
        {
            database.Seed(TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -3, 2, returnedInDays: 0, endMileage: 1100));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO());

            Assert.Equal(VehicleAvailability.Available, Assert.Single(vehicles).Availability);
        }

        [Fact]
        public async Task GetAllAsync_FilterByAvailability_ReturnsOnlyMatchingVehicles()
        {
            var rented = TestData.CreateVehicle("AB-12-CD");
            database.Seed(TestData.CreateContract(TestData.CreateClient(), rented, -1, 3), TestData.CreateVehicle("CD-34-EF"));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO { Availability = VehicleAvailability.Rented });

            Assert.Equal("AB-12-CD", Assert.Single(vehicles).LicensePlate);
        }

        [Fact]
        public async Task GetAllAsync_VehicleWithReturnedContracts_ReturnsLastMileage()
        {
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(
                TestData.CreateContract(client, vehicle, -20, -15, startMileage: 14000, returnedInDays: -15, endMileage: 14800),
                TestData.CreateContract(client, vehicle, -10, -5, startMileage: 14800, returnedInDays: -5, endMileage: 15300));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO());

            Assert.Equal(15300, Assert.Single(vehicles).LastMileage);
        }

        [Fact]
        public async Task GetAllAsync_VehicleWithoutReturnedContracts_ReturnsNoLastMileage()
        {
            database.Seed(TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -1, 3));

            var vehicles = await CreateService().GetAllAsync(new VehicleFilterDTO());

            Assert.Null(Assert.Single(vehicles).LastMileage);
        }

        [Fact]
        public async Task DeactivateAsync_VehicleWithPendingContract_ThrowsVehicleHasPendingContractsException()
        {
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(TestData.CreateClient(), vehicle, 2, 4));

            await Assert.ThrowsAsync<VehicleHasPendingContractsException>(() => CreateService().DeactivateAsync(vehicle.VehicleId));
        }

        [Fact]
        public async Task DeactivateAsync_VehicleWithoutPendingContracts_SetsStatusInactive()
        {
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(TestData.CreateClient(), vehicle, -10, -5, returnedInDays: -5, endMileage: 1500));

            await CreateService().DeactivateAsync(vehicle.VehicleId);

            Assert.Equal(RecordStatus.Inactive, (await CreateService().GetByIdAsync(vehicle.VehicleId)).Status);
        }

        [Fact]
        public async Task DeactivateAsync_AlreadyInactive_ThrowsVehicleStatusConflictException()
        {
            var vehicle = TestData.CreateVehicle(status: RecordStatus.Inactive);
            database.Seed(vehicle);

            await Assert.ThrowsAsync<VehicleStatusConflictException>(() => CreateService().DeactivateAsync(vehicle.VehicleId));
        }

        [Fact]
        public async Task ReactivateAsync_AlreadyActive_ThrowsVehicleStatusConflictException()
        {
            var vehicle = TestData.CreateVehicle();
            database.Seed(vehicle);

            await Assert.ThrowsAsync<VehicleStatusConflictException>(() => CreateService().ReactivateAsync(vehicle.VehicleId));
        }
    }
}
