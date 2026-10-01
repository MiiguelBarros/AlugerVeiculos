using API.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure
{
    public class TestDatabase : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly DbContextOptions<AlugerVeiculosContext> options;

        public TestDatabase()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            options = new DbContextOptionsBuilder<AlugerVeiculosContext>()
                .UseSqlite(connection)
                .Options;

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public AlugerVeiculosContext CreateContext() => new(options);

        public void Seed(params object[] entities)
        {
            using var context = CreateContext();
            context.AttachRange(entities);
            context.SaveChanges();
        }

        public void Dispose() => connection.Dispose();
    }
}
