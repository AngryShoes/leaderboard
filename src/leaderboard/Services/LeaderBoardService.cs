using leaderboard.Models;
using System.Collections.Concurrent;

namespace leaderboard.Services
{
    public class LeaderBoardService : ILeaderBoardService
    {
        private readonly ConcurrentDictionary<int, Customer> _customers = new ConcurrentDictionary<int, Customer>();
        private readonly SortedDictionary<decimal, SortedSet<int>> _scores = new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));
        public async Task<IEnumerable<Customer>> GetNeighborCustomers(int customerId, int high, int low)
        {
            var result = new List<Customer>();
            if (!_customers.TryGetValue(customerId, out var customer))
                return result;
            var currentCustomerRank = _scores.Values.SelectMany(x => x).ToList().IndexOf(customerId) + 1;
            int startRank = Math.Max(1, currentCustomerRank - high);
            int endRank = currentCustomerRank + low;
            var neighborScores = _scores.Skip(startRank - 1).Take(endRank - startRank + 1);
            foreach (var kvp in neighborScores)
            {
                foreach (var id in kvp.Value)
                {
                    result.Add(new Customer(id, score: kvp.Key) { Rank = startRank });
                    startRank++;
                }
            }
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<Customer>> GetLeaderboards(int start, int end)
        {
            var result = new List<Customer>();
            int currentRank = 1;
            foreach (var kvp in _scores)
            {
                foreach (var customerId in kvp.Value)
                {
                    if (currentRank > end)
                        return result;
                    if (currentRank >= start && currentRank <= end)
                        result.Add(new Customer(customerId, score: kvp.Key) { Rank = currentRank });
                    currentRank++;
                }
            }
            return await Task.FromResult(result);
        }

        public async Task<decimal> UpdateScore(int customerId, decimal changeScore)
        {
            var customer = _customers.GetOrAdd(customerId, id => new Customer(id, score: decimal.Zero));
            var oldScore = customer.Score;
            customer.CalculateScore(changeScore);

            if (_scores.ContainsKey(oldScore))
            {
                _scores[oldScore].Remove(customerId);
                if (!_scores[oldScore].Any())
                    _scores.Remove(oldScore);
            }

            if (!_scores.ContainsKey(customer.Score))
                _scores[customer.Score] = new SortedSet<int>();

            _scores[customer.Score].Add(customerId);
            return await Task.FromResult(customer.Score);
        }
    }
}
