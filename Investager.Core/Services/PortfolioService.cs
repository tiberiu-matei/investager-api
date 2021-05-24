using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Investager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Investager.Core.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly ICoreUnitOfWork _coreUnitOfWork;

        public PortfolioService(ICoreUnitOfWork coreUnitOfWork)
        {
            _coreUnitOfWork = coreUnitOfWork;
        }

        public async Task<PortfolioDto> GetById(int userId, int portfolioId)
        {
            var portfolios = await _coreUnitOfWork.Portfolios.Find(e => e.UserId == userId && e.Id == portfolioId, nameof(Portfolio.PortfolioAssets));
            var portfolio = portfolios.Single();

            return Map(portfolio);
        }

        public async Task<IEnumerable<PortfolioDto>> GetAll(int userId)
        {
            var portfolios = await _coreUnitOfWork.Portfolios.Find(e => e.UserId == userId, nameof(Portfolio.PortfolioAssets));

            return portfolios.Select(e => Map(e)).ToList();
        }

        public async Task<PortfolioDto> Create(int userId, UpdatePortfolioDto updatePortfolioDto)
        {
            var portfolio = new Portfolio
            {
                Name = updatePortfolioDto.Name,
                UserId = userId,
                PortfolioAssets = updatePortfolioDto.AssetIds.Select(e => new PortfolioAsset { AssetId = e }).ToList(),
            };

            _coreUnitOfWork.Portfolios.Insert(portfolio);

            await _coreUnitOfWork.SaveChanges();

            return Map(portfolio);
        }

        public async Task Update(int userId, int portfolioId, UpdatePortfolioDto updatePortfolioDto)
        {
            var portfolios = await _coreUnitOfWork.Portfolios.Find(e => e.Id == portfolioId && e.UserId == userId);
            var portfolio = portfolios.Single();

            portfolio.Name = updatePortfolioDto.Name;
            portfolio.PortfolioAssets = updatePortfolioDto.AssetIds.Select(e => new PortfolioAsset { AssetId = e }).ToList();

            await _coreUnitOfWork.SaveChanges();
        }

        public async Task Delete(int userId, int portfolioId)
        {
            var portfolio = await _coreUnitOfWork.Portfolios.GetByIdWithTracking(portfolioId);
            if (portfolio.UserId != userId)
            {
                throw new InvalidOperationException("Cannot delete a portfolio for another user.");
            }

            _coreUnitOfWork.Portfolios.Delete(portfolio);
            await _coreUnitOfWork.SaveChanges();
        }

        private PortfolioDto Map(Portfolio portfolio)
        {
            return new PortfolioDto
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                AssetIds = portfolio.PortfolioAssets.Select(e => e.AssetId).ToList(),
            };
        }
    }
}
