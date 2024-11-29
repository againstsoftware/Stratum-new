using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abacus : MonoBehaviour
{
    [SerializeField] private Transform _plantsColumn;
    [SerializeField] private Transform _herbsColumn;
    [SerializeField] private Transform _carnsColumn;
    [SerializeField] private Transform _growthsColumn;


    private class BeadColumn
    {
        public Bead[] Beads;
        public int Number = 0;
    }

    private readonly BeadColumn _plantsBeads = new();
    private readonly BeadColumn _herbsBeads = new();
    private readonly BeadColumn _carnsBeads = new();
    private readonly BeadColumn _growthsBeads = new();


    private void Awake()
    {
        InitBeads(_plantsColumn, _plantsBeads);
        InitBeads(_herbsColumn, _herbsBeads);
        InitBeads(_carnsColumn, _carnsBeads);
        InitBeads(_growthsColumn, _growthsBeads);
    }

    private void Start()
    {
        var model = ServiceLocator.Get<IModel>();
        model.Ecosystem.OnEcosystemChange += UpdateInfo;
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

    private void InitBeads(Transform parent, BeadColumn beadColumn)
    {
        beadColumn.Beads = new Bead[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            beadColumn.Beads[i] = parent.GetChild(i).GetComponent<Bead>();
            beadColumn.Beads[i].gameObject.SetActive(false);
        }
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