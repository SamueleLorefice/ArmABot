using ArmABot.DBTables;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmABot {

	public class DBManager : DbContext, IDatabaseAccess {
		public DbSet<Admin> AdminTable { get; set; }
		public DbSet<Poll> PollTable { get; set; }
		public DbSet<Vote> VoteTable { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			//MSSQL = "Server=(localdb)\MSSQLLocalDB;Database=ArmAHelperBot;Trusted_Connection=True;"
			//MySQL/MAriaDB = "Server=localhost;Database=ArmABot;Uid=root;Pwd=root;"
			optionsBuilder.UseMySql(Program.ConnectionString);
		}

		public bool TestConnection() {
			return Database.CanConnect();
		}

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
	}
}