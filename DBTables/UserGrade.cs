using System;
using System.Collections.Generic;
using System.Text;

namespace ArmABot.DBTables {
	public class UserGrade {
		public int Id { get; set; }
		public User User { get; set; }
		public Grade Grade { get; set; }
	}
}
