namespace ArmABot.DBTables {

    public class Vote {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public EVote Choice { get; set; }
        public int PollId { get; set; }
    }
}

namespace ArmABot {

    public enum EVote {
        Present = 1,
        Absent = 2,
        Maybe = 3
    }
}