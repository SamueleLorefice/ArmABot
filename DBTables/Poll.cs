using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArmA_Bot.DBTables {

    public class Poll {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PollId { get; set; }

        public long MessageId { get; set; }
        public ulong UserId { get; set; } //Creator
        public ulong GroupId { get; set; }
        public string Title { get; set; } //Poll title
        public DateTime EventDate { get; set; }
        public int EventQuota { get; set; }
    }
}