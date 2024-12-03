
public class MotherNature : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action, bool checkOnlyFirstReceiver)
    {
        _feedbackKey = "fatal_error";
        if (checkOnlyFirstReceiver && action.Receivers[0].Location is not ValidDropLocation.TableCenter)
            return false;

        return true;
    }
}
