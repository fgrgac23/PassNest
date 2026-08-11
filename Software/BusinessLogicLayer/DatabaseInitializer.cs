using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace BusinessLogicLayer
{
    [ExcludeFromCodeCoverage]
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            using var context = new PassNestDbContext();
            context.Database.Migrate();
        }
    }
}
