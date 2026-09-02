using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public class Program
{
    public class Meeting
    {
        public Guid Id;
        public Meeting(Guid id) { Id = id; }

        public void Load() { }
        public void Save() { }

        public T ReloadAndRetryOnConflict<T>(Func<T> f) { return f(); }

        public void ReloadAndRetryOnConflict(Action f) { }

        public Participant GetParticipant(Guid userId)
        {
            return null;
        }

        public void SetParticipantUpdated(Participant p) { }
    }

    public static void RunInBackground(Action a) { }

    public static Task SendChangedEvent(Meeting m, Guid userId) { return null; }

    public static void Main()
    {
        Console.WriteLine("Hello World");
    }

    public class Participant
    {
        public bool IsHandRaised;
        public bool InLobby;
    }

    public class UserToken
    {
        public string Id;
    }

    public Task<bool> DoWork1() { return null; }

    public Task<bool> DoWork2() { return null; }

    public Task<bool> DoWork() { return null; }

    public static IActionResult Ok() { return null; }
    public static IActionResult Accepted() { return null; }

    [HttpPost("{meetingId}/admitUser/{targetUserId}")]
    public IActionResult AdmitUser(Guid meetingId, Guid targetUserId)
    {
        var meeting = new Meeting(meetingId);
        // The data lives in memory, Load just does a deep copy.
        meeting.Load();

        var updated = meeting.ReloadAndRetryOnConflict(() =>
        {
            var participant = meeting.GetParticipant(targetUserId);
            if (participant?.InLobby != true)
            {
                return false;
            }

            participant.InLobby = false;
            meeting.SetParticipantUpdated(participant);

            // Under the hood, Save will acquire the mutex for this meeting,
            // merge in its dirty properties to the master object, and then
            // call Load before returning.
            // If during the merge it finds the participant was updated via
            // another Save since its last Load, it'll throw a conflict
            // exception instead.
            // ReloadAndRetryOnConflict will then call Load to get clean
            // up-to-date state before rerunning the closure.
            meeting.Save();
            return true;
        });

        if (updated)
        {
            RunInBackground(async () =>
            {
                await SendChangedEvent(meeting, targetUserId);
            });
        }

        return Accepted();
    }

    public async Task Process(Meeting meeting)
    {
        RunInBackground(async () =>
        {
            var result = await DoWork2();
            meeting.ReloadAndRetryOnConflict(() =>
            {
                // ... Update property Y based on result and save.
            });
        });

        var result = await DoWork1();
        meeting.ReloadAndRetryOnConflict(() =>
        {
            // ... Update property X based on result and save.
        });
    }

    public async Task Process2(Meeting meeting)
    {
        RunInBackground(async () =>
        {
            // NEW:
            var newMeeting = new Meeting(meeting.Id);
            newMeeting.Load();

            var result = await DoWork2();
            newMeeting.ReloadAndRetryOnConflict(() =>
            {
                // ... Update property Y based on result and save.
            });
        });

        var result = await DoWork1();
        meeting.ReloadAndRetryOnConflict(() =>
        {
            // ... Update property X based on result and save.
        });
    }
}