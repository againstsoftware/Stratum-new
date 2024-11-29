using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Abacus : MonoBehaviour
{
    [SerializeField] private Transform _plantsColumn;
    [SerializeField] private Transform _herbsColumn;
    [SerializeField] private Transform _carnsColumn;
    [SerializeField] private Transform _growthsColumn;

    [SerializeField] private TextMeshProUGUI _plantsText;
    [SerializeField] private TextMeshProUGUI _herbsText;
    [SerializeField] private TextMeshProUGUI _carnsText;
    [SerializeField] private TextMeshProUGUI _growthsText;

    private class BeadColumn
    {
        public Bead[] Beads;
        public int Number = 0;
        public TextMeshProUGUI Text;
    }

    private readonly BeadColumn _plantsBeads = new();
    private readonly BeadColumn _herbsBeads = new();
    private readonly BeadColumn _carnsBeads = new();
    private readonly BeadColumn _growthsBeads = new();


    private void Awake()
    {
        InitBeads(_plantsColumn, _plantsBeads, _plantsText);
        InitBeads(_herbsColumn, _herbsBeads, _herbsText);
        InitBeads(_carnsColumn, _carnsBeads, _carnsText);
        InitBeads(_growthsColumn, _growthsBeads, _growthsText);
    }

    private void Start()
    {
        ServiceLocator.Get<IModel>().Ecosystem.OnEcosystemChange += UpdateInfo;

        var comms = ServiceLocator.Get<ICommunicationSystem>();
        comms.OnLocalPlayerChange += SetLocalPlayer;
        SetLocalPlayer(comms.LocalPlayer, comms.Camera);
    }

    private void OnDestroy()
    {
        ServiceLocator.Get<IModel>().Ecosystem.OnEcosystemChange -= UpdateInfo;
    }

    private void SetLocalPlayer(PlayerCharacter localPlayer, Camera cam)
    {
        if (localPlayer is PlayerCharacter.None) return;

        var camPos = cam.transform.position;
        camPos.y = transform.position.y;
        transform.LookAt(camPos);
    }


    public void SetPlants(int amount)
    {
        StartCoroutine(SetBeads(amount, _plantsBeads));
    }

    public void SetHerbivores(int amount)
    {
        StartCoroutine(SetBeads(amount, _herbsBeads));
    }

    public void SetCarnivores(int amount)
    {
        StartCoroutine(SetBeads(amount, _carnsBeads));
    }

    public void SetGrowths(int amount)
    {
        StartCoroutine(SetBeads(amount, _growthsBeads));
    }

    private void InitBeads(Transform parent, BeadColumn beadColumn, TextMeshProUGUI text)
    {
        beadColumn.Beads = new Bead[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            beadColumn.Beads[i] = parent.GetChild(i).GetComponent<Bead>();
            beadColumn.Beads[i].gameObject.SetActive(false);
        }

        beadColumn.Text = text;
        beadColumn.Text.text = "0";
    }

    private IEnumerator SetBeads(int amount, BeadColumn beadColumn)
    {
        if (amount == beadColumn.Number) yield break;

        else if (amount > beadColumn.Number)
        {
            while (beadColumn.Number < amount)
            {
                var bead = beadColumn.Beads[beadColumn.Number];
                bool hasAppeared = false;
                bead.Appear(() => hasAppeared = true);
                yield return new WaitUntil(() => hasAppeared);
                beadColumn.Number++;
                beadColumn.Text.text = $"{beadColumn.Number}";
            }
        }
        
        else
        {
            while (beadColumn.Number > amount)
            {
                var bead = beadColumn.Beads[beadColumn.Number-1];
                bool hasDisappeared = false;
                bead.Disappear(() => hasDisappeared = true);
                yield return new WaitUntil(() => hasDisappeared);
                beadColumn.Number--;
                beadColumn.Text.text = $"{beadColumn.Number}";
            }
        }
    }
    
    private void UpdateInfo()
    {
        SetPlants(ServiceLocator.Get<IModel>().Ecosystem.Plants.Count);
        SetHerbivores(ServiceLocator.Get<IModel>().Ecosystem.Herbivores.Count);
        SetCarnivores(ServiceLocator.Get<IModel>().Ecosystem.Carnivores.Count);
        SetGrowths(ServiceLocator.Get<IModel>().Ecosystem.Growths);
    }
}