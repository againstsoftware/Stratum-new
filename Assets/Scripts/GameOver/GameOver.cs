
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
        if(Input.GetKeyDown(KeyCode.N)) OnGameOver(new [] { PlayerCharacter.Sagitario , PlayerCharacter.Ygdra});
        else if(Input.GetKeyDown(KeyCode.F)) OnGameOver(new [] { PlayerCharacter.Fungaloth});
        else if(Input.GetKeyDown(KeyCode.O)) OnGameOver(new [] { PlayerCharacter.Overlord});
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
