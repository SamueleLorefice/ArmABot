﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArmABot.DBTables {
	public class Specialization {
		public int Id { get; set; }
		public string SpecializationName { get; set; }
		public int GradeRequirementId { get; set; }
		public int PreRequisiteSpecializationId { get; set; }
	}
}
