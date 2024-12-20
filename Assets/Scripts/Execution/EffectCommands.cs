using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffectCommands
{
    public static IEffectCommand Get(Effect effect) => effect switch
    {
        Effect.PlacePopulationCardFromPlayer => new PlacePopulationCardFromPlayer(),
        Effect.PlaceInitialCards => new PlaceInitialCards(),
        Effect.GrowCarnivoreEcosystem => new GrowCarnivoreEcosystem(),
        Effect.GrowHerbivoreEcosystem => new GrowHerbivoreEcosystem(),
        Effect.KillCarnivoreEcosystem => new KillCarnivoreEcosystem(),
        Effect.KillHerbivoreEcosystem => new KillHerbivoreEcosystem(),
        Effect.GrowMushroomEcosystem => new GrowMushroomEcosystem(),
        Effect.Draw2 => new DrawCards(),
        Effect.Draw5 => new DrawCards(),
        Effect.OverviewSwitch => new OverviewSwitch(),
        Effect.Discard => new Discard(),
        Effect.GrowMushroom => new GrowMushroom(),
        Effect.GrowMushroomEndOfAction => new GrowMushroomEOA(),
        Effect.GrowMacrofungi => new GrowMacrofungi(),
        Effect.PlaceConstruction => new PlaceConstruction(),
        Effect.PlayAndDiscardInfluenceCard => new PlayAndDiscardInfluenceCard(),
        Effect.MovePopulationToEmptySlot => new MovePopulationToEmptySlot(),
        Effect.PlaceInfluenceOnPopulation => new PlaceInfluenceOnPopulation(),
        Effect.GiveRabies => new GiveRabies(),
        Effect.PutLeash => new PutLeash(),
        Effect.DestroyAllInTerritory => new DestroyAllInTerritory(),
        Effect.DestroyNonFungiInTerritory => new DestroyNonFungiInTerritory(),
        Effect.MakeOmnivore => new MakeOmnivore(),
        Effect.GrowPlant => new GrowPlant(),
        Effect.GrowPlantEndOfAction => new GrowPlantEOA(),
        Effect.KillPlacedCard => new KillPlacedCard(),
        Effect.ObserveSeededFruit => new ObserveSeededFruit(),
        Effect.ObserveDeepRoots => new ObserveDeepRoots(),
        Effect.ObserveGreenIvy => new ObserveGreenIvy(),
        Effect.ObserveMushroomPredator => new ObserveMushroomPredator(),
        Effect.ObserveParasite => new ObserveParasite(),
        Effect.RushEcosystemTurn => new RushEcosystemTurn(),
        Effect.FireInTerritory => new FireInTerritory(),
        Effect.ShowBirds => new ShowBirds(),
        Effect.ShowFireworks => new ShowFireworks(),
        Effect.ShowDirt => new ShowDirt(),
        Effect.ShowFragrance => new ShowFragrance(),
        Effect.ShowAppetizingMushroom => new ShowAppetizingMushroom(),

        _ => throw new ArgumentOutOfRangeException()
    };

    public class PlacePopulationCardFromPlayer : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var card = action.ActionItem as ACard;
            var owner = action.Actor;
            var slotIndex = action.Receivers[0].Index;
            ServiceLocator.Get<IModel>().RemoveCardFromHand(owner, card);
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(card, owner, slotIndex);
            var location = new IView.CardLocation { Owner = owner, SlotIndex = slotIndex };
            ServiceLocator.Get<IView>().PlayCardOnSlotFromPlayer(card, owner, location, callback);
        }
    }

    public class GrowCarnivoreEcosystem : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>().GrowLastPlacedPopulation(Population.Carnivore, out var parent, out _);
            var location = new IView.CardLocation
                { Owner = parent.Slot.Territory.Owner, SlotIndex = parent.Slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().GrowPopulationCardEcosystem(location, callback);
        }
    }

    public class GrowHerbivoreEcosystem : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>().GrowLastPlacedPopulation(Population.Herbivore, out var parent, out _);
            var location = new IView.CardLocation
                { Owner = parent.Slot.Territory.Owner, SlotIndex = parent.Slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().GrowPopulationCardEcosystem(location, callback);
        }
    }

    public class KillCarnivoreEcosystem : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var killed = ServiceLocator.Get<IModel>().KillLastPlacedPopulation(Population.Carnivore);
            var location = new IView.CardLocation
                { Owner = killed.Slot.Territory.Owner, SlotIndex = killed.Slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().KillPopulationCardEcosystem(location, callback);
        }
    }

    public class KillHerbivoreEcosystem : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var killed = ServiceLocator.Get<IModel>().KillLastPlacedPopulation(Population.Herbivore);
            var location = new IView.CardLocation
                { Owner = killed.Slot.Territory.Owner, SlotIndex = killed.Slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().KillPopulationCardEcosystem(location, callback);
        }
    }

    public class Discard : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveCardFromHand(action.Actor, action.ActionItem as ACard);
            ServiceLocator.Get<IView>().Discard(action.Actor, callback);
        }
    }

    public class DrawCards : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            Dictionary<PlayerCharacter, IReadOnlyList<ACard>> cardsDrawn = new();
            foreach (var character in ServiceLocator.Get<IModel>().Config.TurnOrder)
            {
                if (character is PlayerCharacter.None) continue;
                var cards = ServiceLocator.Get<IModel>().PlayerDrawCards(character);
                cardsDrawn.Add(character, new List<ACard>(cards));
            }

            ServiceLocator.Get<IView>().DrawCards(cardsDrawn, callback);
        }
    }

    public class OverviewSwitch : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IView>().SwitchCamToOverview(action.Actor, callback);
        }
    }

    public class GrowMushroomEcosystem : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var mushroom = ServiceLocator.Get<IModel>().GrowMushroomOverLastDeadPopulation();
            var slotOwner = mushroom.Slot.Territory.Owner;
            var slotIndex = mushroom.Slot.SlotIndexInTerritory;
            ServiceLocator.Get<IView>()
                .GrowMushroom(new IView.CardLocation { Owner = slotOwner, SlotIndex = slotIndex }, callback);
        }
    }

    public class GrowMushroom : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            ServiceLocator.Get<IModel>().GrowMushroom(slotOwner, slotIndex);
            ServiceLocator.Get<IView>()
                .GrowMushroom(new IView.CardLocation { Owner = slotOwner, SlotIndex = slotIndex }, callback);
        }
    }

    public class GrowMushroomEOA : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            ServiceLocator.Get<IModel>().GrowMushroom(slotOwner, slotIndex);
            ServiceLocator.Get<IView>()
                .GrowMushroom(new IView.CardLocation { Owner = slotOwner, SlotIndex = slotIndex }, callback, true);
        }
    }

    public class PlaceInitialCards : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var config = ServiceLocator.Get<IModel>().Config;
            var initialCards = new Stack<PopulationCard>(config.InitialCards);
            var players = config.TurnOrder.ToList();
            players.Remove(PlayerCharacter.None);

            var cardsAndLocations = new List<(ACard card, IView.CardLocation location)>();

            int count = Mathf.Min(initialCards.Count, players.Count);
            while (count > 0)
            {
                count--;
                
                var index = ServiceLocator.Get<IRNG>().Range(0, players.Count);
                
                var slotOwner = players[index];
                players.RemoveAt(index);

                var initialCard = initialCards.Pop();
                int slotIndex = 0;
                ServiceLocator.Get<IModel>().PlaceCardOnSlot(initialCard, slotOwner, slotIndex);
                var location = new IView.CardLocation { Owner = slotOwner, SlotIndex = slotIndex };

                cardsAndLocations.Add((initialCard, location));
            }

            ServiceLocator.Get<IView>().PlaceInitialCards(cardsAndLocations, callback);
        }
    }

    public class GrowMacrofungi : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var locations = new List<IView.CardLocation>();
            PlayerCharacter slotOwner = default;
            int slotIndex = default;

            var receivers = new List<Receiver>(action.Receivers);

            
            //cuando se eligen 2 setas que esten en el mismo slot, hay que eliminarlas de mayoer a menor indice
            //de lo contrario al quitar la de menor indice la otra cambia de indice y da error
            bool needsSorting = false;
            for (int i = 0; i < receivers.Count; i++)
            {
                var r1 = receivers[i];
                for (int j = 0; j < receivers.Count; j++)
                {
                    var r2 = receivers[j];
                    
                    if (!r1.Equals(r2) &&
                        r1.LocationOwner == r2.LocationOwner &&
                        r1.Index == r2.Index)
                    {
                        needsSorting = true;
                        break;
                    }
                }
            }

            if (needsSorting)
            {
                Debug.Log("needs sorting");
                receivers.Sort((a, b) => a.SecondIndex.CompareTo(b.SecondIndex));
                receivers.Reverse();
            }
            
            foreach (var receiver in receivers)
            {
                slotOwner = receiver.LocationOwner;
                slotIndex = receiver.Index;
                locations.Add(new IView.CardLocation { Owner = slotOwner, SlotIndex = slotIndex });
                var cardIndex = receiver.SecondIndex;
                ServiceLocator.Get<IModel>().RemoveCardFromSlot(slotOwner, slotIndex, cardIndex);
            }

            var macrofungiCard = ServiceLocator.Get<IModel>().Config.Macrofungi;
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(macrofungiCard, slotOwner, slotIndex, true);
            ServiceLocator.Get<IView>().GrowMacrofungi(locations.ToArray(), callback);
        }
    }

    public class PlaceConstruction : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var owner = action.Receivers[0].LocationOwner;
            ServiceLocator.Get<IModel>().PlaceConstruction(owner, out var plant1, out var plant2);

            var location1 = new IView.CardLocation
            {
                SlotIndex = plant1.Slot.SlotIndexInTerritory, CardIndex = plant1.IndexInSlot,
                Owner = plant1.Slot.Territory.Owner
            };

            var location2 = new IView.CardLocation
            {
                SlotIndex = plant2.Slot.SlotIndexInTerritory, CardIndex = plant2.IndexInSlot,
                Owner = plant2.Slot.Territory.Owner
            };

            ServiceLocator.Get<IView>().PlaceConstruction(location1, location2, callback);
        }
    }

    public class PlayAndDiscardInfluenceCard : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveCardFromHand(action.Actor, action.ActionItem as ACard);

            IView.CardLocation location;

            if (action.Receivers.Length > 0)
            {
                bool isTerritory = action.Receivers[0].Location is ValidDropLocation.AnyTerritory;
                location = new()
                {
                    IsTerritory = isTerritory,
                    Owner = action.Receivers[0].LocationOwner,
                    SlotIndex = isTerritory ? -1 : action.Receivers[0].Index,
                    CardIndex = isTerritory ? -1 : action.Receivers[0].SecondIndex,
                };
            }
            else location = new() { IsTableCenter = true };


            var influence = action.ActionItem as AInfluenceCard;
            ServiceLocator.Get<IView>().PlayAndDiscardInfluenceCard(action.Actor, influence, location, callback);
        }
    }

    public class MovePopulationToEmptySlot : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var targetSlotOwner = action.Receivers[1].LocationOwner;
            var targetSlotIndex = action.Receivers[1].Index;
            ServiceLocator.Get<IModel>()
                .MoveCardBetweenSlots(slotOwner, slotIndex, cardIndex, targetSlotOwner, targetSlotIndex);

            var from = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            var to = new IView.CardLocation
            {
                Owner = targetSlotOwner,
                SlotIndex = targetSlotIndex
            };
            
            ServiceLocator.Get<IView>().MovePopulationToEmptySlot(action.Actor, from, to, callback);
        }
    }
    
    public class ShowBirds : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var targetSlotOwner = action.Receivers[1].LocationOwner;
            var targetSlotIndex = action.Receivers[1].Index;
            var from = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            var to = new IView.CardLocation
            {
                Owner = targetSlotOwner,
                SlotIndex = targetSlotIndex
            };
            ServiceLocator.Get<IView>().ShowBirds(from, to, callback);
        }
    }
    
    public class ShowFireworks : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };
            ServiceLocator.Get<IView>().ShowFireworks(location, callback);
        }
    }
    
    public class ShowDirt : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
            };
            ServiceLocator.Get<IView>().ShowDirt(location, callback);
        }
    }
    
    public class ShowFragrance : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var targetSlotOwner = action.Receivers[1].LocationOwner;
            var targetSlotIndex = action.Receivers[1].Index;
            var from = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            var to = new IView.CardLocation
            {
                Owner = targetSlotOwner,
                SlotIndex = targetSlotIndex
            };
            ServiceLocator.Get<IView>().ShowFragrance(from, to, callback);
        }
    }
    
    public class ShowAppetizingMushroom : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var targetSlotOwner = action.Receivers[1].LocationOwner;
            var targetSlotIndex = action.Receivers[1].Index;
            var from = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            var to = new IView.CardLocation
            {
                Owner = targetSlotOwner,
                SlotIndex = targetSlotIndex
            };
            ServiceLocator.Get<IView>().ShowAppetizingMushroom(from, to, callback);
        }
    }

    public class PlaceInfluenceOnPopulation : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var influenceCard = action.ActionItem as AInfluenceCard;
            ServiceLocator.Get<IModel>().RemoveCardFromHand(action.Actor, action.ActionItem as ACard);
            ServiceLocator.Get<IModel>().PlaceInlfuenceCardOnCard(influenceCard, slotOwner, slotIndex, cardIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            ServiceLocator.Get<IView>().PlaceInfluenceOnPopulation(action.Actor, influenceCard, location, callback);
        }
    }

    public class GiveRabies : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;

            ServiceLocator.Get<IModel>().GiveRabies(slotOwner, slotIndex, cardIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            ServiceLocator.Get<IView>().GiveRabies(action.Actor, location, callback);
        }
    }

    public class PutLeash : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;

            ServiceLocator.Get<IModel>().PutLeash(slotOwner, slotIndex, cardIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            ServiceLocator.Get<IView>().PutLeash(action.Actor, location, callback);
        }
    }

    public class DestroyAllInTerritory : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            DestroyInTerritory(action, callback, card => card is MacrofungiCard);
        }
    }

    public class DestroyNonFungiInTerritory : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            DestroyInTerritory(action, callback, card => card is MushroomCard or MacrofungiCard);
        }
    }

    public static void DestroyInTerritory(PlayerAction action, Action callback, Predicate<ACard> filter)
    {
        var owner = action.Receivers[0].LocationOwner;
        Predicate<TableCard> modelFilter = filter is null ? null : tCard => filter(tCard.Card);
        ServiceLocator.Get<IModel>().RemoveCardsFromTerritory(owner, modelFilter);

        if (ServiceLocator.Get<IModel>().GetPlayer(owner).Territory.HasConstruction)
            ServiceLocator.Get<IModel>().RemoveConstruction(owner);

        ServiceLocator.Get<IView>().DestroyInTerritory(owner, callback, filter);
    }

    public class MakeOmnivore : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;

            ServiceLocator.Get<IModel>().MakeOmnivore(slotOwner, slotIndex, cardIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };

            ServiceLocator.Get<IView>().MakeOmnivore(action.Actor, location, callback);
        }
    }

    public class GrowPlant : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var card = action.ActionItem as ACard;
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(card, slotOwner, slotIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
            };

            ServiceLocator.Get<IView>().GrowPopulation(location, Population.Plant, callback);
        }
    }

    public class GrowPlantEOA : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var card = ServiceLocator.Get<IModel>().Config.GetPopulationCard(Population.Plant);
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(card, slotOwner, slotIndex);

            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
            };

            ServiceLocator.Get<IView>().GrowPopulation(location, Population.Plant, callback, true);
        }
    }

    public class KillPlacedCard : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;

            ServiceLocator.Get<IModel>().RemoveCardFromSlot(slotOwner, slotIndex, cardIndex);
            var location = new IView.CardLocation
            {
                Owner = slotOwner,
                SlotIndex = slotIndex,
                CardIndex = cardIndex
            };
            ServiceLocator.Get<IView>().KillPlacedCard(location, callback /*, true*/);
        }
    }

    public class ObserveSeededFruit : IEffectCommand
    {
        private TableCard _tableCardWherePlaced;
        private Territory _territory;

        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            _territory = ServiceLocator.Get<IModel>().GetPlayer(slotOwner).Territory;
            _tableCardWherePlaced = _territory.Slots[slotIndex].PlacedCards[cardIndex];

            _tableCardWherePlaced.OnSlotRemove += OnCardRemoved;
            ServiceLocator.Get<IModel>().OnPopulationGrow += OnPopulationGrow;
            callback?.Invoke();
        }

        private void OnCardRemoved()
        {
            Debug.Log("frutasemillas quitado");
            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            ServiceLocator.Get<IModel>().OnPopulationGrow -= OnPopulationGrow;
        }

        private void OnPopulationGrow(TableCard parent, TableCard child)
        {
            if (parent.Slot.Territory != _territory) return;

            ServiceLocator.Get<IModel>().OnPopulationGrow -= OnPopulationGrow;

            var growPlant = new DelayedGrowPlant(_tableCardWherePlaced.Card, _tableCardWherePlaced.Slot);
            var discard = new DelayedDiscardPlayedInfluence(_tableCardWherePlaced);
            ServiceLocator.Get<IExecutor>().PushDelayedCommand(growPlant);
            ServiceLocator.Get<IExecutor>().PushDelayedCommand(discard);
        }
    }

    public class ObserveDeepRoots : IEffectCommand
    {
        private TableCard _tableCardWherePlaced;

        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var territory = ServiceLocator.Get<IModel>().GetPlayer(slotOwner).Territory;
            _tableCardWherePlaced = territory.Slots[slotIndex].PlacedCards[cardIndex];

            _tableCardWherePlaced.OnSlotRemove += OnCardRemoved;
            callback?.Invoke();
        }

        private void OnCardRemoved()
        {
            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            var growPlant = new DelayedGrowPlant(_tableCardWherePlaced.Card, _tableCardWherePlaced.Slot);
            ServiceLocator.Get<IExecutor>().PushDelayedCommand(growPlant);
        }
    }


    public class ObserveGreenIvy : IRoundEndObserverEffectCommand
    {
        private TableCard _tableCardWherePlaced;
        private Territory _territory;

        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            _territory = ServiceLocator.Get<IModel>().GetPlayer(slotOwner).Territory;
            _tableCardWherePlaced = _territory.Slots[slotIndex].PlacedCards[cardIndex];

            _tableCardWherePlaced.OnSlotRemove += OnCardRemoved;
            ServiceLocator.Get<IRulesSystem>().RegisterRoundEndObserver(this);

            callback?.Invoke();
        }

        private void OnCardRemoved()
        {
            Debug.Log("hiedraverde quitado");
            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            ServiceLocator.Get<IRulesSystem>().RemoveRoundEndObserver(this);
        }

        public IEnumerable<IEffectCommand> GetRoundEndEffects()
        {
            if (!_territory.HasConstruction) return new IEffectCommand[] { };

            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            ServiceLocator.Get<IRulesSystem>().RemoveRoundEndObserver(this);

            var discard = new DelayedDiscardPlayedInfluence(_tableCardWherePlaced);
            var destroyConstruction = new DelayedDestroyConstruction(_territory, true);
            return new IEffectCommand[] { discard, destroyConstruction };
        }
    }

    public class ObserveMushroomPredator : IRoundEndObserverEffectCommand
    {
        private TableCard _tableCardWherePlaced;
        private Territory _territory;

        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            _territory = ServiceLocator.Get<IModel>().GetPlayer(slotOwner).Territory;
            _tableCardWherePlaced = _territory.Slots[slotIndex].PlacedCards[cardIndex];

            _tableCardWherePlaced.OnSlotRemove += OnCardRemoved;
            ServiceLocator.Get<IRulesSystem>().RegisterRoundEndObserver(this);

            callback?.Invoke();
        }

        private void OnCardRemoved()
        {
            Debug.Log("depresetas quitado");
            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            ServiceLocator.Get<IRulesSystem>().RemoveRoundEndObserver(this);
        }

        public IEnumerable<IEffectCommand> GetRoundEndEffects()
        {
            var lastMushroom = ServiceLocator.Get<IModel>().GetLastMushroomInTerritory(_territory.Owner);
            if (lastMushroom is null) return new IEffectCommand[] { };


            //a lo mejor renta antes un efectillo visual
            var killMushroom = new DelayedKillMushroom(lastMushroom);
            return new IEffectCommand[] { killMushroom };
        }
    }

    public class ObserveParasite : IEffectCommand
    {
        private TableCard _tableCardWherePlaced;

        public void Execute(PlayerAction action, Action callback)
        {
            var slotOwner = action.Receivers[0].LocationOwner;
            var slotIndex = action.Receivers[0].Index;
            var cardIndex = action.Receivers[0].SecondIndex;
            var territory = ServiceLocator.Get<IModel>().GetPlayer(slotOwner).Territory;
            _tableCardWherePlaced = territory.Slots[slotIndex].PlacedCards[cardIndex];

            _tableCardWherePlaced.OnSlotRemove += OnCardRemoved;
            ServiceLocator.Get<IModel>().OnPopulationGrow += OnPopulationGrow;
            callback?.Invoke();
        }

        private void OnCardRemoved()
        {
            Debug.Log("parasito quitado");
            _tableCardWherePlaced.OnSlotRemove -= OnCardRemoved;
            ServiceLocator.Get<IModel>().OnPopulationGrow -= OnPopulationGrow;
        }

        private void OnPopulationGrow(TableCard parent, TableCard child)
        {
            if (parent != _tableCardWherePlaced) return;

            ServiceLocator.Get<IModel>().OnPopulationGrow -= OnPopulationGrow;

            var growMushroom = new DelayedGrowMushroom(_tableCardWherePlaced.Slot);
            var discard = new DelayedDiscardPlayedInfluence(_tableCardWherePlaced);
            ServiceLocator.Get<IExecutor>().PushDelayedCommand(growMushroom);
            ServiceLocator.Get<IExecutor>().PushDelayedCommand(discard);
        }
    }

    public class RushEcosystemTurn : IEffectCommand
    {
        public void Execute(PlayerAction _, Action callback)
        {
            var ecosystemEffects = RulesCheck.CheckEcosystem();
            ecosystemEffects.Reverse();
            ServiceLocator.Get<IView>().SpinTurnMarker(null);
            foreach (var effect in ecosystemEffects)
                ServiceLocator.Get<IExecutor>().PushDelayedCommand(Get(effect));
            callback?.Invoke();
        }
    }

    public class FireInTerritory : IEffectCommand
    {
        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IView>().GetViewPlayer(action.Receivers[0].LocationOwner).Territory.SetOnFire(callback);
        }
    }


    public class DelayedDiscardPlayedInfluence : IEffectCommand
    {
        private TableCard _tableCardWherePlaced;

        public DelayedDiscardPlayedInfluence(TableCard tableCardWherePlaced)
        {
            _tableCardWherePlaced = tableCardWherePlaced;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveInfluenceCardFromCard(_tableCardWherePlaced);
            var location = new IView.CardLocation()
            {
                Owner = _tableCardWherePlaced.Slot.Territory.Owner,
                SlotIndex = _tableCardWherePlaced.Slot.SlotIndexInTerritory,
                CardIndex = _tableCardWherePlaced.IndexInSlot
            };
            ServiceLocator.Get<IView>().DiscardInfluenceFromPopulation(location, callback);
        }
    }

    public class DelayedGrowPlant : IEffectCommand
    {
        private ACard _card;
        private Slot _slot;

        public DelayedGrowPlant(ACard card, Slot slot)
        {
            _card = card;
            _slot = slot;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(_card, _slot);

            var location = new IView.CardLocation
            {
                Owner = _slot.Territory.Owner,
                SlotIndex = _slot.SlotIndexInTerritory,
            };

            ServiceLocator.Get<IView>().GrowPopulation(location, Population.Plant, callback);
        }
    }

    public class DelayedGrowMushroom : IEffectCommand
    {
        private Slot _slot;

        public DelayedGrowMushroom(Slot slot)
        {
            _slot = slot;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().GrowMushroom(_slot);

            var location = new IView.CardLocation
            {
                Owner = _slot.Territory.Owner,
                SlotIndex = _slot.SlotIndexInTerritory,
            };

            ServiceLocator.Get<IView>().GrowMushroom(location, callback);
        }
    }

    public class DelayedDestroyConstruction : IEffectCommand
    {
        private Territory _territory;
        private bool _isIvy = false;

        public DelayedDestroyConstruction(Territory territory, bool isIvy)
        {
            _territory = territory;
            _isIvy = isIvy;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveConstruction(_territory.Owner);
            ServiceLocator.Get<IView>().DestroyConstruction(_territory.Owner, callback, _isIvy);
        }
    }

    public class DelayedKillMushroom : IEffectCommand
    {
        private TableCard _mushroom;

        public DelayedKillMushroom(TableCard mushroom)
        {
            _mushroom = mushroom;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveCardFromSlot(_mushroom);
            var location = new IView.CardLocation
            {
                Owner = _mushroom.Slot.Territory.Owner,
                SlotIndex = _mushroom.Slot.SlotIndexInTerritory,
                CardIndex = _mushroom.IndexInSlot
            };
            ServiceLocator.Get<IView>().KillPlacedCard(location, callback);
        }
    }


    public class PlaceCardOnSlotTutorial : IEffectCommand
    {
        private ACard _card;
        private Slot _slot;
        private bool _atTheBottom;

        public PlaceCardOnSlotTutorial(ACard card, Slot slot, bool atTheBottom = false)
        {
            _card = card;
            _slot = slot;
            _atTheBottom = atTheBottom;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(_card, _slot, _atTheBottom);
            var location =
                new IView.CardLocation { Owner = _slot.Territory.Owner, SlotIndex = _slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().PlaceCardOnSlotFromDeck(_card, location, callback);
        }
    }
    
    public class PlaceCardOnFreeSlotTutorial : IEffectCommand
    {
        private ACard _card;
        private Territory _territory;
        private bool _atTheBottom;

        public PlaceCardOnFreeSlotTutorial(ACard card, Territory territory, bool atTheBottom = false)
        {
            _card = card;
            _territory = territory;
            _atTheBottom = atTheBottom;
        }

        public void Execute(PlayerAction _, Action callback)
        {
            var slot = GetFreeSlot(_territory);
            ServiceLocator.Get<IModel>().PlaceCardOnSlot(_card, slot, _atTheBottom);
            var location =
                new IView.CardLocation { Owner = slot.Territory.Owner, SlotIndex = slot.SlotIndexInTerritory };
            ServiceLocator.Get<IView>().PlaceCardOnSlotFromDeck(_card, location, callback);
        }
        
        private Slot GetFreeSlot(Territory territory)
        {
            foreach (var slot in territory.Slots)
            {
                if (!slot.PlacedCards.Any()) return slot;
            }

            return null;
        }
    }

    public class RemoveCardsFromTerritoryTutorial : IEffectCommand
    {
        private Territory _territory;

        public RemoveCardsFromTerritoryTutorial(Territory territory)
        {
            _territory = territory;
        }

        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>().RemoveCardsFromTerritory(_territory.Owner, null);

            if (ServiceLocator.Get<IModel>().GetPlayer(_territory.Owner).Territory.HasConstruction)
                ServiceLocator.Get<IModel>().RemoveConstruction(_territory.Owner);

            ServiceLocator.Get<IView>().DestroyInTerritory(_territory.Owner, callback, null);
        }
    }

    public class RemoveCardFromSlotTutorial : IEffectCommand
    {
        private Slot _slot;
        private int _cardIndex;

        public RemoveCardFromSlotTutorial(Slot slot, int cardIndex)
        {
            _slot = slot;
            _cardIndex = cardIndex;
        }

        public void Execute(PlayerAction action, Action callback)
        {
            ServiceLocator.Get<IModel>()
                .RemoveCardFromSlot(_slot.Territory.Owner, _slot.SlotIndexInTerritory, _cardIndex);
            var location = new IView.CardLocation
            {
                Owner = _slot.Territory.Owner,
                SlotIndex = _slot.SlotIndexInTerritory,
                CardIndex = _cardIndex
            };
            ServiceLocator.Get<IView>().KillPlacedCard(location, callback);
        }
    }
    
    public class DrawFixedCardsTutorial : IEffectCommand //esta mal hecho los 4 roban las mismas pero bueno
    {
        private IReadOnlyList<ACard> _cards;

        public DrawFixedCardsTutorial(IReadOnlyList<ACard> cards)
        {
            _cards = cards;
        }
        public void Execute(PlayerAction _, Action callback)
        {
            Dictionary<PlayerCharacter, IReadOnlyList<ACard>> cardsDrawn = new();
            foreach (var character in ServiceLocator.Get<IModel>().Config.TurnOrder)
            {
                if (character is PlayerCharacter.None) continue;
                var cards = ServiceLocator.Get<IModel>().PlayerDrawFixedCards(character, _cards);
                cardsDrawn.Add(character, new List<ACard>(cards));
            }

            ServiceLocator.Get<IView>().DrawCards(cardsDrawn, callback);
        }
    }
    
    public class PlaceConstructionTutorial : IEffectCommand
    {
        private Territory _territory;

        public PlaceConstructionTutorial(Territory territory)
        {
            _territory = territory;
        }
        
        public void Execute(PlayerAction _, Action callback)
        {
            ServiceLocator.Get<IModel>().PlaceConstruction(_territory.Owner, out var plant1, out var plant2);
    
            var location1 = new IView.CardLocation
            {
                SlotIndex = plant1.Slot.SlotIndexInTerritory, CardIndex = plant1.IndexInSlot,
                Owner = plant1.Slot.Territory.Owner
            };
    
            var location2 = new IView.CardLocation
            {
                SlotIndex = plant2.Slot.SlotIndexInTerritory, CardIndex = plant2.IndexInSlot,
                Owner = plant2.Slot.Territory.Owner
            };
    
            ServiceLocator.Get<IView>().PlaceConstruction(location1, location2, callback);
        }
    }
}