namespace ArmABot.DBTables {

	public class UserGrade {
		public int Id { get; set; }
		public User User { get; set; }
		public Grade Grade { get; set; }
	}
}