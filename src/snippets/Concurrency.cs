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