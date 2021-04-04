using ArmABot.DBTables;
using System.Collections.Generic;

namespace ArmABot {
	//This class was a compatibility layer to support various type of database APIs effortlessly.
	//Simply creating a class for each database type that implemented all theese functions translated to the database would add support for the database.
	//This is actually not ideal and needs to be reviewed.
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
	}
}