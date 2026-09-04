public void LowerHands(Meeting meeting)
{
    var participants = meeting.GetAllParticipants();
    foreach (var participant in participants)
    {
        participant.SetIsHandRaised(false);

        this.SharedHelper(meeting, participant.Id);
    }

    meeting.Save();
}