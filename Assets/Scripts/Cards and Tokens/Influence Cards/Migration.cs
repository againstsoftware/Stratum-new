
using System.Linq;

public class Migration : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action, bool checkOnlyFirstReceiver)
    {
        var receivers = action.Receivers;

        if (!checkOnlyFirstReceiver && receivers.Length != 2)
        {
            return false;
        }


        if (receivers[0].Location is not ValidDropLocation.OwnerCard)
        {
            return false;
        }


        var player = ServiceLocator.Get<IModel>().GetPlayer(action.Actor);
        if (receivers[0].Index is < 0 or >= 5)
        {
            return false;
        }

        var modelCards = player.Territory.Slots[receivers[0].Index].PlacedCards;

        if (receivers[0].SecondIndex < 0 || receivers[0].SecondIndex >= modelCards.Count)
        {
            return false;
        }

        var card = modelCards[receivers[0].SecondIndex];

        _feedbackKey = "on_animal";
        if (card.Card is not PopulationCard)
        {
            return false;
        }


        if (!card.GetPopulations().Contains(Population.Carnivore) &&
            !card.GetPopulations().Contains(Population.Herbivore))
        {
            return false;
        }

        _feedbackKey = "has_leash";
        if (card.HasLeash)
        {
            return false;
        }

        if (checkOnlyFirstReceiver) return true;

        _feedbackKey = "mig";
        if (receivers[1].LocationOwner == action.Actor)
        {
            return false;
        }

        if (receivers[1].Location is not ValidDropLocation.AnySlot)
        {
            return false;
        }

        if (receivers[1].Index is < 0 or >= 5)
        {
            return false;
        }

        var slotOwner = ServiceLocator.Get<IModel>().GetPlayer(receivers[1].LocationOwner);
        var slot = slotOwner.Territory.Slots[receivers[1].Index];
        if (slot.PlacedCards.Count > 0)
        {
            return false;
        }

        return true;
    }}