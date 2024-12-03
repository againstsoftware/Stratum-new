public class MacrofungiToken : AToken
{
    public override bool CheckAction(PlayerAction action, out string feedbackKey, bool checkOnlyFirstReceiver)
    {
        feedbackKey = null;
        // comprobar receivers 3 elementos
        if (!checkOnlyFirstReceiver && action.Receivers.Length != 3)
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

        foreach (var receiver in action.Receivers)
        {
            Slot slot = ServiceLocator.Get<IModel>().GetPlayer(receiver.LocationOwner).Territory.Slots[receiver.Index];

            var placedCard = slot.PlacedCards[receiver.SecondIndex];
            if (placedCard.Card is not MushroomCard)
            {
                feedbackKey = "macrofungi";
                return false;
            }
            else if (checkOnlyFirstReceiver) return true;
        }

        return true;
    }
}