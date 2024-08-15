using leaderboard.Models;
using System.Collections.Concurrent;

namespace leaderboard.Services
{
    public class LeaderBoardService : ILeaderBoardService
    {
        private readonly ConcurrentDictionary<int, Customer> _customers = new ConcurrentDictionary<int, Customer>();
        private readonly SortedDictionary<decimal, SortedSet<int>> _scores = new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        #region Get neighbor customers by specified customer id and range of rank
        public async Task<IEnumerable<Customer>> GetNeighborCustomers(int customerId, int high, int low)
        {
            _lock.EnterReadLock();
            try
            {
                if (!_customers.TryGetValue(customerId, out var customer))
                    return Enumerable.Empty<Customer>();
                int currentCustomerRank = GetCustomerRank(customerId);
                int startRank = Math.Max(1, currentCustomerRank - high);
                int endRank = currentCustomerRank + low;
                var neighborScores = _scores.Skip(startRank - 1).Take(endRank - startRank + 1);
                var customers = GetCustomers(startRank, neighborScores);
                return await Task.FromResult(customers);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        private int GetCustomerRank(int customerId)
        {
            return _scores.Values.SelectMany(x => x).ToList().IndexOf(customerId) + 1;
        }
        private List<Customer> GetCustomers(int startRank, IEnumerable<KeyValuePair<decimal, SortedSet<int>>> neighborScores)
        {
            return neighborScores.SelectMany(kvp => kvp.Value.Select(id => new Customer(id, score: kvp.Key) { Rank = startRank++ })).ToList();
        }
        #endregion

        #region Get customer leaderboards by range of rank
        public async Task<IEnumerable<Customer>> GetLeaderboards(int start, int end)
        {
            _lock.EnterReadLock();
            try
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
            finally
            {
                _lock.ExitReadLock();
            }
        }
        #endregion

        #region Update Score
        public async Task<decimal> UpdateScore(int customerId, decimal changeScore)
        {
            _lock.EnterWriteLock();
            try
            {
                var customer = _customers.GetOrAdd(customerId, id => new Customer(id, score: decimal.Zero));
                var oldScore = customer.Score;
                customer.CalculateScore(changeScore);
                var newScore = customer.Score;
                RecalculateLeaderboard(customerId, oldScore, newScore);
                _scores[customer.Score].Add(customerId);
                return await Task.FromResult(customer.Score);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void RecalculateLeaderboard(int customerId, decimal oldScore, decimal newScore)
        {
            RemoveOldCustomerScore(customerId, oldScore);
            AddNewCustomerScore(customerId, newScore);
        }
        private void RemoveOldCustomerScore(int customerId, decimal oldScore)
        {
            if (_scores.ContainsKey(oldScore))
            {
                _scores[oldScore].Remove(customerId);
                if (!_scores[oldScore].Any())
                {
                    _scores.Remove(oldScore);
                }
            }
        }
        private void AddNewCustomerScore(int customerId, decimal newScore)
        {
            if (!_scores.ContainsKey(newScore))
            {
                _scores[newScore] = new SortedSet<int>();
            }
            _scores[newScore].Add(customerId);
        }
        #endregion
    }
}
