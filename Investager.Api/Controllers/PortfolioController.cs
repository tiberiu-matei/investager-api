using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Investager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpGet("portfolioId")]
        public async Task<IActionResult> GetById([FromRoute] int portfolioId)
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            var portfolioDto = await _portfolioService.GetById(userId, portfolioId);

            return Ok(portfolioDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            var portfolioDtos = await _portfolioService.GetAll(userId);

            return Ok(portfolioDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            var portfolioDto = await _portfolioService.CreatePortfolio(userId, updatePortfolioDto);

            return Ok(portfolioDto);
        }

        [HttpPut("{portfolioId}")]
        public async Task<IActionResult> Update([FromRoute] int portfolioId, [FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            await _portfolioService.UpdatePortfolio(userId, portfolioId, updatePortfolioDto);

            return NoContent();
        }

        [HttpDelete("{portfolioId}")]
        public async Task<IActionResult> Delete([FromRoute] int portfolioId)
        {
            var userId = Convert.ToInt32(HttpContext.Items[HttpContextKeys.UserId]);
            await _portfolioService.DeletePortfolio(userId, portfolioId);

            return NoContent();
        }
    }
}
