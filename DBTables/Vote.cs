using System.ComponentModel.DataAnnotations.Schema;

namespace ArmA_Bot.DBTables {

    public class Vote {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong UserId { get; set; }
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