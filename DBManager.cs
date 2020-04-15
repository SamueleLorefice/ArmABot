using ArmABot.DBTables;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmABot {

	public class DBManager : DbContext, IDatabaseAccess {
		public DbSet<Admin> AdminsTable { get; set; }
		public DbSet<User> UserTable { get; set; }
		public DbSet<Poll> PollsTable { get; set; }
		public DbSet<Vote> VotesTable { get; set; }
		public DbSet<Specialization> SpecializationsTable { get; set; }
		public DbSet<Grade> GradesTable { get; set; }
		public DbSet<SpecGradePreReq> SpecsGradePreReqTable { get; set; }
		public DbSet<SpecPreReq> SpecPreReqsTable { get; set; }
		public DbSet<UserSpecs> UserSpecsTable { get; set; }
		public DbSet<UserGrade> UserGradeTable { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			//MSSQL = "Server=(localdb)\MSSQLLocalDB;Database=ArmAHelperBot;Trusted_Connection=True;"
			//MySQL/MAriaDB = "Server=localhost;Database=ArmABot;Uid=root;Pwd=root;"
			optionsBuilder.UseMySql(Program.ConnectionString);
		}

		public bool TestConnection() {
			return Database.CanConnect();
		}

		#region Admin Poll Vote

		public void AddAdmin(Admin admin) {
			try {
				AdminsTable.Add(admin);
				SaveChanges();
			} catch (MySqlException e) when (e.Message == "Field 'Id' doesn't have a default value") {
				Console.WriteLine("Write to the database is not possible, disable Strict SQL mode");
			}
		}

		public int AddPoll(Poll poll) {
			PollsTable.Add(poll);
			SaveChanges();
			return poll.Id;
		}

		public void AddVote(EVote choice, int pollId, long userId, string username) {
			var vote = new Vote { Choice = choice, UserId = userId, Id = pollId, Username = username };
			VotesTable.Add(vote);
			SaveChanges();
		}

		public Admin FindAdmin(long userId, long ChatId) {
			return AdminsTable.Where(x => x.UserId == userId && x.GroupId == ChatId).FirstOrDefault();
		}

		public Poll GetPoll(int pollId) {
			return PollsTable.Where(x => x.Id == pollId).FirstOrDefault();
		}

		public IEnumerable<Poll> GetPollsBy(long userId, long groupId) {
			return PollsTable.Where(x => x.UserId == userId && x.GroupId == groupId);
		}

		public IEnumerable<Vote> GetVotesInPoll(int pollId) {
			return VotesTable.Where(x => x.Id == pollId);
		}

		public IEnumerable<Vote> GetVotesInPollFrom(long userId, int pollId) {
			return VotesTable.Where(x => x.Id == pollId && x.UserId == userId);
		}

		public void EditVote(int voteId, EVote choice) {
			Vote edit = VotesTable.Find(voteId);
			edit.Choice = choice;
			SaveChanges();
		}

		public void UpdatePollMessageId(int pollId, long messageId) {
			Poll edit = PollsTable.Find(pollId);
			if (edit != null) {
				edit.MessageId = messageId;
			} else {
				throw new NullReferenceException("Poll could not be found");
			}

			SaveChanges();
		}

		public void RemoveAdmin(Admin admin) {
			AdminsTable.Remove(admin);
			SaveChanges();
		}

		public void RemoveAdmin(long Id) {
			var admin = AdminsTable.Where(x => x.UserId == Id).ToList();
			AdminsTable.RemoveRange(admin);
			SaveChanges();
		}

		public void RemovePoll(int pollId) {
			var polls = PollsTable.Where(x => x.Id == pollId).ToList();
			PollsTable.RemoveRange(polls);
			var votes = VotesTable.Where(x => x.Id == pollId).ToList();
			VotesTable.RemoveRange(votes);
			SaveChanges();
		}

		public void RemovePoll(Poll poll) {
			PollsTable.Remove(poll);
			var votes = VotesTable.Where(x => x.Id == poll.Id).ToList();
			VotesTable.RemoveRange(votes);
			SaveChanges();
		}

		public void RemoveVote(int id) {
			Vote vote = VotesTable.Where(x => x.Id == id).First();
			VotesTable.Remove(vote);
			SaveChanges();
		}

		public override int SaveChanges() {
			try {
				base.SaveChanges();
			} catch (MySqlException) {
				return 1;
			}
			return 0;
		}

		#endregion Admin Poll Vote

		#region Specializations User Grades

		public void AddSpecialization(string name) {
			SpecializationsTable.Add(new Specialization { SpecializationName = name });
			SaveChanges();
		}

		public void AddPreRequisiteSpec(int specializationId, int preRequisiteId) {
			var row = new SpecPreReq {
				RequisiteSpec = SpecializationsTable.Find(preRequisiteId),
				Specialization = SpecializationsTable.Find(specializationId)
			};
			SpecPreReqsTable.Add(row);
			SaveChanges();
		}

		public IEnumerable<Specialization> GetSpecializations(string user) {
			return SpecializationsTable;
		}

		public void RemoveSpecialization(int id) {
			var spec = SpecializationsTable.Find(id);
			if (spec != null) {
				SpecializationsTable.Remove(spec);
				SaveChanges();
			}
		}

		//User Section
		public void AddUser(string name, long telegramId, Grade grade = null) {
			var user = new User {
				Name = name,
				TelegramId = telegramId,
				Grade = grade
			};
			UserTable.Add(user);
		}

		public IEnumerable<User> GetUsers() {
			return UserTable;
		}

		public User FindUserFromId(long id) {
			return UserTable.Find(id);
		}

		public User FindUserFromName(string name) {
			return UserTable.Where(x => x.Name == name).First();
		}

		public User FindUserFromTelegram(long telegramId) {
			return UserTable.Where(x => x.TelegramId == telegramId).First();
		}

		public void RemoveUser(User user) {
			UserTable.Remove(user);
			SaveChanges();
		}

		public void RemoveUser(long id) {
			User usr = UserTable.Find(id);
			UserTable.Remove(usr);
		}

		public void AddGrade(Grade grade) {
			GradesTable.Add(grade);
			SaveChanges();
		}

		public IEnumerable<Grade> GetGrades() {
			return GradesTable;
		}

		public void AssignUpgradeGrade(int userId, int gradeId) {
			UserGrade graderow = UserGradeTable.Where(x => x.User.Id == userId).First();

			if (graderow == null)
				UserGradeTable.Add(new UserGrade {
					User = UserTable.Where(x => x.Id == userId).First(),
					Grade = GradesTable.Where(x => x.Id == gradeId).First()
				});
			else
				graderow.Grade = GradesTable.Where(x => x.Id == gradeId)?.First();
			SaveChanges();
		}

		#endregion Specializations User Grades
	}
}