using System.Collections.Generic;
using System.Linq;
using ArmA_Bot.DBTables;
using Microsoft.EntityFrameworkCore;

namespace ArmA_Bot {

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
            AdminTable.Add(admin);
            SaveChanges();
        }

        public int AddPoll(Poll poll) {
            PollTable.Add(poll);
            SaveChanges();
            return poll.PollId;
        }

        public void AddVote(EVote choice, int pollId, ulong userId, string username) {
            var vote = new Vote { Choice = choice, UserId = userId, PollId = pollId, Username = username };
            VoteTable.Add(vote);
            SaveChanges();
        }

        public Admin FindAdmin(ulong userId, ulong ChatId) {
            return AdminTable.Where(x => x.UserId == userId && x.GroupId == ChatId).FirstOrDefault();
        }

        public Poll GetPoll(int pollId) {
            return PollTable.Where(x => x.PollId == pollId).FirstOrDefault();
        }

        public IEnumerable<Poll> GetPollsBy(ulong userId, ulong groupId) {
            return PollTable.Where(x => x.UserId == userId && x.GroupId == groupId);
        }

        public IEnumerable<Vote> GetVotesInPoll(int pollId) {
            return VoteTable.Where(x => x.PollId == pollId);
        }

        public IEnumerable<Vote> GetVotesInPollFrom(ulong userId, int pollId) {
            return VoteTable.Where(x => x.PollId == pollId && x.UserId == userId);
        }

        public void EditVote(int voteId, EVote choice) {
            var edit = VoteTable.Find(voteId);
            edit.Choice = choice;
            SaveChanges();
        }

        public void UpdatePollMessageId(int pollId, long messageId) {
            var edit = PollTable.Find(pollId);
            edit.MessageId = messageId;
            SaveChanges();
        }

        public void RemoveAdmin(Admin admin) {
            AdminTable.Remove(admin);
            SaveChanges();
        }

        public void RemoveAdmin(ulong Id) {
            var admin = AdminTable.Where(x => x.UserId == Id).ToList();
            AdminTable.RemoveRange(admin);
            SaveChanges();
        }

        public void RemovePoll(int pollId) {
            var polls = PollTable.Where(x => x.PollId == pollId).ToList();
            PollTable.RemoveRange(polls);
            var votes = VoteTable.Where(x => x.PollId == pollId).ToList();
            VoteTable.RemoveRange(votes);
            SaveChanges();
        }

        public void RemovePoll(Poll poll) {
            PollTable.Remove(poll);
            var votes = VoteTable.Where(x => x.PollId == poll.PollId).ToList();
            VoteTable.RemoveRange(votes);
            SaveChanges();
        }

        public void RemoveVote(int id) {
            var vote = VoteTable.Where(x => x.Id == id).First();
            VoteTable.Remove(vote);
            SaveChanges();
        }
    }
}