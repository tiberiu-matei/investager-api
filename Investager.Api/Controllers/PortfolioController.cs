using Investager.Core.Constants;
using Investager.Core.Dtos;
using Investager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{portfolioId}")]
        public async Task<IActionResult> GetById([FromRoute] int portfolioId)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            var portfolioDto = await _portfolioService.GetById(userId, portfolioId);

            return Ok(portfolioDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            var portfolioDtos = await _portfolioService.GetAll(userId);

            return Ok(portfolioDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            var portfolioDto = await _portfolioService.Create(userId, updatePortfolioDto);

            return Ok(portfolioDto);
        }

        [HttpPut("{portfolioId}")]
        public async Task<IActionResult> Update([FromRoute] int portfolioId, [FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            await _portfolioService.Update(userId, portfolioId, updatePortfolioDto);

            return NoContent();
        }

        [HttpDelete("{portfolioId}")]
        public async Task<IActionResult> Delete([FromRoute] int portfolioId)
        {
            var userId = int.Parse(HttpContext.Items[HttpContextKeys.UserId] as string);
            await _portfolioService.Delete(userId, portfolioId);

            return NoContent();
        }
    }
}
