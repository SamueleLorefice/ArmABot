using System;

namespace ArmABot.DBTables {

	public class Poll {
		public int Id { get; set; }
		public long MessageId { get; set; }
		public long UserId { get; set; } //Creator
		public long GroupId { get; set; }
		public string Title { get; set; } //Poll title
		public DateTime EventDate { get; set; }
		public int EventQuota { get; set; }
	}
}