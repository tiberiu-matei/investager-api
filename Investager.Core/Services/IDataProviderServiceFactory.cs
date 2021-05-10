using Investager.Core.Interfaces;

namespace Investager.Core.Services
{
    public interface IDataProviderServiceFactory
    {
        IDataProviderService CreateService(string provider);
    }
}
