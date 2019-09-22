using System.Collections.Generic;
using ArmA_Bot.DBTables;

namespace ArmA_Bot {

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