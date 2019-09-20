using System.ComponentModel.DataAnnotations.Schema;

namespace ArmA_Bot.DBTables {

    public class Admin {

        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GroupId { get; set; }
    }
}