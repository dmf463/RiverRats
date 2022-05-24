using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum RankType
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
}

public enum SuitType
{
    Clubs, Diamonds, Hearts, Spades
}



[Serializable]
public class CardType
{
    [SerializeField] public RankType rank { get; set; }
    [SerializeField] public SuitType suit { get; set; }

    public CardType(RankType _rank, SuitType _suit)
    {
        rank = _rank;
        suit = _suit;
    }
}
