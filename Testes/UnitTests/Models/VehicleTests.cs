using API.Models;
using UnitTests.Infrastructure;

namespace UnitTests.Models
{
    public class VehicleTests
    {
        private static Vehicle CreateVehicleWithContract(int startInDays, int endInDays, int? returnedInDays = null, bool cancelled = false)
        {
            var vehicle = TestData.CreateVehicle();
            vehicle.Contracts.Add(TestData.CreateContract(TestData.CreateClient(), vehicle, startInDays, endInDays,
                returnedInDays: returnedInDays, cancelled: cancelled));

            return vehicle;
        }

        [Fact]
        public void IsRentedOn_ContractStartingToday_ReturnsTrue()
        {
            var vehicle = CreateVehicleWithContract(0, 4);

            Assert.True(vehicle.IsRentedOn(TestData.Today));
        }

        [Fact]
        public void IsRentedOn_OverdueContract_ReturnsTrue()
        {
            var vehicle = CreateVehicleWithContract(-10, -2);

            Assert.True(vehicle.IsRentedOn(TestData.Today));
        }

        [Fact]
        public void IsRentedOn_CancelledContract_ReturnsFalse()
        {
            var vehicle = CreateVehicleWithContract(-1, 3, cancelled: true);

            Assert.False(vehicle.IsRentedOn(TestData.Today));
        }

        [Fact]
        public void IsRentedOn_ContractReturnedEarly_ReturnsFalse()
        {
            var vehicle = CreateVehicleWithContract(-5, 3, returnedInDays: -1);

            Assert.False(vehicle.IsRentedOn(TestData.Today));
        }

        [Fact]
        public void GetLastMileage_VehicleWithoutContracts_ReturnsNull()
        {
            var vehicle = TestData.CreateVehicle();

            Assert.Null(vehicle.GetLastMileage());
        }

        [Fact]
        public void GetLastMileage_ReturnedContracts_ReturnsHighestEndMileage()
        {
            var vehicle = TestData.CreateVehicle();
            var client = TestData.CreateClient();
            vehicle.Contracts.Add(TestData.CreateContract(client, vehicle, -20, -15, returnedInDays: -15, endMileage: 14800));
            vehicle.Contracts.Add(TestData.CreateContract(client, vehicle, -10, -5, returnedInDays: -5, endMileage: 15300));
            vehicle.Contracts.Add(TestData.CreateContract(client, vehicle, 2, 5));

            Assert.Equal(15300, vehicle.GetLastMileage());
        }
    }
}
