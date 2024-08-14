using System.ComponentModel.DataAnnotations;

namespace leaderboard.Models
{
    public class Customer
    {
        public int CustomerId { get; private set; }
        [Range(-1000,1000)]
        public decimal Score { get; private set; }
        public int Rank { get;  set; }
        public Customer(int customerId, decimal score)
        {
            CustomerId = customerId;
            Score = score;
        }
        public void CalculateScore(decimal changeScore)
        {
            Score += changeScore;
        }
    }
}
