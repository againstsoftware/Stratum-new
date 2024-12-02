using System.Linq;

public class ExplosiveSpores : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action)
    {
        if (action.Receivers.Length != 1)
        {
            return false;
        }

        var receiver = action.Receivers[0];

        if (receiver.Location != ValidDropLocation.AnyTerritory)
        {
            return false;
        }

        var playerOwner = ServiceLocator.Get<IModel>().GetPlayer(receiver.LocationOwner);

        _feedbackKey = "on_fungi_terr";
        return ExistsFungiOnTerritory(playerOwner.Territory);
    }
}