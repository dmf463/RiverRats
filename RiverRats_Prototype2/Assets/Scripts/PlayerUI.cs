using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Statistics.Distributions.Univariate;

public class PlayerUI : MonoBehaviour
{
    public int numCardsInDeck = 36;
    public int numSuccessesinDeck = 4;
    public int numOfDrawsToMake = 2;
    public int successesInSample = 1;
    // Start is called before the first frame update
    void Start()
    {
        Services.PlayerUI = this;
        Debug.Log("Avg = " + GetCardDrawProbability(numCardsInDeck, numSuccessesinDeck, numOfDrawsToMake, successesInSample));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Ben says: pass in k for each value of successes you care about and probably sum them, so like if you needed one club for a flush in 2 draws you could call it with k=1 and k=2 since two clubs also gives you a flush, and sum those, vs if you needed 2 clubs then you'd just want k=2 etc
    public double GetCardDrawProbability(int cardsInDeck, int successes, int drawsNeeded, int sampleSuccess)
    {
        double probability;

        HypergeometricDistribution dist = new HypergeometricDistribution(cardsInDeck, successes, drawsNeeded);
        probability = dist.ProbabilityMassFunction(sampleSuccess) * 100;

        return probability;
    }

    public void FindSuccessesInDeck(Player targetPlayer)
    {
        //So first thing we need to do is create our fake players (with cards), and fakeDeck, fakeBoard, and fakeBurn
        List<Player> duplicatePlayers = Services.DealerManager.CreateDuplicatePlayers();
        List<CardType> fakeDeck = Services.DealerManager.CreateFakeDeck();
        List<CardType> fakeBoard = Services.TableManager.CreateFakeBoardCards();
        List<CardType> fakeBurn = Services.TableManager.CreateFakeBurnCards();

        Debug.Log("Players = " + duplicatePlayers.Count);
        Debug.Log("FakeDeck = " + fakeDeck.Count);
        Debug.Log("fakeBoard = " + fakeBoard.Count);
        Debug.Log("fakeBurn = " + fakeBurn.Count);

        //next thing we want to do is remove all these cards from the deck
        RemovePlayerCardsFromDeck(fakeDeck, duplicatePlayers);
        RemoveTableCardsFromDeck(fakeDeck, fakeBoard);
        RemoveTableCardsFromDeck(fakeDeck, fakeBurn);
        Debug.Log("FakeDeck Has " + fakeDeck.Count + " and RealDeck has " + Services.DealerManager.cardsInDeck.Count);
        
    }

    public void RemoveTableCardsFromDeck(List<CardType> deck, List<CardType> cards)
    {
        for(int tableCard = 0; tableCard < cards.Count; tableCard++)
        {
            for(int deckCard = 0; deckCard < deck.Count; deckCard++)
            {
                if (deck[deckCard].suit == cards[tableCard].suit)
                {
                    if (deck[deckCard].suit == cards[tableCard].suit)
                    {
                        deck.RemoveAt(deckCard);
                    }
                }
            }
        }
    }

    public void RemovePlayerCardsFromDeck(List<CardType> deck, List<Player> players)
    {
        int count = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].holeCards.Count != 0)
            {
                for (int card = 0; card < deck.Count; card++)
                {
                    for(int holeCard = 0; holeCard < 2; holeCard++)
                    {
                        if (deck[card].rank == players[i].holeCards[holeCard].rank)
                        {
                            if (deck[card].suit == players[i].holeCards[holeCard].suit)
                            {
                                deck.RemoveAt(card);
                                count++;
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Count = " + count);
    }
}
