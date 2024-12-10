using System;
using Unity.VisualScripting;
using UnityEngine;

public class LastPopulationTracker : MonoBehaviour
{
    [SerializeField] private Population _population;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _slotOffset;

    private Ecosystem _ecosystem;
    private TableCard _last;
    private Vector3 _destination;
    private bool _isTraveling;
    private Vector3 _defaultPos;


    private void Awake()
    {
        _defaultPos = transform.position;
    }

    private void Start()
    {
        _ecosystem = ServiceLocator.Get<IModel>().Ecosystem;
        _ecosystem.OnEcosystemChange += OnEcosystemChange;
    }

    private void Update()
    {
        if (!_isTraveling) return;

        if (Vector3.Distance(transform.position, _destination) < .01f) _isTraveling = false;
        
        else transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.deltaTime);

    }

    private TableCard GetLast() => _population switch
    {
        Population.Carnivore => _ecosystem.LastCarnivore,
        Population.Herbivore => _ecosystem.LastHerbivore,
        Population.Plant => _ecosystem.LastPlant,
        _ => throw new ArgumentOutOfRangeException()
    };

    private void OnEcosystemChange()
    {
        var newLast = GetLast();
        // if (newLast == _last) return;

        if (newLast is null) Travel(_defaultPos);

        else
        {
            _last = newLast;
            Travel(GetDestination(_last));
        }
    }

    private void Travel(Vector3 destination)
    {
        _isTraveling = true;
        _destination = destination;
        _destination.y = _defaultPos.y;
    }

    private Vector3 GetDestination(TableCard card)
    {
        var slotIndex = card.Slot.SlotIndexInTerritory;
        var character = card.Slot.Territory.Owner;
        var slotTransform = ServiceLocator.Get<IView>().GetViewPlayer(character).Territory.Slots[slotIndex].transform;
        var globalOffset = slotTransform.TransformPoint(_slotOffset) - slotTransform.position;
        return slotTransform.position + globalOffset;
    }
}