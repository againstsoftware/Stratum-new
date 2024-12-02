
using System.Linq;

public class ConstructionToken : AToken
{
    
    public override bool CheckAction(PlayerAction action, out string feedbackKey)
    {
        var receivers = action.Receivers;

        if (receivers.Length != 1)
        {
            feedbackKey = "fatal_error";
            return false;
        }
        
        var actor = ServiceLocator.Get<IModel>().GetPlayer(action.Actor);
        if (actor.TokenPlayed)
        {
            feedbackKey = "token_played";
            return false;
        }

        var owner = ServiceLocator.Get<IModel>().GetPlayer(receivers[0].LocationOwner);

        if (owner is null)
        {
            feedbackKey = "fatal_error";
            return false;
        }

        if (receivers[0].Location != ValidDropLocation.AnyTerritory ||
            owner.Territory.HasConstruction)
        {
            feedbackKey = "con_token_empty";
            return false;
        }


        // comprobar si hay algun carnivoro y 2 o mas plantas
        int plants = 0;
        foreach (var slot in owner.Territory.Slots)
        {
            foreach (var placedCard in slot.PlacedCards)
            {
                if (placedCard.Card is not PopulationCard) continue;

                if (placedCard.GetPopulations().Contains(Population.Carnivore))
                {
                    feedbackKey = "con_token_carn";
                    return false;
                }
                if(placedCard.HasRabies)
                {
                    feedbackKey = "con_token_rabies";

                    return false;
                }

                if (placedCard.GetPopulations().Contains(Population.Plant))
                    plants++;
            }
        }

        // 2 plantas al menos
        feedbackKey = null;
        if (plants >= 2) return true;

        feedbackKey = "con_token_plants";
        return false;
    }
}