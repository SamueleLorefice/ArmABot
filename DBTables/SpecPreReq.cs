namespace ArmABot.DBTables {

	public class SpecPreReq {
		public int Id { get; set; }
		public Specialization Specialization { get; set; }
		public Specialization RequisiteSpec { get; set; }
	}
}