using API.Clientes.Exceptions;
using API.Contratos.DTOs;
using API.Contratos.Exceptions;
using API.Contratos.Services;
using API.Models;
using API.Models.Enums;
using API.Veiculos.Exceptions;
using UnitTests.Infrastructure;

namespace UnitTests.Contratos
{
    public class ContractServiceTests : IDisposable
    {
        private readonly TestDatabase database = new();

        private ContractService CreateService() => new(database.CreateContext());

        private static CreateContractDTO CreateRequest(Client client, Vehicle vehicle, int startInDays, int endInDays, int startMileage = 1000)
        {
            return new CreateContractDTO
            {
                ClientId = client.ClientId,
                VehicleId = vehicle.VehicleId,
                StartDate = TestData.Today.AddDays(startInDays),
                EndDate = TestData.Today.AddDays(endInDays),
                StartMileage = startMileage
            };
        }

        public void Dispose() => database.Dispose();

        [Fact]
        public async Task CreateAsync_ClientNotFound_ThrowsClientNotFoundException()
        {
            var vehicle = TestData.CreateVehicle();
            database.Seed(vehicle);
            var request = CreateRequest(new Client { ClientId = 999 }, vehicle, 1, 5);

            await Assert.ThrowsAsync<ClientNotFoundException>(() => CreateService().CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_VehicleNotFound_ThrowsVehicleNotFoundException()
        {
            var client = TestData.CreateClient();
            database.Seed(client);
            var request = CreateRequest(client, new Vehicle { VehicleId = 999 }, 1, 5);

            await Assert.ThrowsAsync<VehicleNotFoundException>(() => CreateService().CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ValidContract_CreatesScheduledContract()
        {
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(client, vehicle);

            var contract = await CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 5));

            Assert.Equal(ContractStatus.Scheduled, contract.Status);
            Assert.Single(await CreateService().GetAllAsync(new ContractFilterDTO()));
        }

        [Fact]
        public async Task CreateAsync_InactiveClient_ThrowsClientUnavailableException()
        {
            var client = TestData.CreateClient(status: RecordStatus.Inactive);
            var vehicle = TestData.CreateVehicle();
            database.Seed(client, vehicle);

            await Assert.ThrowsAsync<ClientUnavailableException>(() => CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 5)));
        }

        [Fact]
        public async Task CreateAsync_InactiveVehicle_ThrowsVehicleUnavailableException()
        {
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle(status: RecordStatus.Inactive);
            database.Seed(client, vehicle);

            await Assert.ThrowsAsync<VehicleUnavailableException>(() => CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 5)));
        }

        [Fact]
        public async Task CreateAsync_ClientWithOverdueContract_ThrowsClientUnavailableException()
        {
            var client = TestData.CreateClient();
            var otherVehicle = TestData.CreateVehicle("CD-34-EF");
            var vehicle = TestData.CreateVehicle("AB-12-CD");
            database.Seed(TestData.CreateContract(client, otherVehicle, -10, -2), vehicle);

            await Assert.ThrowsAsync<ClientUnavailableException>(() => CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 5)));
        }

        [Fact]
        public async Task CreateAsync_VehicleWithOverdueContract_ThrowsVehicleUnavailableException()
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, -10, -2), client);

            await Assert.ThrowsAsync<VehicleUnavailableException>(() => CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 5)));
        }

        [Theory]
        [InlineData(4, 6)]
        [InlineData(1, 9)]
        [InlineData(7, 9)]
        [InlineData(1, 3)]
        public async Task CreateAsync_OverlappingPeriod_ThrowsVehicleUnavailableException(int startInDays, int endInDays)
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, 3, 7), client);

            await Assert.ThrowsAsync<VehicleUnavailableException>(() =>
                CreateService().CreateAsync(CreateRequest(client, vehicle, startInDays, endInDays)));
        }

        [Fact]
        public async Task CreateAsync_StartingDayAfterPreviousEnd_Succeeds()
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, 1, 3), client);

            var contract = await CreateService().CreateAsync(CreateRequest(client, vehicle, 4, 6));

            Assert.Equal(ContractStatus.Scheduled, contract.Status);
        }

        [Fact]
        public async Task CreateAsync_StartingDayAfterEarlyReturn_Succeeds()
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, -5, 5, returnedInDays: -1, endMileage: 1500), client);

            var contract = await CreateService().CreateAsync(CreateRequest(client, vehicle, 0, 3, startMileage: 1500));

            Assert.Equal(ContractStatus.Active, contract.Status);
        }

        [Fact]
        public async Task CreateAsync_StartingOnReturnDay_ThrowsVehicleUnavailableException()
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, -5, 5, returnedInDays: 0, endMileage: 1500), client);

            await Assert.ThrowsAsync<VehicleUnavailableException>(() =>
                CreateService().CreateAsync(CreateRequest(client, vehicle, 0, 3, startMileage: 1500)));
        }

        [Fact]
        public async Task CreateAsync_OverlappingCancelledContract_Succeeds()
        {
            var otherClient = TestData.CreateClient("outro@cliente.com", "L-7654321");
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(otherClient, vehicle, 1, 5, cancelled: true), client);

            var contract = await CreateService().CreateAsync(CreateRequest(client, vehicle, 2, 4));

            Assert.Equal(ContractStatus.Scheduled, contract.Status);
        }

        [Fact]
        public async Task CreateAsync_StartMileageBelowLastRecorded_ThrowsInvalidMileageException()
        {
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(client, vehicle, -10, -5, startMileage: 14000, returnedInDays: -5, endMileage: 15000));

            await Assert.ThrowsAsync<InvalidMileageException>(() =>
                CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 3, startMileage: 14999)));
        }

        [Fact]
        public async Task CreateAsync_StartMileageEqualToLastRecorded_Succeeds()
        {
            var client = TestData.CreateClient();
            var vehicle = TestData.CreateVehicle();
            database.Seed(TestData.CreateContract(client, vehicle, -10, -5, startMileage: 14000, returnedInDays: -5, endMileage: 15000));

            var contract = await CreateService().CreateAsync(CreateRequest(client, vehicle, 1, 3, startMileage: 15000));

            Assert.Equal(15000, contract.StartMileage);
        }

        [Fact]
        public async Task ReturnAsync_ValidReturn_SetsStatusCompleted()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -3, 2);
            database.Seed(contract);

            var returned = await CreateService().ReturnAsync(contract.ContractId, new ReturnContractDTO { ReturnedAt = TestData.Today, EndMileage = 1200 });

            Assert.Equal(ContractStatus.Completed, returned.Status);
        }

        [Fact]
        public async Task ReturnAsync_CancelledContract_ThrowsContractStatusConflictException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), 1, 3, cancelled: true);
            database.Seed(contract);

            await Assert.ThrowsAsync<ContractStatusConflictException>(() =>
                CreateService().ReturnAsync(contract.ContractId, new ReturnContractDTO { ReturnedAt = TestData.Today, EndMileage = 1200 }));
        }

        [Fact]
        public async Task ReturnAsync_AlreadyReturned_ThrowsContractStatusConflictException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -5, -1, returnedInDays: -1, endMileage: 1200);
            database.Seed(contract);

            await Assert.ThrowsAsync<ContractStatusConflictException>(() =>
                CreateService().ReturnAsync(contract.ContractId, new ReturnContractDTO { ReturnedAt = TestData.Today, EndMileage = 1300 }));
        }

        [Fact]
        public async Task ReturnAsync_ReturnedBeforeStartDate_ThrowsInvalidReturnDateException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -3, 2);
            database.Seed(contract);

            await Assert.ThrowsAsync<InvalidReturnDateException>(() =>
                CreateService().ReturnAsync(contract.ContractId, new ReturnContractDTO { ReturnedAt = TestData.Today.AddDays(-5), EndMileage = 1200 }));
        }

        [Fact]
        public async Task ReturnAsync_EndMileageBelowStartMileage_ThrowsInvalidMileageException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -3, 2, startMileage: 1000);
            database.Seed(contract);

            await Assert.ThrowsAsync<InvalidMileageException>(() =>
                CreateService().ReturnAsync(contract.ContractId, new ReturnContractDTO { ReturnedAt = TestData.Today, EndMileage = 999 }));
        }

        [Fact]
        public async Task CancelAsync_ScheduledContract_SetsStatusCancelled()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), 2, 5);
            database.Seed(contract);

            var cancelled = await CreateService().CancelAsync(contract.ContractId);

            Assert.Equal(ContractStatus.Cancelled, cancelled.Status);
        }

        [Fact]
        public async Task CancelAsync_OnStartDay_SetsStatusCancelled()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), 0, 3);
            database.Seed(contract);

            var cancelled = await CreateService().CancelAsync(contract.ContractId);

            Assert.Equal(ContractStatus.Cancelled, cancelled.Status);
        }

        [Fact]
        public async Task CancelAsync_AfterStartDay_ThrowsContractStatusConflictException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -1, 3);
            database.Seed(contract);

            await Assert.ThrowsAsync<ContractStatusConflictException>(() => CreateService().CancelAsync(contract.ContractId));
        }

        [Fact]
        public async Task CancelAsync_ReturnedContract_ThrowsContractStatusConflictException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), -5, -1, returnedInDays: -1, endMileage: 1200);
            database.Seed(contract);

            await Assert.ThrowsAsync<ContractStatusConflictException>(() => CreateService().CancelAsync(contract.ContractId));
        }

        [Fact]
        public async Task CancelAsync_AlreadyCancelled_ThrowsContractStatusConflictException()
        {
            var contract = TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), 2, 5, cancelled: true);
            database.Seed(contract);

            await Assert.ThrowsAsync<ContractStatusConflictException>(() => CreateService().CancelAsync(contract.ContractId));
        }

        [Fact]
        public async Task GetAllAsync_FilterByStatus_ReturnsOnlyMatchingContracts()
        {
            var client = TestData.CreateClient();
            var overdue = TestData.CreateContract(client, TestData.CreateVehicle("AB-12-CD"), -10, -2);
            database.Seed(TestData.CreateContract(client, TestData.CreateVehicle("CD-34-EF"), 2, 5), overdue);

            var contracts = await CreateService().GetAllAsync(new ContractFilterDTO { Status = ContractStatus.Overdue });

            Assert.Equal(overdue.ContractId, Assert.Single(contracts).ContractId);
        }
    }
}
