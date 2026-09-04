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

        participant.SetInLobby(false);

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