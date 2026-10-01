using API.Clientes.DTOs;
using API.Clientes.Exceptions;
using API.Clientes.Services;
using API.Models.Enums;
using UnitTests.Infrastructure;

namespace UnitTests.Clientes
{
    public class ClientServiceTests : IDisposable
    {
        private readonly TestDatabase database = new();

        private ClientService CreateService() => new(database.CreateContext());

        private static ClientRequestDTO CreateRequest(string email = "ana@cliente.com", string driverLicenseNumber = "P-1234567", string fullName = "Ana Ferreira")
        {
            return new ClientRequestDTO
            {
                FullName = fullName,
                Email = email,
                Phone = "912345678",
                DriverLicenseNumber = driverLicenseNumber
            };
        }

        public void Dispose() => database.Dispose();

        [Fact]
        public async Task CreateAsync_DuplicateEmail_ThrowsClientEmailAlreadyExistsException()
        {
            database.Seed(TestData.CreateClient(email: "ana@cliente.com", driverLicenseNumber: "P-1234567"));

            await Assert.ThrowsAsync<ClientEmailAlreadyExistsException>(() =>
                CreateService().CreateAsync(CreateRequest(email: "ana@cliente.com", driverLicenseNumber: "L-7654321")));
        }

        [Fact]
        public async Task CreateAsync_DuplicateDriverLicense_ThrowsDriverLicenseAlreadyExistsException()
        {
            database.Seed(TestData.CreateClient(email: "ana@cliente.com", driverLicenseNumber: "P-1234567"));

            await Assert.ThrowsAsync<DriverLicenseAlreadyExistsException>(() =>
                CreateService().CreateAsync(CreateRequest(email: "outro@cliente.com", driverLicenseNumber: "P-1234567")));
        }

        [Fact]
        public async Task CreateAsync_DriverLicenseInLowercase_StoresUppercase()
        {
            var client = await CreateService().CreateAsync(CreateRequest(driverLicenseNumber: "p-1234567"));

            Assert.Equal("P-1234567", client.DriverLicenseNumber);
        }

        [Fact]
        public async Task UpdateAsync_KeepingOwnEmail_Succeeds()
        {
            var existing = TestData.CreateClient(email: "ana@cliente.com");
            database.Seed(existing);

            var client = await CreateService().UpdateAsync(existing.ClientId, CreateRequest(email: "ana@cliente.com", fullName: "Ana Maria Ferreira"));

            Assert.Equal("Ana Maria Ferreira", client.FullName);
        }

        [Fact]
        public async Task DeactivateAsync_ClientWithPendingContract_ThrowsClientHasPendingContractsException()
        {
            var client = TestData.CreateClient();
            database.Seed(TestData.CreateContract(client, TestData.CreateVehicle(), 2, 4));

            await Assert.ThrowsAsync<ClientHasPendingContractsException>(() => CreateService().DeactivateAsync(client.ClientId));
        }

        [Fact]
        public async Task DeactivateAsync_ClientWithoutPendingContracts_SetsStatusInactive()
        {
            var client = TestData.CreateClient();
            database.Seed(TestData.CreateContract(client, TestData.CreateVehicle(), -10, -5, returnedInDays: -5, endMileage: 1500));

            await CreateService().DeactivateAsync(client.ClientId);

            Assert.Equal(RecordStatus.Inactive, (await CreateService().GetByIdAsync(client.ClientId)).Status);
        }
    }
}
