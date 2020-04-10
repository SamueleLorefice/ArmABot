using ArmABot.DBTables;
using System.Collections.Generic;

namespace ArmABot {

	internal interface IDatabaseAccess {

		//Admin section
		void AddAdmin(Admin admin);

		Admin FindAdmin(long id, long groupId);

		void RemoveAdmin(Admin admin);

		void RemoveAdmin(long id);

		//Poll Section
		int AddPoll(Poll poll);

		Poll GetPoll(int pollId);

		IEnumerable<Poll> GetPollsBy(long adminId, long groupId);

		void UpdatePollMessageId(int pollId, long messageId);

		void RemovePoll(int pollId);

		void RemovePoll(Poll poll);

		//Vote section
		void AddVote(EVote choice, int pollId, long userId, string username);

		void RemoveVote(int id);

		IEnumerable<Vote> GetVotesInPoll(int pollId);

		IEnumerable<Vote> GetVotesInPollFrom(long userId, int pollId);

		void EditVote(int voteId, EVote choice);

		//Specialization section
		void AddSpecialization(string name);

		void AddPreRequisiteSpec(int specializationId, int preRequisiteId);

		void RemoveSpecialization(int id);

		IEnumerable<Specialization> GetSpecializations(string user);

		//User table
		public void AddUser(string name, long telegramId, int gradeId = 0);

		User FindUserFromId(long id);

		User FindUserFromName(string name);

		User FindUserFromTelegram(long telegramId);

		IEnumerable<User> GetUsers();

		void RemoveUser(User user);

		void RemoveUser(long id);

		//Grade Table
		void AddGrade(Grade grade);

		IEnumerable<Grade> GetGrades();

		//Spec Assignment
		void AssignUpgradeGrade(int userId, int gradeId);
	}
}