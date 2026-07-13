using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            using var context = new PassNestDbContext();
            context.Database.Migrate();
        }
    }
}
