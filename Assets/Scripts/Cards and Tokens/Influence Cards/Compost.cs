using System.Linq;

public class Compost : AInfluenceCard
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

        if (receiver.Index is < 0 or >= 5)
        {
            return false;
        }

        var slotOwner = ServiceLocator.Get<IModel>().GetPlayer(receiver.LocationOwner);
        var slot = slotOwner.Territory.Slots[receiver.Index];

        _feedbackKey = "on_empty_slot";
        if (slot.PlacedCards.Any())
        {
            return false;
        }

        return true;
    }
}