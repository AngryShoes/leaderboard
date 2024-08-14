using leaderboard.Models;

namespace leaderboard.Services
{
    public interface ILeaderBoardService
    {
        Task<decimal> UpdateScore(int customerId, decimal changeScore);
        Task<IEnumerable<Customer>> GetLeaderboards(int start, int end);
        Task<IEnumerable<Customer>> GetNeighborCustomers(int customerId, int high, int low);
    }
}
