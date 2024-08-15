using leaderboard.Models;
using leaderboard.Services;
using leaderboard.Validators;
using Microsoft.AspNetCore.Mvc;

namespace leaderboard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaderBoardController : ControllerBase
    {
        private readonly ILeaderBoardService _leaderBoardService;

        public LeaderBoardController(ILeaderBoardService leaderBoardService)
        {
            _leaderBoardService = leaderBoardService;
        }
        /// <summary>
        /// update customer score
        /// </summary>
        /// <param name="customerid">customer id</param>
        /// <param name="score">customer score</param>
        /// <returns></returns>
        [HttpPost("customer/{customerid}/score/{score}")]
        public async Task<IActionResult> UpdateScore([FromRoute] int customerid, [FromRoute] decimal score)
        {
            var model = new UpdateCustomerScoreModel { CustomerId = customerid, Score = score };
            var validateResult = await new CustomerValidator().ValidateAsync(model);
            if (!validateResult.IsValid)
            {
                return BadRequest(validateResult.Errors.Select(x => x.ErrorMessage));
            }
            var result = await _leaderBoardService.UpdateScore(model.CustomerId, model.Score);
            return Ok(result);
        }

        /// <summary>
        /// Get customer leaderboards by rank
        /// </summary>
        /// <param name="start"> start rank </param>
        /// <param name="end"> end rank </param>
        /// <returns>customer list</returns>
        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboards(int start, int end)
        {
            var result = await _leaderBoardService.GetLeaderboards(start, end);
            return Ok(result);
        }

        /// <summary>
        /// Get neighbor customers by range of rank 
        /// </summary>
        /// <param name="customerid"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        [HttpGet("leaderboard/{customerid}")]
        public async Task<IActionResult> GetNeighborCustomers(int customerid, int high = 0, int low = 0)
        {
            var result = await _leaderBoardService.GetNeighborCustomers(customerid, high, low);
            return Ok(result);
        }
    }
}