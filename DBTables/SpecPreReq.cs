using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ArmABot.DBTables {
	public class SpecPreReq {
		public Specialization Specialization { get; set; }
		public Specialization RequisiteSpec { get; set; }
	}
}
