using API.Models;
using API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedEssentials(AlugerVeiculosContext context, IConfiguration config)
        {
            if (await context.Users.AnyAsync()) return;

            var managerEmail = config["Manager:Email"];
            var managerPassword = config["Manager:Password"];

            if (string.IsNullOrWhiteSpace(managerEmail) || string.IsNullOrWhiteSpace(managerPassword))
                throw new InvalidOperationException("Manager Email and Manager Password must be configured to create the first Manager.");

            var manager = new User
            {
                Name = config["Manager:Name"] ?? "Manager",
                Email = managerEmail.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(managerPassword, 12),
                Role = Role.Manager,
                Status = RecordStatus.Active
            };

            await context.Users.AddAsync(manager);
            await context.SaveChangesAsync();
        }

        public static async Task SeedTestData(AlugerVeiculosContext context, IHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException($"SeedTestData refused — environment '{environment.EnvironmentName}' is not Development!");

            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
            context.ChangeTracker.Clear();

            Console.WriteLine("A seed da base de dados de teste está a ser executada...");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Strong123@", 12);

            var manager = new User { Name = "Marta Sousa", Email = "manager@test.com", PasswordHash = passwordHash, Role = Role.Manager, Status = RecordStatus.Active };
            var employee = new User { Name = "Rui Martins", Email = "employee@test.com", PasswordHash = passwordHash, Role = Role.Employee, Status = RecordStatus.Active };
            var inactiveEmployee = new User { Name = "Carla Nunes", Email = "inactive@test.com", PasswordHash = passwordHash, Role = Role.Employee, Status = RecordStatus.Inactive };

            await context.Users.AddRangeAsync(manager, employee, inactiveEmployee);

            var clio = new Vehicle { Brand = "Renault", Model = "Clio", LicensePlate = "AB-12-CD", Year = 2021, FuelType = FuelType.Gasoline };
            var peugeot = new Vehicle { Brand = "Peugeot", Model = "208", LicensePlate = "45-XZ-12", Year = 2019, FuelType = FuelType.Diesel };
            var golf = new Vehicle { Brand = "Volkswagen", Model = "Golf", LicensePlate = "12-34-AB", Year = 2003, FuelType = FuelType.Gasoline };
            var tesla = new Vehicle { Brand = "Tesla", Model = "Model 3", LicensePlate = "BC-34-DE", Year = 2023, FuelType = FuelType.Electric };
            var corolla = new Vehicle { Brand = "Toyota", Model = "Corolla", LicensePlate = "CD-56-EF", Year = 2022, FuelType = FuelType.Hybrid };
            var ibiza = new Vehicle { Brand = "Seat", Model = "Ibiza", LicensePlate = "67-QR-89", Year = 2015, FuelType = FuelType.Diesel };
            var punto = new Vehicle { Brand = "Fiat", Model = "Punto", LicensePlate = "98-76-ZY", Year = 2000, FuelType = FuelType.Gasoline };
            var corsa = new Vehicle { Brand = "Opel", Model = "Corsa", LicensePlate = "23-TT-45", Year = 2010, FuelType = FuelType.Diesel, Status = RecordStatus.Inactive };

            await context.Vehicles.AddRangeAsync(clio, peugeot, golf, tesla, corolla, ibiza, punto, corsa);

            var joao = new Client { FullName = "João Oliveira", Email = "joao.oliveira@cliente.com", Phone = "912345678", DriverLicenseNumber = "P-1234567" };
            var maria = new Client { FullName = "Maria Costa", Email = "maria.costa@cliente.com", Phone = "913456789", DriverLicenseNumber = "L-2345678" };
            var ana = new Client { FullName = "Ana Ferreira", Email = "ana.ferreira@cliente.com", Phone = "224567890", DriverLicenseNumber = "P-3456789" };
            var tiago = new Client { FullName = "Tiago Alves", Email = "tiago.alves@cliente.com", Phone = "916789012", DriverLicenseNumber = "C-4567890" };
            var sofia = new Client { FullName = "Sofia Lopes", Email = "sofia.lopes@cliente.com", Phone = "917890123", DriverLicenseNumber = "P-5678901" };
            var pedro = new Client { FullName = "Pedro Santos", Email = "pedro.santos@cliente.com", Phone = "918901234", DriverLicenseNumber = "L-6789012", Status = RecordStatus.Inactive };

            await context.Clients.AddRangeAsync(joao, maria, ana, tiago, sofia, pedro);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var completed = new Contract { DailyRate = 45, CreatedByUser = manager, Client = tiago, Vehicle = clio, StartDate = today.AddDays(-20), EndDate = today.AddDays(-15), StartMileage = 14000, EndMileage = 14800, ReturnedAt = today.AddDays(-15) };
            var completedEarly = new Contract { DailyRate = 95, CreatedByUser = employee, Client = joao, Vehicle = tesla, StartDate = today.AddDays(-12), EndDate = today.AddDays(-5), StartMileage = 5000, EndMileage = 5600, ReturnedAt = today.AddDays(-8) };
            var scheduled = new Contract { DailyRate = 45, CreatedByUser = employee, Client = joao, Vehicle = clio, StartDate = today.AddDays(3), EndDate = today.AddDays(7), StartMileage = 14800 };
            var active = new Contract { DailyRate = 50, CreatedByUser = employee, Client = maria, Vehicle = peugeot, StartDate = today.AddDays(-2), EndDate = today.AddDays(3), StartMileage = 42000 };
            var overdue = new Contract { DailyRate = 35, CreatedByUser = manager, Client = ana, Vehicle = golf, StartDate = today.AddDays(-10), EndDate = today.AddDays(-2), StartMileage = 120000 };
            var cancelled = new Contract { DailyRate = 60, CreatedByUser = employee, Client = sofia, Vehicle = corolla, StartDate = today.AddDays(5), EndDate = today.AddDays(8), StartMileage = 8000, CancelledAt = DateTime.UtcNow.AddDays(-1) };

            await context.Contracts.AddRangeAsync(completed, completedEarly, scheduled, active, overdue, cancelled);
            await context.SaveChangesAsync();

            Console.WriteLine("Seed concluído com sucesso.");
            Console.WriteLine($"  Utilizadores: {await context.Users.CountAsync()}");
            Console.WriteLine($"  Veículos: {await context.Vehicles.CountAsync()}");
            Console.WriteLine($"  Clientes: {await context.Clients.CountAsync()}");
            Console.WriteLine($"  Contratos: {await context.Contracts.CountAsync()}");
        }
    }
}
