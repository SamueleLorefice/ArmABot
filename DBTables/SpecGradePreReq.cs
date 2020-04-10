namespace ArmABot.DBTables {

	public class SpecGradePreReq {
		public int Id { get; set; }
		public Specialization Specialization { get; set; }
		public Grade GradeRequired { get; set; }
	}
}