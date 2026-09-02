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