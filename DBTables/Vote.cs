using System.ComponentModel.DataAnnotations.Schema;

namespace ArmA_Bot.DBTables {

    public class Vote {

        public int Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public EVote Choice { get; set; }
        public int PollId { get; set; }
    }
}

namespace ArmA_Bot {

    public enum EVote {
        Present = 1,
        Absent = 2,
        Maybe = 3
    }
}