using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioDto> GetById(int userId, int portfolioId);

        Task<IEnumerable<PortfolioDto>> GetAll(int userId);

        Task<PortfolioDto> Create(int userId, UpdatePortfolioDto updatePortfolioDto);

        Task Update(int userId, int portfolioId, UpdatePortfolioDto updatePortfolioDto);

        Task Delete(int userId, int portfolioId);
    }
}
