public class Arson : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action)
    {
        _feedbackKey = "fatal_error";
        if (action.Receivers.Length != 1)
        {
            return false;
        }

        var receiver = action.Receivers[0];

        if (receiver.Location != ValidDropLocation.AnyTerritory)
        {
            return false;
        }

        return true;
    }
}