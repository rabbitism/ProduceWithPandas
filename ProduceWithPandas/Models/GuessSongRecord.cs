using System;

namespace ProduceWithPandas.Models
{
    public class DoubanPostCache
    {
        public DateTime LastUpdateDate { get; set; }
        public bool Initialized { get; set; } = false;
        public List<DoubanReply> Replies { get; set; } = new List<DoubanReply>();
    }

    public class DoubanReply
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime PublishTime { get; set; }
        public double Score { get; set; }
    }

}