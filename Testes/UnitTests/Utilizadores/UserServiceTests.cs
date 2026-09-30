using API.Models;
using API.Models.Enums;
using API.Utilizadores.DTOs;
using API.Utilizadores.Exceptions;
using API.Utilizadores.Services;
using UnitTests.Infrastructure;

namespace UnitTests.Utilizadores
{
    public class UserServiceTests : IDisposable
    {
        private readonly TestDatabase database = new();

        private UserService CreateService() => new(database.CreateContext());

        private static CreateUserDTO CreateRequest(string email = "rui@test.com")
        {
            return new CreateUserDTO
            {
                Name = "Rui Martins",
                Email = email,
                Password = TestData.Password,
                ConfirmPassword = TestData.Password,
                Role = Role.Employee
            };
        }

        public void Dispose() => database.Dispose();

        [Fact]
        public async Task CreateAsync_DuplicateEmail_ThrowsEmailAlreadyExistsException()
        {
            database.Seed(TestData.CreateUser("rui@test.com"));

            await Assert.ThrowsAsync<EmailAlreadyExistsException>(() => CreateService().CreateAsync(CreateRequest("rui@test.com")));
        }

        [Fact]
        public async Task CreateAsync_EmailWithUppercase_StoresLowercase()
        {
            var user = await CreateService().CreateAsync(CreateRequest("Rui@Test.com"));

            Assert.Equal("rui@test.com", user.Email);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_StoresHashedPassword()
        {
            var user = await CreateService().CreateAsync(CreateRequest());

            using var context = database.CreateContext();
            var storedHash = context.Users.Single(u => u.UserId == user.UserId).PasswordHash;

            Assert.NotEqual(TestData.Password, storedHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(TestData.Password, storedHash));
        }

        [Fact]
        public async Task DeactivateAsync_OwnAccount_ThrowsSelfDeactivationException()
        {
            var manager = TestData.CreateUser();
            database.Seed(manager);

            await Assert.ThrowsAsync<SelfDeactivationException>(() => CreateService().DeactivateAsync(manager.UserId, manager.UserId));
        }

        [Fact]
        public async Task DeactivateAsync_UserWithActiveSessions_RevokesRefreshTokens()
        {
            var manager = TestData.CreateUser();
            var employee = TestData.CreateUser("rui@test.com", Role.Employee);
            employee.RefreshTokens.Add(new RefreshToken { TokenHash = "hash-1", ExpiresAt = DateTime.UtcNow.AddDays(7) });
            employee.RefreshTokens.Add(new RefreshToken { TokenHash = "hash-2", ExpiresAt = DateTime.UtcNow.AddDays(7) });
            database.Seed(manager, employee);

            await CreateService().DeactivateAsync(employee.UserId, manager.UserId);

            using var context = database.CreateContext();
            Assert.All(context.RefreshTokens.Where(rt => rt.UserId == employee.UserId), rt => Assert.NotNull(rt.RevokedAt));
            Assert.Equal(RecordStatus.Inactive, context.Users.Single(u => u.UserId == employee.UserId).Status);
        }

        [Fact]
        public async Task DeactivateAsync_AlreadyInactive_ThrowsUserStatusConflictException()
        {
            var manager = TestData.CreateUser();
            var employee = TestData.CreateUser("rui@test.com", Role.Employee, RecordStatus.Inactive);
            database.Seed(manager, employee);

            await Assert.ThrowsAsync<UserStatusConflictException>(() => CreateService().DeactivateAsync(employee.UserId, manager.UserId));
        }

        [Fact]
        public async Task ReactivateAsync_AlreadyActive_ThrowsUserStatusConflictException()
        {
            var employee = TestData.CreateUser("rui@test.com", Role.Employee);
            database.Seed(employee);

            await Assert.ThrowsAsync<UserStatusConflictException>(() => CreateService().ReactivateAsync(employee.UserId));
        }
    }
}
