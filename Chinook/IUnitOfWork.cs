namespace Chinook
{
    public interface IUnitOfWork
    {
        void SaveChanges();
        Task SaveChangesAsync();
        ChinookContext GetDatabaseContext();
    }
}
