
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Deck")]
public class Deck : ScriptableObject
{
    [field:SerializeField] public PlayerCharacter Owner { get; private set; }

    [System.Serializable]
    private class CardAmount
    {
        public ACard Card;
        public int Amount;
    }
    
    public int Size
    {
        get
        {
            if (!_initialized) Initialize();
            return _size;
        }
    }
    
    
    [SerializeField] private CardAmount[] Cards;

    
    private int _size;
    private bool _initialized;
    private ACard[] _deck;
    private List<ACard> _shuffledDeck = new();
    
    public ACard DrawCard()
    {
        if (!_initialized) Initialize();
        
        if(_shuffledDeck.Count == 0) Shuffle();
        
        var card = _shuffledDeck[^1];
        _shuffledDeck.RemoveAt(_shuffledDeck.Count - 1);
        
        return card;
    }

    private void Initialize()
    {
        _initialized = true;
        _size = Cards.Aggregate(0, (total, current) => total + current.Amount);
        _deck = new ACard[_size];
        int i = 0;
        foreach (var c in Cards)
        {
            for (int j = 0; j < c.Amount; j++)
            {
                _deck[i] = c.Card;
                i++;
            }
        }
    }

    private void OnValidate()
    {
        _initialized = false;
    }
    
    private void Shuffle() 
    {
        _shuffledDeck = new(_deck);
        for (var i = 0; i < _shuffledDeck.Count - 1; ++i)
        {
            var r = ServiceLocator.Get<IRNG>().Range(0, _shuffledDeck.Count);
            (_shuffledDeck[i], _shuffledDeck[r]) = (_shuffledDeck[r], _shuffledDeck[i]);
        }
    }
}
