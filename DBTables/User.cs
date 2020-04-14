using System.Collections.Generic;

namespace ArmABot.DBTables {

	public class User {
		public int Id { get; set; }
		public string Name { get; set; }
		public long TelegramId { get; set; }
		public Grade Grade { get; set; }
		public List<Specialization> Specializations { get; set; }
	}
}