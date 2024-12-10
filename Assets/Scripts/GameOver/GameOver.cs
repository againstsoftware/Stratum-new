
using System;
using System.Linq;
using UnityEngine;

public class GameOver : MonoBehaviour, IGameOverService
{
    private void Start()
    {
        ServiceLocator.Get<IRulesSystem>().OnGameOver += OnGameOver;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A)) OnGameOver(new [] { PlayerCharacter.Sagitario , PlayerCharacter.Ygdra});
    }

    private void OnGameOver(PlayerCharacter[] winners)
    {
        var config = ServiceLocator.Get<IModel>().Config;

        foreach (var player in config.TurnOrder)
        {
            if(player is PlayerCharacter.None) continue;

            if (winners.Contains(player))
            {
                ServiceLocator.Get<IView>().GetViewPlayer(player).Win();
        
            }
            else
            {
                ServiceLocator.Get<IView>().GetViewPlayer(player).Die();
            }
        }
    }
}
