using System.Collections.Generic;
using ArmA_Bot.DBTables;

namespace ArmA_Bot {

    internal interface IDatabaseAccess {

        //Admin section
        void AddAdmin(Admin admin);

        Admin FindAdmin(ulong id, ulong groupId);

        void RemoveAdmin(Admin admin);

        void RemoveAdmin(ulong id);

        //Poll Section
        int AddPoll(Poll poll);

        Poll GetPoll(int pollId);

        IEnumerable<Poll> GetPollsBy(ulong adminId, ulong groupId);

        void UpdatePollMessageId(int pollId, long messageId);

        void RemovePoll(int pollId);

        void RemovePoll(Poll poll);

        //Vote section
        void AddVote(EVote choice, int pollId, ulong userId, string username);

        void RemoveVote(int id);

        IEnumerable<Vote> GetVotesInPoll(int pollId);

        IEnumerable<Vote> GetVotesInPollFrom(ulong userId, int pollId);

        void EditVote(int voteId, EVote choice);
    }
}