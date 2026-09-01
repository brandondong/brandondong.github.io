using System;
using System.Threading.Tasks;

public class Program
{
    public class Meeting
    {
        public Meeting(Guid id) { }

        public void Load() { }
        public void Save() { }

        public T ReloadAndRetryOnConflict<T>(Func<T> f) { return f(); }

        public Participant GetParticipant(string userId)
        {
            return null;
        }

        public void SetParticipantUpdated(Participant p) { }
    }

    public static void RunInBackground(Action a) { }

    public static Task SendChangedEvent(Meeting m, string userId) { return null; }

    public static void Main()
    {
        Console.WriteLine("Hello World");
    }

    public class Participant
    {
        public bool IsHandRaised;
    }

    public class UserToken
    {
        public string Id;
    }

    public void RaiseHand(Guid meetingId, UserToken user)
    {
        var meeting = new Meeting(meetingId);
        // The data lives in memory, Load just does a deep copy.
        meeting.Load();

        var updated = meeting.ReloadAndRetryOnConflict(() =>
        {
            var participant = meeting.GetParticipant(user.Id);
            if (participant.IsHandRaised)
            {
                return false;
            }

            participant.IsHandRaised = true;
            meeting.SetParticipantUpdated(participant);

            // Under the hood, Save will acquire the mutex for this meeting
            // and merge in its dirty properties to the master object.
            // If the participant we set dirty was updated via another Save
            // since our last Load/Save, it'll throw a conflict exception.
            // ReloadAndRetryOnConflict will then call Load to get clean
            // up-to-date state and then rerun the closure.
            meeting.Save();
            return true;
        });

        if (updated)
        {
            RunInBackground(async () =>
            {
                await SendChangedEvent(meeting, user.Id);
            });
        }
    }
}