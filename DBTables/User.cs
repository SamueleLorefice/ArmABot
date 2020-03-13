using System;
using System.Collections.Generic;
using System.Text;

namespace ArmABot.DBTables {
	public class User {
		public int Id { get; set; }
		public string Name { get; set; }
		public long TelegramId { get; set; }
		public int GradeId { get; set; }
		public int[] Specializations { get; set; }
	}
}
