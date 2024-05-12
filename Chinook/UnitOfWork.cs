using Microsoft.EntityFrameworkCore;

namespace Chinook
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ChinookContext _dbContext;

        public UnitOfWork(ChinookContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public ChinookContext GetDatabaseContext()
        {
            return _dbContext;
        }
    }
}
