using System.Linq;

public class Mold : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action, bool _)
    {
        _feedbackKey = "fatal_error";
        if (action.Receivers.Length != 1)
        {
            return false;
        }

        var receiver = action.Receivers[0];

        if (receiver.Location != ValidDropLocation.AnySlot)
        {
            return false;
        }

        _feedbackKey = "mold";
        var territory = ServiceLocator.Get<IModel>().GetPlayer(receiver.LocationOwner).Territory;
        var slot = territory.Slots[receiver.Index];
        if (slot.PlacedCards.Any())
        {
            return false;
        }

        if (!territory.HasConstruction)
        {
            return false;
        }

        return true;
    }
}