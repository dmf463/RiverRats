using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public enum PokerHand { HighCard = 0, OnePair, TwoPair, ThreeOfKind, Straight, Flush, FullHouse, FourOfKind, StraightFlush }


public struct HandValue
{
    public int Total { get; set; }
    public int HighCard { get; set; }
    public PokerHand PokerHand { get; set; }
    public string hexValue { get; set; }
}

public class HandEvaluator {

    private int spadeSum;
    private int heartSum;
    private int diamondSum;
    private int clubSum;
    private int straightCount;
    private int straightFlushCount;
    private List<CardType> incomingCards;
    private HandValue handValue;
    public bool isTesting;

    public HandEvaluator(List<CardType> sortedCards)
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
        incomingCards = sortedCards;
        Cards = sortedCards;
    }

    public HandEvaluator()
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
    }

    public void SetHandEvalutor(List<CardType> sortedCards)
    {
        incomingCards = sortedCards;
        Cards = sortedCards;
    }

    public void ResetHandEvaluator()
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
    }

    public HandValue HandValues
    {
        get { return handValue; }
        set { handValue = value; }
    }

    public List<CardType> Cards
    {
        get { return incomingCards; }
        set
        {
             GameState round = Services.TableManager.gameState;
            if (round == GameState.PreFlop || isTesting == true)
            {
                if (incomingCards.Count != 2)
                {
                    //Debug.Log("Pre-Flop Count (2) was Wrong, there are this many cards: " + incomingCards.Count);
                }
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                isTesting = false;
            }

            else if(round == GameState.Flop)
            {
                if (incomingCards.Count != 5)
                {
                    //Debug.Log("Flop Count (5) was Wrong, there are this many cards: " + incomingCards.Count);
                }
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
            }

            else if (round == GameState.Turn)
            {
                if (incomingCards.Count != 6)
                {
                    //Debug.Log("Turn Count (6) was Wrong, there are this many cards: " + incomingCards.Count);
                }
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
                incomingCards[5] = value[5];
            }

            else if(round == GameState.River)
            {
                if (incomingCards.Count != 7)
                {
                    //Debug.Log("River Count (7) was Wrong, there are this many cards: " + incomingCards.Count);
                }
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
                incomingCards[5] = value[5];
                incomingCards[6] = value[6];
            }
        }
    }

    public string DetermineHexValue(PokerHand hand, List<CardType> cards)
    {
        string handValue = "0x00";
        handValue += ((int)hand).ToString("X");
        for(int i = 0; i < cards.Count; i++)
        {
            ((int)cards[i].rank).ToString("X");
        }
        return handValue;
    }

    public string DetermineHexValue(PokerHand hand, CardType card)
    {
        string handValue = "0x00";
        handValue += ((int)hand).ToString("X");
        handValue += ((int)card.rank).ToString("X");
        while(handValue.Length < 10)
        {
            handValue += 0;
        }

        return handValue;
    }

    public void EvaluateHandAtPreFlop()
    {
        DeterminePreFlopHandScore();
    }

    public PokerHand EvaluateHandAtFlop()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtFlop())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtFlop())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseAtFlop())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtFlop())
        {
            return PokerHand.Flush;
        }
        else if (Straight())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtFlop())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairAtFlop())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtFlop())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard =  ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;
    }

    public PokerHand EvaluateHandAtTurn()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtTurn())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtTurn())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseAtTurn())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtTurn())
        {
            return PokerHand.Flush;
        }
        else if (Straight())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtTurn())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairAtTurn())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtTurn())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                             ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                             ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3)) +
                             ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 2)) +
                             ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 1));
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;
    }

    public PokerHand EvaluateHandAtRiver()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtRiver())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtRiver())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseRiver())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtRiver())
        {
            return PokerHand.Flush;
        }
        else if (Straight())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtRiver())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairRiver())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtRiver())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                             ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                             ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                             ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 2)) +
                             ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 1));
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;

    }

    private void getNumberOfSuit()
    {
        foreach (CardType card in Cards)
        {
            if(card.suit == SuitType.Spades)
            {
                spadeSum++;
            }
            else if(card.suit == SuitType.Hearts)
            {
                heartSum++;
            }
            else if(card.suit == SuitType.Diamonds)
            {
                diamondSum++;
            }
            else if(card.suit == SuitType.Clubs)
            {
                clubSum++;
            }
        }
    }

 /*
 * 
 * Everything below here is the logic for PRE-FLOP hand evaluation 
 * 
 */

    public void DeterminePreFlopHandScore()
    {
        float handScore = 0;
        float highCardValue = 0;
        switch (incomingCards[1].rank)
        {
            case RankType.Two:
                highCardValue = 1;
                break;
            case RankType.Three:
                highCardValue = 1.5f;
                break;
            case RankType.Four:
                highCardValue = 2;
                break;
            case RankType.Five:
                highCardValue = 2.5f;
                break;
            case RankType.Six:
                highCardValue = 3;
                break;
            case RankType.Seven:
                highCardValue = 3.5f;
                break;
            case RankType.Eight:
                highCardValue = 4;
                break;
            case RankType.Nine:
                highCardValue = 4.5f;
                break;
            case RankType.Ten:
                highCardValue = 5;
                break;
            case RankType.Jack:
                highCardValue = 6;
                break;
            case RankType.Queen:
                highCardValue = 7;
                break;
            case RankType.King:
                highCardValue = 8;
                break;
            case RankType.Ace:
                highCardValue = 10;
                break;
            default:
                break;
        }
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handScore = highCardValue * 2;
            if (handScore < 5) handScore = 5;
            handValue.PokerHand = PokerHand.OnePair;
        }
        else
        {
            handScore = highCardValue;
            handValue.PokerHand = PokerHand.HighCard;
        }
        if (incomingCards[0].suit == incomingCards[1].suit) handScore += 3;

        if ((int)incomingCards[1].rank - (int)incomingCards[0].rank == 0) handScore += 0;
        else if ((int)incomingCards[1].rank - (int)incomingCards[0].rank == 1) handScore -= 1;
        else if ((int)incomingCards[1].rank - (int)incomingCards[0].rank == 2) handScore -= 2;
        else if ((int)incomingCards[1].rank - (int)incomingCards[0].rank == 3) handScore -= 4;
        else if ((int)incomingCards[1].rank - (int)incomingCards[0].rank >= 4) handScore -= 5;

        if((int)incomingCards[0].rank < (int)RankType.Queen && 
           (int)incomingCards[1].rank < (int)RankType.Queen &&
           (int)incomingCards[1].rank - (int)incomingCards[0].rank <= 1)
        {
            handScore += 4;
        }
        handValue.Total = Mathf.RoundToInt(handScore + 0.1f);
        handValue.HighCard = (int)incomingCards[1].rank;
            
    }

   
    //public bool PocketPair()
    //{

    //    //if your preflop cards are the same, you have a pocket pair
    //    if(incomingCards[0].rank == incomingCards[1].rank)
    //    {
    //        handValue.Total = (int)incomingCards[0].rank * 2;
    //        handValue.HighCard = (int)incomingCards[1].rank;
    //        handValue.PokerHand = PokerHand.OnePair;
    //        return true;
    //    }
    //    return false;
    //}

    //public bool SuitedConnectors()
    //{
    //    //if you're cards fall into a sequence and have the same suit, then you have suitedConnectors
    //    if(incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
    //    {
    //        handValue.Total = (int)incomingCards[0].rank + (int)incomingCards[1].rank;
    //        handValue.HighCard = (int)incomingCards[1].rank;
    //        handValue.PokerHand = PokerHand.SuitedConnectors;
    //        return true;
    //    }
    //    return false;
    //}

    //public bool Connectors()
    //{
    //    //if you're cards fall into a sequence, then you have Connectors
    //    if (incomingCards[0].rank + 1 == incomingCards[1].rank)
    //    {
    //        handValue.Total = (int)incomingCards[0].rank + (int)incomingCards[1].rank;
    //        handValue.HighCard = (int)incomingCards[1].rank;
    //        handValue.PokerHand = PokerHand.Connectors;
    //        return true;
    //    }
    //    return false;
    //}


    /*
    * 
    * Everything below here is the logic for FLOP hand evaluation 
    * 
    */


    public bool OnePairAtFlop()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 4 high card
        //HighCard represents = after the main pair, the rest of the cards in this formula =
        //if 5 cards, (a x 16^5) + (b x 16^4) + (c x 16^3) + (d x 16^2) + (9 x 16^1);
        //if we're in a pair then we subtract the pair, take the remaining 3 highest cards
        //(a x 16^5) + (b x 16^4) + (c x 16^3) = highCard
        //possibly convert to hexidecimal
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 2;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) + 
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 4 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 2;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) + 
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 4 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 2;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 4)) + 
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 2 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 2;
            handValue.HighCard = ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 4)) + 
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;
    }

    public bool TwoPairAtFlop()
    {
        //if there are two pairs of cards that are the same then it's two pair

        //1, 2 && 3, 4
        if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[0].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtFlop()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        //need to do the ranked list of highCards here as well
        //except I go down 2 down the list (because 5 card hand).
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 4));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 4));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }

        return false;
    }

    //public bool StraightAtFlop()
    //{
    //    if( incomingCards[0].rank + 1 == incomingCards[1].rank &&
    //        incomingCards[1].rank + 1 == incomingCards[2].rank &&
    //        incomingCards[2].rank + 1 == incomingCards[3].rank &&
    //        incomingCards[3].rank + 1 == incomingCards[4].rank)
    //    {
    //        handValue.Total = (int)incomingCards[4].rank;
    //        handValue.PokerHand = PokerHand.Straight;
    //        return true;
    //    }
    //    if (incomingCards[0].rank == RankType.Two &&
    //        incomingCards[1].rank == RankType.Three &&
    //        incomingCards[2].rank == RankType.Four &&
    //        incomingCards[3].rank == RankType.Five &&
    //        incomingCards[4].rank == RankType.Ace)
    //    {
    //        handValue.Total = (int)incomingCards[3].rank;
    //        handValue.PokerHand = PokerHand.Straight;
    //        return true;
    //    }
    //    return false;
    //}

    public bool FlushAtFlop()
    {
        //Total should be highCard, and need to compare down list of all 5 cards. 
        if (spadeSum == 5 || heartSum == 5 || diamondSum == 5 || clubSum == 5)
        {
            handValue.Total = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) + 
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) + 
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) + 
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) + 
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.Flush;
            return true;
        }
        return false;
    }

    public bool FullHouseAtFlop()
    {
        //if there is a pair and trips , it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + 
                              (int)(incomingCards[1].rank) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              ((int)(incomingCards[4].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) + 
                              ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              (int)(incomingCards[3].rank) + 
                              (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtFlop()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[0].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;

    }

    public bool StraightFlushAtFlop()
    {
        if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit &&
            incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit &&
            incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit &&
            incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
        {
            handValue.Total = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.StraightFlush;
            handValue.hexValue = DetermineHexValue(PokerHand.Straight, incomingCards[4]);
            return true;
        }
        if (incomingCards[0].rank == RankType.Two && incomingCards[0].suit == incomingCards[1].suit &&
            incomingCards[1].rank == RankType.Three && incomingCards[1].suit == incomingCards[2].suit &&
            incomingCards[2].rank == RankType.Four && incomingCards[2].suit == incomingCards[3].suit &&
            incomingCards[3].rank == RankType.Five && incomingCards[3].suit == incomingCards[4].suit &&
            incomingCards[4].rank == RankType.Ace)
        {
            handValue.Total = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.StraightFlush;
            handValue.hexValue = DetermineHexValue(PokerHand.Straight, incomingCards[3]);
            return true;
        }
        return false;
    }


/*
* 
* Everything below here is the logic for TURN hand evaluation 
* 
*/

    public bool OnePairAtTurn()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 5 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 2;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) + 
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) + 
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 5 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 2;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 5 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 2;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 5 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 2;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //4, 5 with 3 high card
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 2;
            handValue.HighCard = ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;

    }

    public bool TwoPairAtTurn()
    {
        //if there are two pairs of cards that are the same then it's two pair

        //2, 3 && 4, 5
        if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 4, 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 4, 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 3, 4
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtTurn()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //3, 4, 5
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 3;
            handValue.HighCard = ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 3));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        return false;
    }

    //public bool StraightAtTurn()
    //{
    //    //need to check the straight, but because of the possibility of pairs in the middle of the straight I need to check each sequence
    //    //if the cards rank is equal to the following card + 1, then they are connectors and the "straight count" goes up
    //    //if the straight count reachs 4, that means there were at least 4 instances of connectors and there MUST be a straight
    //    //to check the high card, I do the same thing as the flush
    //    //EDGE CASE, THE WHEEL: A, 2, 3, 4, 5. A = 1 and RANK5 is the high card, because we don't know where the 5 is, but we know it's the highcard
    //    List<RankType> rankList = new List<RankType>();
    //    List<CardType> straightCards = new List<CardType>();
    //    for (int i = 0; i < incomingCards.Count; i++)
    //    {
    //        if (!rankList.Contains(incomingCards[i].rank))
    //        {
    //            rankList.Add(incomingCards[i].rank);
    //            straightCards.Add(incomingCards[i]);
    //        }
    //    }
    //    bool straightHit = false;
    //    CardType highCard = null;
    //    Debug.Assert(straightCards.Count != 0);
    //    for (int i = 0; i < straightCards.Count - 1; i++)
    //    {
    //        RankType card1Rank = straightCards[i].rank;
    //        RankType card2Rank = straightCards[i + 1].rank;
    //        if (card1Rank + 1 == card2Rank)
    //        {
    //            straightCount++;
    //            if (straightCount == 4)
    //            {
    //                straightHit = true;
    //            }
    //            if (straightCount >= 4)
    //            {
    //                highCard = straightCards[i + 1];
    //            }
    //        }
    //        else straightCount = 0;
    //    }

    //    //NEED TO MAKE A FRINGE CASE FOR CHECK FOR THE WHEEL (A, 2, 3, 4, 5)
    //    //will be ordered with as (2..3..4..5..A) with two cards inserted at any other point, but the order would make the straigh count = 0
    //    //so need to run a check AFTER the straight count has been recalculated to see if it's THE WHEEL
    //    #region Fringe case for The Wheel
    //    if (!straightHit)
    //    {
    //        int countForTheWheel = 0;
    //        foreach (CardType card in straightCards)
    //        {
    //            switch (card.rank)
    //            {
    //                case RankType.Two:
    //                    countForTheWheel++;
    //                    break;
    //                case RankType.Three:
    //                    countForTheWheel++;
    //                    break;
    //                case RankType.Four:
    //                    countForTheWheel++;
    //                    break;
    //                case RankType.Five:
    //                    countForTheWheel++;
    //                    break;
    //                case RankType.Ace:
    //                    countForTheWheel++;
    //                    break;
    //                default:
    //                    break;
    //            }
    //        }
    //        if (countForTheWheel == 5)
    //        {
    //            handValue.Total = 5;
    //            handValue.PokerHand = PokerHand.Straight;
    //            return true;
    //        }
    //        else return false;
    //    }
    //    #endregion

    //    if (straightHit)
    //    {
    //        handValue.Total = (int)highCard.rank;
    //        handValue.PokerHand = PokerHand.Straight;
    //        return true;
    //    }
    //    return false;
    //}

    public bool FlushAtTurn()
    {
        //if there are at least 5 of the same suited cards, then the player has a flush
        if (spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            SuitType suitToCheck;
            List<CardType> flushCards = new List<CardType>();
            if (spadeSum >= 5) suitToCheck = SuitType.Spades;
            else if (heartSum >= 5) suitToCheck = SuitType.Hearts;
            else if (diamondSum >= 5) suitToCheck = SuitType.Diamonds;
            else suitToCheck = SuitType.Clubs;

            for (int i = 0; i < incomingCards.Count; i++)
            {
                if(incomingCards[i].suit == suitToCheck)
                {
                    flushCards.Add(incomingCards[i]);
                }
            }
            int listCount = flushCards.Count;
            handValue.Total = ((int)flushCards[listCount - 1].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)flushCards[listCount - 2].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)flushCards[listCount - 3].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)flushCards[listCount - 4].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)flushCards[listCount - 5].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.Flush;
            return true;
        }
        return false;
    }

    public bool FullHouseAtTurn()
    {
        //if there is a pair and trips within the 7 cards, it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total =  (int)(incomingCards[0].rank) +
                               (int)(incomingCards[1].rank) +
                              ((int)(incomingCards[2].rank) * 5) +
                              ((int)(incomingCards[3].rank) * 5) +
                              ((int)(incomingCards[4].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 3 = 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total =  (int)(incomingCards[0].rank) +
                               (int)(incomingCards[1].rank) +
                              ((int)(incomingCards[3].rank) * 5) +
                              ((int)(incomingCards[4].rank) * 5) + 
                              ((int)(incomingCards[5].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2, 3 = 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total =  (int)(incomingCards[1].rank) + 
                               (int)(incomingCards[2].rank) +
                              ((int)(incomingCards[3].rank) * 5) +
                              ((int)(incomingCards[4].rank) * 5) +
                              ((int)(incomingCards[5].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) + 
                              ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                               (int)(incomingCards[3].rank) + 
                               (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) +
                              ((int)(incomingCards[1].rank) * 5) +
                              ((int)(incomingCards[2].rank) * 5) +
                               (int)(incomingCards[4].rank) + 
                               (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)(incomingCards[1].rank) * 5) +
                              ((int)(incomingCards[2].rank) * 5) +
                              ((int)(incomingCards[3].rank) * 5) + 
                               (int)(incomingCards[4].rank) + 
                               (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtTurn()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        //2, 3, 4, 5
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[2].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 4;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;
    }

    public bool StraightFlushAtTurn()
    {
        //if you have more than 5 of one suit
        //check cards of that suit
        //if they are consecutive
        //grats, you have a straight flush

        if(spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            CardType highCard = null;
            List<CardType> straightFlushCards = new List<CardType>();
            SuitType suitToCheck;
            if (spadeSum >= 5) suitToCheck = SuitType.Spades;
            else if (heartSum >= 5) suitToCheck = SuitType.Hearts;
            else if (diamondSum >= 5) suitToCheck = SuitType.Diamonds;
            else suitToCheck = SuitType.Clubs;
            for (int i = 0; i < incomingCards.Count; i++)
            {
                if(incomingCards[i].suit == suitToCheck)
                {
                    straightFlushCards.Add(incomingCards[i]);
                }
            }
            for (int i = 0; i < straightFlushCards.Count - 1; i++)
            {
                if (straightFlushCards[i].rank + 1 == straightFlushCards[i + 1].rank)
                {
                    straightFlushCount++;
                    highCard = straightFlushCards[i + 1];
                }

            }
            if (straightFlushCount >= 4)
            {
                handValue.Total = (int)highCard.rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else return false;
        }
        return false;
    }


/*
* 
* Everything below here is the logic for RIVER and ShowDown hand evaluation 
* 
*/

    public bool OnePairAtRiver()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 6 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 2;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 6 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 2;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 6 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 2;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 6 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 2;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //4, 5 with 6 high card
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 2;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //5, 6 with 4 high card
        else if (incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 2;
            handValue.HighCard = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;
    }

    public bool TwoPairRiver()
    {
        //if there are two pairs of cards that are the same then it's two pair

        //3, 4 && 5, 6
        if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //2, 3 && 5, 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 5, 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 5, 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //2, 3 && 4, 5
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 4, 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 4, 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 3, 4
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 1));
            handValue.HighCard = handValue.Total;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtRiver()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[4].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[5].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //3, 4, 5
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 3;
            handValue.HighCard = ((int)incomingCards[6].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //4, 5, 6
        else if (incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 3;
            handValue.HighCard = ((int)incomingCards[3].rank * (int)Mathf.Pow(16, 5)) +
                                 ((int)incomingCards[2].rank * (int)Mathf.Pow(16, 4)) +
                                 ((int)incomingCards[1].rank * (int)Mathf.Pow(16, 3)) +
                                 ((int)incomingCards[0].rank * (int)Mathf.Pow(16, 2));
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        return false;
    }

    public bool Straight()
    {
        List<RankType> rankList = new List<RankType>();
        List<CardType> straightCards = new List<CardType>();
        for (int i = 0; i < incomingCards.Count; i++)
        {
            if (!rankList.Contains(incomingCards[i].rank))
            {
                rankList.Add(incomingCards[i].rank);
                straightCards.Add(incomingCards[i]);
            }
        }
        Debug.Assert(rankList.Count == straightCards.Count);
        bool straightHit = false;
        CardType highCard = null;
        Debug.Assert(straightCards.Count != 0);
        for (int i = 0; i < straightCards.Count - 1; i++)
        {
            RankType card1Rank = straightCards[i].rank;
            RankType card2Rank = straightCards[i + 1].rank;
            if (card1Rank + 1 == card2Rank)
            {
                straightCount++;
                if (straightCount == 4)
                {
                    straightHit = true;
                }
                if (straightCount >= 4)
                {
                    highCard = straightCards[i + 1];
                }
            }
            else straightCount = 0;
        }

        //NEED TO MAKE A FRINGE CASE FOR CHECK FOR THE WHEEL (A, 2, 3, 4, 5)
        //will be ordered with as (2..3..4..5..A) with two cards inserted at any other point, but the order would make the straigh count = 0
        //so need to run a check AFTER the straight count has been recalculated to see if it's THE WHEEL
        #region Fringe case for The Wheel
        if (!straightHit)
        {
            int countForTheWheel = 0;
            foreach (CardType card in straightCards)
            {
                switch (card.rank)
                {
                    case RankType.Two:
                        countForTheWheel++;
                        break;
                    case RankType.Three:
                        countForTheWheel++;
                        break;
                    case RankType.Four:
                        countForTheWheel++;
                        break;
                    case RankType.Five:
                        countForTheWheel++;
                        break;
                    case RankType.Ace:
                        countForTheWheel++;
                        break;
                    default:
                        break;
                }
            }
            if (countForTheWheel == 5)
            {
                handValue.Total = 5;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else return false;
        }
        #endregion

        if (straightHit)
        {
            handValue.Total = (int)highCard.rank;
            handValue.PokerHand = PokerHand.Straight;
            return true;
        }
        return false;
    }

    public bool FlushAtRiver()
    {
        //if there are at least 5 of the same suited cards, then the player has a flush
        if (spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            SuitType suitToCheck;
            List<CardType> flushCards = new List<CardType>();
            if (spadeSum >= 5) suitToCheck = SuitType.Spades;
            else if (heartSum >= 5) suitToCheck = SuitType.Hearts;
            else if (diamondSum >= 5) suitToCheck = SuitType.Diamonds;
            else suitToCheck = SuitType.Clubs;

            for (int i = 0; i < incomingCards.Count; i++)
            {
                if (incomingCards[i].suit == suitToCheck)
                {
                    flushCards.Add(incomingCards[i]);
                }
            }
            int listCount = flushCards.Count;
            handValue.Total = ((int)flushCards[listCount - 1].rank * (int)Mathf.Pow(16, 5)) +
                              ((int)flushCards[listCount - 2].rank * (int)Mathf.Pow(16, 4)) +
                              ((int)flushCards[listCount - 3].rank * (int)Mathf.Pow(16, 3)) +
                              ((int)flushCards[listCount - 4].rank * (int)Mathf.Pow(16, 2)) +
                              ((int)flushCards[listCount - 5].rank * (int)Mathf.Pow(16, 1));
            handValue.PokerHand = PokerHand.Flush;
            return true;
        }
        return false;
    }

    public bool FullHouseRiver()
    {
        //if there is a pair and trips within the 7 cards, it's a full house

        //0 = 1, 4 = 5 = 6
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total =  (int)(incomingCards[0].rank) + 
                               (int)(incomingCards[1].rank) + 
                              ((int)(incomingCards[4].rank) * 5) + 
                              ((int)(incomingCards[5].rank) * 5) + 
                              ((int)(incomingCards[6].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }

        //1 = 2, 4 = 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total =  (int)(incomingCards[1].rank) + 
                               (int)(incomingCards[2].rank) +
                              ((int)(incomingCards[4].rank) * 5) +
                              ((int)(incomingCards[5].rank) * 5) +
                              ((int)(incomingCards[6].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //2 = 3, 4 =  5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[2].rank) + 
                              (int)(incomingCards[3].rank) +
                              ((int)(incomingCards[4].rank) * 5) +
                              ((int)(incomingCards[5].rank) * 5) +
                              ((int)(incomingCards[6].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 3 = 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total =  (int)(incomingCards[0].rank) + 
                               (int)(incomingCards[1].rank) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              ((int)(incomingCards[4].rank) * 5) + 
                              ((int)(incomingCards[5].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2, 3 = 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total =  (int)(incomingCards[1].rank) + 
                               (int)(incomingCards[2].rank) +
                              ((int)(incomingCards[3].rank) * 5) +
                              ((int)(incomingCards[4].rank) * 5) +
                              ((int)(incomingCards[5].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 2 = 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + 
                              (int)(incomingCards[1].rank) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              ((int)(incomingCards[4].rank) * 5);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) + 
                              ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              (int)(incomingCards[3].rank) + 
                              (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //2 = 3 = 4, 5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)(incomingCards[2].rank) * 5) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              ((int)(incomingCards[4].rank) * 5) + 
                              (int)(incomingCards[5].rank) + 
                              (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              (int)(incomingCards[5].rank) + 
                              (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 5 = 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) + 
                              ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              (int)(incomingCards[5].rank) + 
                              (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              ((int)(incomingCards[3].rank) * 5) + 
                              (int)(incomingCards[4].rank) + 
                              (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)(incomingCards[0].rank) * 5) + 
                              ((int)(incomingCards[1].rank) * 5) + 
                              ((int)(incomingCards[2].rank) * 5) + 
                              (int)(incomingCards[4].rank) + 
                              (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtRiver()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        //2, 3, 4, 5
        //3, 4, 5, 6
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[2].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank && incomingCards[3].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 4;
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;
    }

    public bool StraightFlushAtRiver()
    {
        if (spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            CardType highCard = null;
            List<CardType> straightFlushCards = new List<CardType>();
            SuitType suitToCheck;
            if (spadeSum >= 5) suitToCheck = SuitType.Spades;
            else if (heartSum >= 5) suitToCheck = SuitType.Hearts;
            else if (diamondSum >= 5) suitToCheck = SuitType.Diamonds;
            else suitToCheck = SuitType.Clubs;
            for (int i = 0; i < incomingCards.Count; i++)
            {
                if (incomingCards[i].suit == suitToCheck)
                {
                    straightFlushCards.Add(incomingCards[i]);
                }
            }
            for (int i = 0; i < straightFlushCards.Count - 1; i++)
            {
                if (straightFlushCards[i].rank + 1 == straightFlushCards[i + 1].rank)
                {
                    straightFlushCount++;
                    highCard = straightFlushCards[i + 1];
                }

            }
            if (straightFlushCount >= 4)
            {
                handValue.Total = (int)highCard.rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
        }
        return false;
    }

}
