using System;
using System.Collections.Generic;
using System.Text;

namespace ArmA_Bot.DBTables {
	public class Specializations {
		public int Id { get; set; }
		public string SpecializationName { get; set; }
		public int GradeRequirementId { get; set; }
		public int PreRequisiteSpecializationId { get; set; }
	}
}
