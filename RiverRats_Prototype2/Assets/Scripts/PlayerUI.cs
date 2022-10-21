using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Statistics.Distributions.Univariate;
using System.Linq;

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

    public void FindSuccessesInDeck()
    {
        //So first thing we need to do is create our fake players (with cards), and fakeDeck, fakeBoard, and fakeBurn
        List<Player> duplicatePlayers = Services.DealerManager.CreateDuplicatePlayers();
        List<CardType> fakeDeck = Services.DealerManager.CreateFakeDeck();
        List<CardType> fakeBoard = Services.TableManager.CreateFakeBoardCards();
        List<CardType> fakeBurn = Services.TableManager.CreateFakeBurnCards();

        for(int i = 0; i < duplicatePlayers.Count; i++)
        {
            AddFakeCardsToPlayer(duplicatePlayers[i], fakeBoard);
            AddFakeCardsToPlayer(duplicatePlayers[i], duplicatePlayers[i].holeCards);
        }

        //Debug.Log("Players = " + duplicatePlayers.Count);
        //Debug.Log("FakeDeck = " + fakeDeck.Count);
        //Debug.Log("fakeBoard = " + fakeBoard.Count);
        //Debug.Log("fakeBurn = " + fakeBurn.Count);

        //next thing we want to do is remove all these cards from the deck
        RemovePlayerCardsFromDeck(fakeDeck, duplicatePlayers);
        RemoveTableCardsFromDeck(fakeDeck, fakeBoard);
        RemoveTableCardsFromDeck(fakeDeck, fakeBurn);
        //Debug.Log("FakeDeck Has " + fakeDeck.Count + " and RealDeck has " + Services.DealerManager.cardsInDeck.Count);

        //then once we have an accurate account of what each player has, as well as how many cards are left in the deck, let's check the likelihood that player 0 will get a pair. 
        //so the plan is that I'm going to run through each of player O's hole cards
        //and compare them to each card in the deck
        //and if they match rank, then add that card to a list of potential success
        List<CardType> potentialSuccesses = new List<CardType>();

        foreach (CardType card in duplicatePlayers[0].holeCards)
        {
            for(int i = 0; i < fakeDeck.Count; i++)
            {
                if (fakeDeck[i].rank == card.rank)
                {
                    potentialSuccesses.Add(fakeDeck[i]);
                    Debug.Log("Adding " + fakeDeck[i].rank + " of " + fakeDeck[i].suit + "s");
                }
            }
        }
        Debug.Log("PotenialSuccesses = " + potentialSuccesses.Count);
        Debug.Log("Avg to get a pair for Player 0 = " + GetCardDrawProbability(fakeDeck.Count, potentialSuccesses.Count, 2, 1));

        //the above is all well and good, but these numbers and cards don't exist in a vacuum, and honestly if someone has a 15% chance to make a pair, but their playing against someone who has a full house, then their percent should be ZERO, since we want these numbers to reflect a percentage to win the hand. So what we need to do as well is determine
        //focusing on Player0 we would  need to know
        //1) What Poker Hand does each player currnently have
        //2) What are the order of ranks of the hand, and how many steps below are you from that hand. 
        //3) what type of hand would you need in order to beat the highest hand. 
        //4) what cards are left in the deck that would give you that hand
        //5) if that card came, would it inadvertently give somebody else the winning hand
        //6 if yes, then 0%, if no then hypergeometric distribution
        foreach(Player player in duplicatePlayers)
        {
            player.EvaluateMyHand(Services.TableManager.gameState);
        }
        List<Player> sortedPlayers = new List<Player>(duplicatePlayers.
                                  OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
                                  ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
                                  ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
        Debug.Log("Best Hand = Player " + sortedPlayers[0].SeatPos);


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
                for (int card = deck.Count; card --> 0;)
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

    public void AddFakeCardsToPlayer(Player fakePlayer, List<CardType> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            fakePlayer.Cards.Add(cards[i]);
        }
    }
}
