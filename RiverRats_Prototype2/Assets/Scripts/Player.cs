using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Playing, NotPlaying, Winner, Loser, Eliminated } //what is the player doing
public enum PlayerEmotion { Joyous, Happy, Amused, Content, Annoyed, Angry, OnTilt } //emotions of the player
public enum PlayerAction { Fold, Call, Raise, None }
public class Player
{
    public int SeatPos { get; set; } //Where are they sitting at the table
    public int ChipCount; //How many chips do they have?
    public int ChipCountToCheckWhenWinning; //At the time of winning, how many chips do they have?
    public int MaxAmountToWin = 0;
    public int maxChips;
    public bool playerIsAllIn = false; //CURRENTLY THESE HAVE NO REAL FUNCTION
    public int currentBet; //this is how much money they currently have bet in front of them
    public bool actedThisRound;
    public float rateOfReturn;
    public int amountToAward;
    public bool awardedThisRound = false;
    public int timesRaisedThisRound;
    public int lossCount = 0;
    public List<int> playersLostAgainst;
    public PlayerAction lastAction = PlayerAction.None;
    public HandEvaluator Hand { get; set; } //what is their Poker Hand, not their cards, their hand value
    public List<CardType> Cards = new List<CardType>(); //raw list of cards they're holding.
    public List<CardType> holeCards = new List<CardType>();

    public float HandStrength;
    public float percievedHandStrength;
    public PlayerEmotion PlayerEmotion; //Player's emotional state;
    public PlayerState PlayerState; //what's their current state in the game
    public int amountToRaise;

    public Player(int seatPos, int chipCount, PlayerEmotion emotion, PlayerState state) //strcut to build new players
    {
        SeatPos = seatPos;
        ChipCount = chipCount;
        PlayerEmotion = emotion;
        PlayerState = state;
        maxChips = Services.DealerManager.startingChipStack * 5;
    }

    public void Fold()
    {
        //Debug.Log("ENTERED FOLD FUNCTION");
        Cards.Clear();
        PlayerState = PlayerState.NotPlaying;
        lastAction = PlayerAction.Fold;
        Hand = null;
        Services.UIManager.TurnPlayerCardImageOff(Services.TableManager.playerDestinations[SeatPos]);
        Debug.Log("Player " + SeatPos + " folded!");

        //Debug.Log("Active player count = " + Services.DealerManager.ActivePlayerCount());
        if (Services.DealerManager.ActivePlayerCount() == 1)
        {
            Services.TableManager.gameState = GameState.CleanUp;
            for (int i = 0; i < Services.TableManager.players.Length; i++)
            {
                if (Services.TableManager.players[i].PlayerState == PlayerState.Playing)
                {
                    Services.DealerManager.everyoneFolded = true;
                    Services.TableManager.players[i].PlayerState = PlayerState.Winner;
                    Services.DealerManager.numberOfWinners = 1;
                    //Debug.Log("current chip stack is at " + chipCount);
                    Services.TableManager.players[i].ChipCountToCheckWhenWinning = Services.TableManager.players[i].ChipCount;
                    //Debug.Log("We are getting into the fold and the chipCountToCheckWhenWinning = " + ChipCountToCheckWhenWinning);
                    Services.TableManager.gameState = GameState.CleanUp;
                    Services.DealerManager.SetAwardPlayer_Amount();
                }
            }
        }
    }

    public void Call()
    {
        //Debug.Log("ENTERED CALL FUNCTION");
        if (ChipCount > 0)
        {
            //Debug.Log("currentBet - lastBet = " + (currentBet - Services.DealerManager.lastBet));
            int betToCall = Services.DealerManager.lastBet - currentBet;
            //Debug.Log("currentBet = " + currentBet + " and betToCall = " + betToCall);
            lastAction = PlayerAction.Call;
            if (ChipCount - betToCall <= 0)
            {
                playerIsAllIn = true;
                Debug.Log("Player " + SeatPos + " calls, going all in for " + ChipCount);
                currentBet = ChipCount + currentBet;
                Bet(ChipCount);
                //Services.DealerManager.lastBet = currentBet;
            }
            else
            {
                //Debug.Log("betToCall = " + betToCall);
                if (betToCall == 0)
                {
                    Debug.Log("player " + SeatPos + " Checks");
                }
                else
                {
                    Debug.Log("player " + SeatPos + " Calls"); 
                }
                Bet(betToCall);
                currentBet = betToCall + currentBet;
                //moneyCommitted += betToCall;
                Services.DealerManager.lastBet = currentBet;
                //Debug.Log("Player " + SeatPos + " called " + betToCall);
                //Debug.Log("and the pot is now at " + Table.instance.potChips);
            }
            actedThisRound = true;
        }
    }

    public void Raise()
    {
        if (ChipCount > 0)
        {
            //int raiseAmount = amountToRaise; //this is used when we're actually determining handStrength and really building the AI, but for now we'll just go directly to raising as a set amount.
            int raiseAmount = amountToRaise;
            lastAction = PlayerAction.Raise;
            timesRaisedThisRound++;
            Services.DealerManager.raisesInRound++;
            int betToRaise;
            int lastBet = Services.DealerManager.lastBet;
            betToRaise = lastBet + (raiseAmount - currentBet);
            int highestPlayerAmount = 0;
            for(int i = 0; i < Services.DealerManager.ActivePlayerCount(); i++)
            {
                if (Services.TableManager.players[i].SeatPos != SeatPos) 
                {
                    if(highestPlayerAmount < Services.TableManager.players[i].ChipCount)
                    {
                        highestPlayerAmount = Services.TableManager.players[i].ChipCount;
                    }      
                }
            }
            if(betToRaise >= highestPlayerAmount && highestPlayerAmount != 0)
            {
                betToRaise = highestPlayerAmount;
            }
            if (ChipCount - betToRaise <= 0)
            {
                playerIsAllIn = true;
                //Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
                Debug.Log("player " + SeatPos + " raises " + ChipCount);
                currentBet+= ChipCount; //made the change here
                Bet(ChipCount);
                Services.DealerManager.lastBet = currentBet;
            }
            else
            {
                if (Services.DealerManager.lastBet == 0)
                {
                    //Debug.Log("Saying Bet");
                    Debug.Log("player " + SeatPos + " bets " + betToRaise);
                }
                else
                {
                    //Debug.Log("Saying Raise");
                    Debug.Log("player " + SeatPos + " raises " + betToRaise);
                }
                Bet(betToRaise);
                currentBet = betToRaise + currentBet;
                Services.DealerManager.lastBet = currentBet;
                //Debug.Log("Player " + SeatPos + " raised!");
                //Debug.Log("and the pot is now at " + Services.TableManager.pot);
                //Debug.Log("and player " + SeatPos + " is now at " + ChipCount);
            }
            actedThisRound = true;
        }
    }

    public void Bet(int betAmount)
    {
        ChipCount -= betAmount;
        Services.TableManager.pot += betAmount;
    }

    public int DetermineRaiseAmount(Player player)
    {

        int raise = Services.PlayerBehaviour.DetermineRaiseAmount(player);
        return raise;
    }

    //this is the FCR decision and this is where we can adjust player types
    //we should go back to the generic one and make percentage variables that we can adjust in individual players
    public void FoldCallRaiseDecision(float returnRate, Player player)
    {
        if (Services.TableManager.gameState == GameState.PreFlop)
        {
            Services.PlayerBehaviour.Preflop_FCR_Neutral(player);
            Services.DealerManager.preFlopHandCount++;
            Services.DealerManager.accumulatedHS += HandStrength;
            Services.DealerManager.averageHS = (Services.DealerManager.accumulatedHS / Services.DealerManager.preFlopHandCount);
            Services.DealerManager.SetNextPlayer();
        }
        else
        {
            Debug.Log("player" + player.SeatPos + " has a returnRate of " + returnRate);
            Services.PlayerBehaviour.FCR_Neutral(player, returnRate);
            //Services.PlayerBehaviour.FCR(player);
            Services.DealerManager.SetNextPlayer();
        }
        Services.DealerManager.pauseAutomation = false;
    }

    public void EvaluateMyHand(GameState round) //the function to actually determine the strength of their hand
    {
        List<CardType> sortedCards = SortMyCards(Cards); //we have to sort their cards to pass it along
        HandEvaluator playerHand = new HandEvaluator(sortedCards); //we open a new evaluator
        switch (round) //depending on the round, the evaluation is different
        {
            case GameState.PreFlop:
                playerHand.EvaluateHandAtPreFlop();
                break;
            case GameState.Flop:
                playerHand.EvaluateHandAtFlop();
                break;
            case GameState.Turn:
                playerHand.EvaluateHandAtTurn();
                break;
            case GameState.River:
                playerHand.EvaluateHandAtRiver();
                break;
        }
        Hand = playerHand;
        //Debug.Log("Player" + SeatPos + " has " + Hand.HandValues.PokerHand);
    }
    
    public void SetMaxWinnings()
    {
        DealerManager dm = Services.DealerManager;
        MaxAmountToWin = ChipCount * Services.DealerManager.ActivePlayerCount();//Services.TableManager.players.Length;
        if (MaxAmountToWin > maxChips)
        {
            MaxAmountToWin = maxChips;
        }
        int chipsAtTable = 0;
        for(int i = 0; i < Services.TableManager.players.Length; i++)
        {
            chipsAtTable += Services.TableManager.players[i].ChipCount;
        }
        if(MaxAmountToWin > chipsAtTable)
        {
            MaxAmountToWin = chipsAtTable;
        }
    }

    public List<CardType> SortMyCards(List<CardType> cards) //have to sort the cards to evaluate them
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = cards;
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        return EvaluatedHand;
    }

    public void DetermineHandStrength(CardType myCard1, CardType myCard2)
    {
        if (Services.TableManager.gameState == GameState.PreFlop)
        {
            Services.DealerManager.StartCoroutine(DeterminePreFlopHandStrength(/*0.5f, */myCard1, myCard2));
        }
        else
        {
            Services.DealerManager.StartCoroutine(RunHandStrengthLoopAfterFlop(myCard1, myCard2, Services.DealerManager.ActivePlayerCount()));
        }
    }

    //this is actually not far from the truth of how to determine preflop hands
    //but I don't have the FCR decision set up to accomodate these handStrengths
    //shouldn't be too hard
    //this is essentially why they're always calling preflop
    IEnumerator DeterminePreFlopHandStrength(/*float time,*/ CardType myCard1, CardType myCard2)
    {
        //yield return new WaitForSeconds(time);
        Services.DealerManager.pauseAutomation = true;
        HandStrength = Hand.HandValues.Total;
        Debug.Log("Player " + SeatPos + " has a Pre-Flop HandStrength of " + HandStrength);
        Debug.Log("Player " + SeatPos + " has a " + holeCards[0].rank + " of " + holeCards[0].suit +
                                        " and a " + holeCards[1].rank + " of " + holeCards[1].suit);
        rateOfReturn = FindRateOfReturn();
        FoldCallRaiseDecision(rateOfReturn, this);
        yield break;
    }

    IEnumerator RunHandStrengthLoopAfterFlop(CardType myCard1, CardType myCard2, int activePlayers)
    {
        Services.DealerManager.pauseAutomation = true;
        //set up all my empty lists to use 
        List<CardType> testDeck = new List<CardType>();
        #region populatingTheDeck
        SuitType[] suits = new SuitType[4]
        {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
        };
        RankType[] ranks = new RankType[13]
        {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
        };

        foreach (SuitType suit in suits)
        {
            foreach (RankType rank in ranks)
            {
                testDeck.Add(new CardType(rank, suit));
            }
        }
        #endregion
        List<CardType> referenceDeck = new List<CardType>();
        referenceDeck.AddRange(testDeck);
        List<CardType> testBoard = new List<CardType>();
        List<Player> testPlayers = new List<Player>();
        List<List<CardType>> playerCards = new List<List<CardType>>();
        List<HandEvaluator> testEvaluators = new List<HandEvaluator>();
        for (int i = 0; i < activePlayers; i++)
        {
            testPlayers.Add(new Player(i, ChipCount, PlayerEmotion, PlayerState));
            playerCards.Add(new List<CardType>());
            testEvaluators.Add(new HandEvaluator());
        }
        Debug.Assert(testPlayers.Count == Services.DealerManager.ActivePlayerCount());
        float numberOfWins = 0;
        float handStrengthTestLoops = 0;
        while (handStrengthTestLoops < 100)
        {
            #region 10x For-Loop for Hand Strength
            for (int f = 0; f < 10; f++)
            {
                //clear everything
                //clear each players hands
                foreach (Player player in testPlayers)
                {
                    player.Hand = null;
                }
                //clear each players handEvaluators
                foreach (HandEvaluator eval in testEvaluators)
                {
                    eval.ResetHandEvaluator();
                }
                //clear the deck
                testDeck.Clear();
                //add the deck
                testDeck.AddRange(referenceDeck);
                Debug.Assert(testDeck.Count == 52);
                //clear the board
                testBoard.Clear();
                Debug.Assert(testBoard.Count == 0);
                //clear each players cardList
                foreach (List<CardType> cardList in playerCards)
                {
                    cardList.Clear();
                    Debug.Assert(cardList.Count == 0);
                }
                //Start simulating the game
                //remove my cards from the deck
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == myCard1.rank)
                    {
                        if (testDeck[i].suit == myCard1.suit)
                        {
                            testDeck.RemoveAt(i);
                            //Debug.Log("removing my cards " + testDeck[i].rank + " of " + testDeck[i].suit);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == myCard2.rank)
                    {
                        if (testDeck[i].suit == myCard2.suit)
                        {
                            testDeck.RemoveAt(i);
                            //Debug.Log("removing " + testDeck[i].rank + " of " + testDeck[i].suit);
                        }
                    }
                }
                Debug.Assert(testDeck.Count == 50);
                //remove the cards on the board from the deck and then add them to the fake board.
                foreach (CardType boardCard in Services.TableManager.board)
                {
                    testDeck.Remove(boardCard);
                    testBoard.Add(boardCard);
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == Services.TableManager.board[0].rank)
                    {
                        if (testDeck[i].suit == Services.TableManager.board[0].suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == Services.TableManager.board[1].rank)
                    {
                        if (testDeck[i].suit == Services.TableManager.board[1].suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == Services.TableManager.board[2].rank)
                    {
                        if (testDeck[i].suit == Services.TableManager.board[2].suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                //set THIS as test player0
                playerCards[0].Add(myCard1);
                playerCards[0].Add(myCard2);
                //Debug.Log("PlayerCards.count = " + playerCards.Count);
                //give two cards two each other testPlayer, and then remove those cards from the deck
                for (int i = 1; i < testPlayers.Count; i++)
                {
                    //for (int j = 0; j < 2; j++)
                    //{
                    //	int cardPos = Random.Range(0, testDeck.Count);
                    //	CardType cardType = testDeck[cardPos];
                    //	playerCards[i].Add(cardType);
                    //	testDeck.Remove(cardType);
                    //}
                    List<CardType> holeCards = GetPreFlopHand(testDeck);
                    foreach (CardType card in holeCards)
                    {
                        playerCards[i].Add(card);
                        testDeck.Remove(card);
                    }
                }
                //if we're on the flop, deal out two more card to the board
                //and take those from the deck
                if (Services.TableManager.board.Count == 3)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int cardPos = Random.Range(0, testDeck.Count);
                        CardType cardType = testDeck[cardPos];
                        testDeck.Remove(cardType);
                        testBoard.Add(cardType);
                    }
                }
                //if we're on the turn, only take out one more card from the deck to the board
                else if (Services.TableManager.board.Count == 4)
                {
                    int cardPos = Random.Range(0, testDeck.Count);
                    CardType cardType = testDeck[cardPos];
                    testDeck.Remove(cardType);
                    testBoard.Add(cardType);
                }
                //for each player, add the board cards
                //sort the hands
                //assign them an evaluator
                //set the evaluator
                //evaluate the hand
                //set the hand = to the evaluator
                for (int i = 0; i < playerCards.Count; i++)
                {
                    playerCards[i].AddRange(testBoard);
                    playerCards[i].Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
                    HandEvaluator testHand = testEvaluators[i];
                    testHand.SetHandEvalutor(playerCards[i]);
                    testHand.EvaluateHandAtRiver();
                    testPlayers[i].Hand = testHand;
                }
                //compare all test players and find the winner
                Services.DealerManager.EvaluatePlayersOnShowdown(testPlayers);
                //if testPlayer[0] (this player) wins, we notch up the win score
                if (testPlayers[0].PlayerState == PlayerState.Winner)
                {
                    float numberOfTestWinners = 0;
                    foreach (Player player in testPlayers)
                    {
                        if (player.PlayerState == PlayerState.Winner)
                        {
                            numberOfTestWinners++;
                        }
                        else
                        {
                            //Debug.Log("losing player had a " + player.Hand.HandValues.PokerHand);
                        }
                    }
                    numberOfWins += (1 / numberOfTestWinners);
                }
            }
            #endregion
            handStrengthTestLoops++;
            yield return null;
        }
        float tempHandStrength = numberOfWins / 1000f;
        //Debug.Log("Player " + SeatPos + " has a HandStrength of " + tempHandStrength + " and a numberOfWins of " + numberOfWins);
        //HandStrength = Mathf.Pow(tempHandStrength, (float)Services.DealerManager.ActivePlayerCount());
        HandStrength = tempHandStrength;
        Debug.Log("Player " + SeatPos + " has a HS of " + HandStrength);
        rateOfReturn = FindRateOfReturn();
        FoldCallRaiseDecision(rateOfReturn, this);
        yield break;
    }

    public float FindRateOfReturn()
    {
        //in this case, since we're going to do limit, the bet will always be the bigBlind;
        //else, we need to write another function that determines what the possible bet will be
        amountToRaise = DetermineRaiseAmount(this);
        float potSize = Services.TableManager.pot;
        float potOdds = amountToRaise / (amountToRaise + potSize);
        if (Services.TableManager.gameState > GameState.PreFlop) Debug.Assert(HandStrength <= 1);
        float returnRate = HandStrength / potOdds;
        return returnRate;
    }

    public List<CardType> GetPreFlopHand(List<CardType> testDeck)
    {
        List<CardType> deck = testDeck;
        List<CardType> holeCards = new List<CardType>();
        /*
         * so we take two cards at random from the testDeck
         * we evaluate that hand
         * if it's good return those two cards
         * if it's not, then return those two cards into the deck and do it again until you do
         */
        for (int i = 0; i < 2; i++)
        {
            int cardPos = Random.Range(0, testDeck.Count);
            CardType cardType = deck[cardPos];
            holeCards.Add(cardType);
        }
        holeCards.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        HandEvaluator eval = new HandEvaluator();
        eval.isTesting = true;
        eval.SetHandEvalutor(holeCards);
        eval.EvaluateHandAtPreFlop();

        if (eval.HandValues.Total < 5)
        {
            return GetPreFlopHand(testDeck);
        }
        else
        {
            eval.isTesting = false;
            return holeCards;
        }
    }
}
