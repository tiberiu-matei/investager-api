using Investager.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Investager.Core.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioDto> GetById(int userId, int portfolioId);

        Task<IEnumerable<PortfolioDto>> GetAll(int userId);

        Task<PortfolioDto> CreatePortfolio(int userId, UpdatePortfolioDto updatePortfolioDto);

        Task UpdatePortfolio(int userId, int portfolioId, UpdatePortfolioDto updatePortfolioDto);

        Task DeletePortfolio(int userId, int portfolioId);
    }
}
