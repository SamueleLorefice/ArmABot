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
				AdminTable.Add(admin);
				SaveChanges();
			} catch (MySqlException e) when (e.Message == "Field 'Id' doesn't have a default value") {
				Console.WriteLine("Write to the database is not possible, disable Strict SQL mode");
			}
		}

		public int AddPoll(Poll poll) {
			PollTable.Add(poll);
			SaveChanges();
			return poll.Id;
		}

		public void AddVote(EVote choice, int pollId, long userId, string username) {
			var vote = new Vote { Choice = choice, UserId = userId, Id = pollId, Username = username };
			VoteTable.Add(vote);
			SaveChanges();
		}

		public Admin FindAdmin(long userId, long ChatId) {
			return AdminTable.Where(x => x.UserId == userId && x.GroupId == ChatId).FirstOrDefault();
		}

		public Poll GetPoll(int pollId) {
			return PollTable.Where(x => x.Id == pollId).FirstOrDefault();
		}

		public IEnumerable<Poll> GetPollsBy(long userId, long groupId) {
			return PollTable.Where(x => x.UserId == userId && x.GroupId == groupId);
		}

		public IEnumerable<Vote> GetVotesInPoll(int pollId) {
			return VoteTable.Where(x => x.Id == pollId);
		}

		public IEnumerable<Vote> GetVotesInPollFrom(long userId, int pollId) {
			return VoteTable.Where(x => x.Id == pollId && x.UserId == userId);
		}

		public void EditVote(int voteId, EVote choice) {
			Vote edit = VoteTable.Find(voteId);
			edit.Choice = choice;
			SaveChanges();
		}

		public void UpdatePollMessageId(int pollId, long messageId) {
			Poll edit = PollTable.Find(pollId);
			if (edit != null) {
				edit.MessageId = messageId;
			} else {
				throw new NullReferenceException("Poll could not be found");
			}

			SaveChanges();
		}

		public void RemoveAdmin(Admin admin) {
			AdminTable.Remove(admin);
			SaveChanges();
		}

		public void RemoveAdmin(long Id) {
			var admin = AdminTable.Where(x => x.UserId == Id).ToList();
			AdminTable.RemoveRange(admin);
			SaveChanges();
		}

		public void RemovePoll(int pollId) {
			var polls = PollTable.Where(x => x.Id == pollId).ToList();
			PollTable.RemoveRange(polls);
			var votes = VoteTable.Where(x => x.Id == pollId).ToList();
			VoteTable.RemoveRange(votes);
			SaveChanges();
		}

		public void RemovePoll(Poll poll) {
			PollTable.Remove(poll);
			var votes = VoteTable.Where(x => x.Id == poll.Id).ToList();
			VoteTable.RemoveRange(votes);
			SaveChanges();
		}

		public void RemoveVote(int id) {
			Vote vote = VoteTable.Where(x => x.Id == id).First();
			VoteTable.Remove(vote);
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
			throw new NotImplementedException();
		}

		public void RemoveSpecialization(int id) {
			throw new NotImplementedException();
		}

		//User Section
		public void AddUser(string name, long telegramId, int gradeId = 0) {
			var user = new User {
				Name = name,
				TelegramId = telegramId,
				GradeId = gradeId
			};
			UserTable.Add(user);
		}

		public IEnumerable<User> GetUsers() {
			return UserTable.ToList<User>();
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
			var usr = UserTable.Find(id);
			UserTable.Remove(usr);
		}

		public void AddGrade(Grade grade) {
			throw new NotImplementedException();
		}

		public IEnumerable<Grade> GetGrades() {
			throw new NotImplementedException();
		}

		#endregion Specializations User Grades
	}
}