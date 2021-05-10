namespace Investager.Core.Services
{
    public interface IDataCollectionServiceFactory
    {
        IDataCollectionService GetService(string provider);
    }
}
