using System;

namespace ClerkBot.Models
{
    public interface IElasticContract
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
