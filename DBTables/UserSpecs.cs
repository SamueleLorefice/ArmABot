﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ArmABot.DBTables {
	public class UserSpecs {
		public int Id { get; set; }
		public User User { get; set; }
		public Specialization Specialization { get; set; }

	}
}
