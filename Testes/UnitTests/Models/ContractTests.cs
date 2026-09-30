using API.Models;
using API.Models.Enums;
using UnitTests.Infrastructure;

namespace UnitTests.Models
{
    public class ContractTests
    {
        private static readonly DateOnly Today = TestData.Today;

        private static Contract CreateContract(int startInDays, int endInDays, int? returnedInDays = null, bool cancelled = false)
        {
            return TestData.CreateContract(TestData.CreateClient(), TestData.CreateVehicle(), startInDays, endInDays,
                returnedInDays: returnedInDays, cancelled: cancelled);
        }

        [Fact]
        public void GetStatusOn_StartDateInFuture_ReturnsScheduled()
        {
            var contract = CreateContract(3, 7);

            Assert.Equal(ContractStatus.Scheduled, contract.GetStatusOn(Today));
        }

        [Fact]
        public void GetStatusOn_BetweenStartAndEndDate_ReturnsActive()
        {
            var contract = CreateContract(-2, 3);

            Assert.Equal(ContractStatus.Active, contract.GetStatusOn(Today));
        }

        [Fact]
        public void GetStatusOn_EndDateIsToday_ReturnsActive()
        {
            var contract = CreateContract(-3, 0);

            Assert.Equal(ContractStatus.Active, contract.GetStatusOn(Today));
        }

        [Fact]
        public void GetStatusOn_EndDatePassedWithoutReturn_ReturnsOverdue()
        {
            var contract = CreateContract(-10, -2);

            Assert.Equal(ContractStatus.Overdue, contract.GetStatusOn(Today));
        }

        [Fact]
        public void GetStatusOn_Returned_ReturnsCompleted()
        {
            var contract = CreateContract(-12, -5, returnedInDays: -8);

            Assert.Equal(ContractStatus.Completed, contract.GetStatusOn(Today));
        }

        [Fact]
        public void GetStatusOn_Cancelled_ReturnsCancelled()
        {
            var contract = CreateContract(5, 8, cancelled: true);

            Assert.Equal(ContractStatus.Cancelled, contract.GetStatusOn(Today));
        }
    }
}
