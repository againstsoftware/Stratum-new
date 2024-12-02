using System.Linq;

public class Rabies : AInfluenceCard
{
    protected override bool CheckInfluenceCardAction(PlayerAction action)
    {
        _feedbackKey = "fatal_error";
        if (action.Receivers.Length != 1)
        {
            return false;
        }

        var receiver = action.Receivers[0];

        if (receiver.Location != ValidDropLocation.AnyCard)
        {
            return false;
        }

        if (receiver.Index is < 0 or >= 5)
        {
            return false;
        }

        var cardOwner = ServiceLocator.Get<IModel>().GetPlayer(receiver.LocationOwner);
        var cardOwnerPlacedCards = cardOwner.Territory.Slots[receiver.Index].PlacedCards;

        if (receiver.SecondIndex < 0 || receiver.SecondIndex >= cardOwnerPlacedCards.Count)
        {
            return false;
        }

        var card = cardOwnerPlacedCards[receiver.SecondIndex];

        _feedbackKey = "influence_ontop";
        if (card.InfluenceCardOnTop is not null)
        {
            return false;
        }

        if (card.HasRabies)
        {
            return false;
        }

        _feedbackKey = "on_herbivore";
        if (!card.GetPopulations().Contains(Population.Herbivore))
        {
            return false;
        }

        if (!card.Card.CanHaveInfluenceCardOnTop)
        {
            return false;
        }

        return true;
    }
}