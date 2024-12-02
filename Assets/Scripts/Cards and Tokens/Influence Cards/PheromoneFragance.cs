using System.Linq;

public class PheromoneFragance : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action)
    {
        _feedbackKey = "fatal_error";
        var receivers = action.Receivers;

        if (receivers.Length != 2)
        {
            return false;
        }

        if (receivers[0].Location is not ValidDropLocation.AnyCard)
        {
            return false;
        }

        if (receivers[0].Index is < 0 or >= 5)
        {
            return false;
        }


        var cardOwner = ServiceLocator.Get<IModel>().GetPlayer(receivers[0].LocationOwner);

        var cardOwnerPlacedCards = cardOwner.Territory.Slots[receivers[0].Index].PlacedCards;

        if (receivers[0].SecondIndex < 0 || receivers[0].SecondIndex >= cardOwnerPlacedCards.Count)
        {
            return false;
        }
        
        if (receivers[0].LocationOwner == action.Actor)
        {
            _feedbackKey = "frag1";
            return false;
        }


        var card = cardOwnerPlacedCards[receivers[0].SecondIndex];

        _feedbackKey = "on_animal";
        if (card.Card is not PopulationCard)
        {
            return false;
        }
        
        if (card.HasLeash)
        {
            _feedbackKey = "has_leash";
            return false;
        }

        if (!card.GetPopulations().Contains(Population.Carnivore) &&
            !card.GetPopulations().Contains(Population.Herbivore))
        {
            return false;
        }

        _feedbackKey = "frag2";
        if (receivers[1].LocationOwner != action.Actor)
        {
            return false;
        }

        if (receivers[1].Location is not ValidDropLocation.OwnerSlot)
        {
            return false;
        }

        if (receivers[1].Index is < 0 or >= 5)
        {
            _feedbackKey = "fatal_error";
            return false;
        }

        var slotOwner = ServiceLocator.Get<IModel>().GetPlayer(receivers[1].LocationOwner);
        var slot = slotOwner.Territory.Slots[receivers[1].Index];
        if (slot.PlacedCards.Count > 0)
        {
            return false;
        }

        return true;
    }
}