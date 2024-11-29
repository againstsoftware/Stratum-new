using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Tutorials/Overlord Tutorial")]
public class OverlordTutorial : ATutorialSequence
{
    public override PlayerCharacter LocalPlayer { get; protected set; } = PlayerCharacter.Overlord;

    [SerializeField] private ConstructionToken _constructionToken;
    [SerializeField] private List<ACard> _initialCards;
    [SerializeField] private List<ACard> _leashCard;



    [SerializeField] private TutorialDialogue[] _initialDialogues;
    //reparten tal
    [SerializeField] private TutorialDialogue[] _plantThenConstructDialogues;
    //jugador juega planta y construye
    //se destruye la construccion
    [SerializeField] private TutorialDialogue[] _constructionDestroyedDialogues;
    //reparten tal 
    [SerializeField] private TutorialDialogue[] _buildWithAnimalDialogues;
    //jugador vuelve a construir donde hay un animal
    [SerializeField] private TutorialDialogue[] _useLeashDialogues;
    //el jugador juega la correa y juega el ecosistema y no se destruye
    [SerializeField] private TutorialDialogue[] _endDialogues;
    
    
    public override IEnumerable<ITutorialElement> GetTutorialElements()
    {
        List<ITutorialElement> elements = new();
        var config = ServiceLocator.Get<IModel>().Config;

        var plant = config.GetPopulationCard(Population.Plant);
        var herbivore = config.GetPopulationCard(Population.Herbivore);
        var carnivore = config.GetPopulationCard(Population.Carnivore);
        var mushroom = config.Mushroom;
        var sagitarioTerritory = ServiceLocator.Get<IModel>().GetPlayer(PlayerCharacter.Sagitario).Territory;
        var fungalothTerritory = ServiceLocator.Get<IModel>().GetPlayer(PlayerCharacter.Fungaloth).Territory;
        var ygdraTerritory = ServiceLocator.Get<IModel>().GetPlayer(PlayerCharacter.Ygdra).Territory;
        var overlordTerritory = ServiceLocator.Get<IModel>().GetPlayer(PlayerCharacter.Overlord).Territory;

        
        var initialCards = new TutorialAction(false, new IEffectCommand[]
        {
            new EffectCommands.PlaceCardOnSlotTutorial(plant, overlordTerritory.Slots[0]),
            new EffectCommands.PlaceCardOnSlotTutorial(plant, sagitarioTerritory.Slots[1]),
            new EffectCommands.PlaceCardOnSlotTutorial(herbivore, sagitarioTerritory.Slots[3]),
            new EffectCommands.PlaceCardOnSlotTutorial(plant, fungalothTerritory.Slots[1]),
            new EffectCommands.PlaceCardOnSlotTutorial(plant, fungalothTerritory.Slots[2]),
            new EffectCommands.PlaceCardOnSlotTutorial(plant, ygdraTerritory.Slots[4]),
        });
        

        elements.AddRange(_initialDialogues);

        elements.Add(initialCards);

        elements.Add(DrawFixed(_initialCards));
        
        elements.AddRange(_plantThenConstructDialogues);
        
        
        var forcePlant = new List<PlayerAction>()
        {
            new PlayerAction(PlayerCharacter.Overlord, _initialCards[0], null, 1)
        };
        var playPlant =
            new TutorialAction(true, null, forcePlant, true);

        var forceConstruct = new List<PlayerAction>()
        {
            new PlayerAction(PlayerCharacter.Overlord, _constructionToken, null, 1)
        };
        var playConstruct =
            new TutorialAction(true, null, forceConstruct, true);

        
        elements.Add(playPlant);
        elements.Add(playConstruct);
        
        elements.Add(EcosystemAct());
        
        var destroyConstructionEffect =
            new TutorialAction(false, RulesCheck.CheckConstructions);
        elements.Add(destroyConstructionEffect);
        
        elements.AddRange(_constructionDestroyedDialogues);
        
        elements.Add(DrawFixed(_leashCard));
        
        var addForConstruct = new TutorialAction(false, new IEffectCommand[]
        {
            new EffectCommands.PlaceCardOnSlotTutorial(plant, sagitarioTerritory.Slots[0]),
        });
        
        elements.Add(addForConstruct);
        
        elements.AddRange(_buildWithAnimalDialogues);
        
        elements.Add(playConstruct);
        
        elements.AddRange(_useLeashDialogues);
        
        var forceLeash = new List<PlayerAction>()
        {
            new PlayerAction(PlayerCharacter.Overlord, _leashCard[0], null, 1)
        };
        var playLeash =
            new TutorialAction(true, null, forceLeash, true);

        elements.Add(playLeash);
        
        elements.Add(EcosystemAct());
        
        elements.AddRange(_endDialogues);
        
        return elements;
    }

    public override void OnTutorialFinished()
    {
        SceneTransition.Instance.TransitionToScene("MainMenu");
    }
}
