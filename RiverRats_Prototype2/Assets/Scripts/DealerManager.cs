using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * The idea of this script is that if it's the job of the dealer at the table, it should be in this script. That DOES mean that quite a bit of logic is going on in here, but that's fine. 
 */
public class DealerManager : MonoBehaviour
{
    public List<CardType> cardsInDeck; //this the is my deck of cards
    public List<CardType> cheatCards;
    public bool cheating = false;
    private TableManager table; //this is the table I'm playing on
    List<List<Player>> PlayerRank = new List<List<Player>>(); //holds our ranked players at the end of the hand
    public int numberOfWinners; //How many winners do we actually have in the round?
    private bool automate = false;
    public Player playerToAct;
    public int lastBet;
    public bool everyoneFolded;
    public int startingChipStack;
    public int raisesInRound;
    public int dealtCardIndex;
    public int EndOfHand_PotAmount;
    public bool pauseAutomation;
    public PokerHand minPostFlopHand = PokerHand.OnePair;
    public int minPreFlopHand = 10;
    public Player cheatingTarget;
    public int cheatCount = 0;
    public int talkCount = 0;
    public bool influencingTable = false;
    #region forcing card hands
    //private List<CardType> straightFlush;
    //private List<CardType> fullHouse;
    //private List<CardType> trips;
    #endregion
    //public bool roundFinished;
    public float preFlopHandCount = 0;
    public float accumulatedHS = 0;
    public float averageHS = 0;
    public int roundCount = 1;

    void Start()
    {
        pauseAutomation = false;
        InitializeDealer();

        #region creating fake card hands
        //string test = "";
        //straightFlush = new List<CardType> //player 0
        //{
        //    new CardType(RankType.Ace, SuitType.Spades),
        //    new CardType(RankType.King, SuitType.Spades),
        //    new CardType(RankType.Queen, SuitType.Spades),
        //    new CardType(RankType.Jack, SuitType.Spades),
        //    new CardType(RankType.Ten, SuitType.Spades),
        //    new CardType(RankType.Four, SuitType.Clubs),
        //    new CardType(RankType.Five, SuitType.Clubs)
        //};
        //for (int i = 0; i < straightFlush.Count; i++)
        //{
        //    test += ((int)straightFlush[i].rank).ToString("X");
        //}
        //Debug.Log("test = " + test);
        //fullHouse = new List<CardType> //player 2 and 3
        //{
        //    new CardType(RankType.Ace, SuitType.Spades),
        //    new CardType(RankType.Ace, SuitType.Hearts),
        //    new CardType(RankType.Ace, SuitType.Diamonds),
        //    new CardType(RankType.King, SuitType.Spades),
        //    new CardType(RankType.King, SuitType.Hearts),
        //    new CardType(RankType.Four, SuitType.Clubs),
        //    new CardType(RankType.Five, SuitType.Clubs)
        //};
        //trips = new List<CardType> //player 4
        //{
        //    new CardType(RankType.Ace, SuitType.Spades),
        //    new CardType(RankType.Ace, SuitType.Hearts),
        //    new CardType(RankType.Ace, SuitType.Diamonds),
        //    new CardType(RankType.King, SuitType.Hearts),
        //    new CardType(RankType.Four, SuitType.Clubs),
        //    new CardType(RankType.Five, SuitType.Clubs),
        //    new CardType(RankType.Three, SuitType.Diamonds)
        //};
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if(table.gameState != GameState.GameOver)
        {
            AutomateGame();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cheat_Flop();
        }
    }

    public void InitializeDealer() //Setting the table, and shuffling the deck
    {
        table = Services.TableManager;
        cheatCards = new List<CardType>();
        PopulateCardDeck();
        SetBlinds();
        dealtCardIndex = 0;
    }

    public void AutomateGame()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            automate = true;
        }
        if (automate)
        {
            if ((table.gameState == GameState.PreFlop ||
                table.gameState == GameState.Flop ||
                table.gameState == GameState.Turn ||
                table.gameState == GameState.River) &&
                playerToAct == null)
            {
                DealCards();
            }
            else if(table.gameState == GameState.CleanUp)
            {
                if(table.pot > 0)
                {
                    AwardPlayer();
                }
                else NextHand();
            }
            else if (playerToAct != null && !pauseAutomation)
            {
                ActionIsOnYou();
            }
        }
    }

    public void Cheat()
    {
        if (playerToAct == null & !influencingTable)
        {
            influencingTable = true;
            cheatCount++;
            if (table.gameState == GameState.PreFlop)
            {
                Cheat_PreFlop();
            }
            else if (table.gameState == GameState.Flop)
            {
                Cheat_Flop();
            }
            else if(table.gameState == GameState.Turn)
            {
                Cheat_TurnV2();
            }
            else if(table.gameState == GameState.River)
            {
                Cheat_RiverV2();
            }
        }
    }

    public void Cheat_PreFlop()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();
        int targetHand = minPreFlopHand;
        int currentHand = 0;
        int roundCount = 0;
        while (currentHand <= targetHand)
        {
            testPlayer.Cards.Clear();
            cheatCards.Clear();
            fakeDeck.Clear();
            for (int i = 0; i < table.players[seatPos].Cards.Count; i++)
            {
                CardType card = new CardType(table.players[seatPos].Cards[i].rank, table.players[seatPos].Cards[i].suit);
                testPlayer.Cards.Add(card);
            }
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType card = new CardType(cardsInDeck[i].rank, cardsInDeck[i].suit);
                fakeDeck.Add(card);

            }
            Debug.Log("testplayer.card = " + testPlayer.Cards.Count);
            Debug.Log("fakeDeck has this many cards " + fakeDeck.Count);
            for (int i = 0; i < 2; i++)
            {
                CardType card;
                int cardPos = Random.Range(0, fakeDeck.Count);
                card = fakeDeck[cardPos];
                fakeDeck.Remove(card);
                testPlayer.Cards.Add(card);
                cheatCards.Add(card);
            }
            testPlayer.EvaluateMyHand(GameState.PreFlop);
            currentHand = testPlayer.Hand.HandValues.Total;
            if (currentHand >= targetHand) break;
            else
            {
                Debug.Log("not good enough yet");
            }
            Debug.Log("Current hand = " + currentHand + " and targetHand = " + targetHand);
        }
        cheating = true;
        Debug.Log("Roundcount = " + roundCount);
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for (int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }
    
    public void Cheat_Flop()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();
        PokerHand targetHand = minPostFlopHand;
        PokerHand currentHand = PokerHand.HighCard;
        if(cheatingTarget.Hand.HandValues.PokerHand == PokerHand.OnePair & table.gameState == GameState.Flop)
        {
            currentHand = PokerHand.OnePair;
            targetHand = currentHand + 1;
        }
        if(table.gameState == GameState.Turn || table.gameState == GameState.River)
        {
            currentHand = cheatingTarget.Hand.HandValues.PokerHand;
            targetHand = currentHand + 1;
        }
        int roundCount = 0;
        bool bestHand = false;
        while (!bestHand)
        {
            foreach (Player fakePlayer in testPlayers)
            {
                fakePlayer.Cards.Clear();
            }
            cheatCards.Clear();
            fakeDeck.Clear();

            for (int playerPos = 0; playerPos < table.players.Length; playerPos++)
            {
                for (int cardPos = 0; cardPos < table.players[playerPos].Cards.Count; cardPos++)
                {
                    CardType card = new CardType(table.players[playerPos].Cards[cardPos].rank, table.players[playerPos].Cards[cardPos].suit);
                    testPlayers[playerPos].Cards.Add(card);
                }
            }

            for(int i = 0; i <cardsInDeck.Count; i++)
            {
                CardType card = new CardType(cardsInDeck[i].rank, cardsInDeck[i].suit);
                fakeDeck.Add(card);  
            }
            Debug.Log("testplayer.card = " + testPlayer.Cards.Count);
            Debug.Log("fakeDeck has this many cards " + fakeDeck.Count);
            roundCount++;
            if (table.gameState == GameState.Flop)
            {
                for (int i = 0; i < 3; i++)
                {
                    CardType card;
                    int cardPos = Random.Range(0, fakeDeck.Count);
                    card = fakeDeck[cardPos];
                    fakeDeck.Remove(card);
                    foreach (Player fakePlayer in testPlayers)
                    {
                        fakePlayer.Cards.Add(card);
                    }
                    cheatCards.Add(card);
                }
                foreach(Player fakePlayer in testPlayers)
                {
                    if (fakePlayer.PlayerState == PlayerState.Playing)
                    {
                        Debug.Log("Fakeplayer #" + fakePlayer.SeatPos + " has " + fakePlayer.Cards.Count + " cards");
                        fakePlayer.EvaluateMyHand(GameState.Flop);
                    }
                }
                for (int i = testPlayers.Count - 1; i >= 0; i--)
                {
                    if(testPlayers[i].PlayerState != PlayerState.Playing)
                    {
                        testPlayers.RemoveAt(i);
                    }
                }
                List<Player> sortedPlayers = new List<Player>(testPlayers.
                                         OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
                                         ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
                                         ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
                if (sortedPlayers[0].SeatPos == seatPos)
                {
                    bestHand = true;
                }
                else
                {
                    testPlayers.Clear();
                    testPlayers = CreateFakePlayers();
                }
            }
        }
        cheating = true;
        Debug.Log("Roundcount = " + roundCount);
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for(int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }

    public void Cheat_TurnV2()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();
       
        int roundCount = 0;
        for(int possibleCard = 0; possibleCard < cardsInDeck.Count; possibleCard++)
        {
            Debug.Log("possibleCard = " + possibleCard);
            Debug.Log("Cards in deck = " + cardsInDeck.Count); 
            foreach (Player fakePlayer in testPlayers)
            {
                fakePlayer.Cards.Clear();
            }
            cheatCards.Clear();
            fakeDeck.Clear();

            for (int playerPos = 0; playerPos < table.players.Length; playerPos++)
            {
                Debug.Log("outer loop");
                for (int playerCard = 0; playerCard < table.players[playerPos].Cards.Count; playerCard++)
                {
                    Debug.Log("inner loop");
                    CardType testCard = new CardType(table.players[playerPos].Cards[playerCard].rank, table.players[playerPos].Cards[playerCard].suit);
                    testPlayers[playerPos].Cards.Add(testCard);
                }
            }

            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType testCard = new CardType(cardsInDeck[i].rank, cardsInDeck[i].suit);
                fakeDeck.Add(testCard);
            }
            Debug.Log("testplayer.card = " + testPlayer.Cards.Count);
            Debug.Log("fakeDeck has this many cards " + fakeDeck.Count + " and we're pulling card number " + possibleCard);
            roundCount++;
            CardType card = cardsInDeck[possibleCard];
            fakeDeck.Remove(card);
            foreach (Player fakePlayer in testPlayers)
            {
                fakePlayer.Cards.Add(card);
            }
            cheatCards.Add(card);
            foreach (Player fakePlayer in testPlayers)
            {
                if (fakePlayer.PlayerState == PlayerState.Playing)
                {
                    Debug.Log("Fakeplayer #" + fakePlayer.SeatPos + " has " + fakePlayer.Cards.Count + " cards");
                    fakePlayer.EvaluateMyHand(GameState.Turn);
                    Debug.Log("DID WE MAKE IT HERE");
                }
            }
            for (int i = testPlayers.Count - 1; i >= 0; i--)
            {
                if (testPlayers[i].PlayerState != PlayerState.Playing)
                {
                    Debug.Log("Removing at " + i);
                    testPlayers.RemoveAt(i);
                }
            }
            List<Player> sortedPlayers = new List<Player>(testPlayers.
                                     OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
                                     ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
                                     ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
            Debug.Log("Sorted Player 0 = ");
            Debug.Log(sortedPlayers[0].SeatPos + " = " + seatPos);
            if (sortedPlayers[0].SeatPos == seatPos)
            {
                Debug.Log("HERE?");
                break;
            }
            //else if(sortedPlayers[0].Hand.HandValues.Total == sortedPlayers[1].Hand.HandValues.Total)
            //{
            //    Debug.Log("WHERE?");
            //    bestHand = true;
            //    break;
            //}
            else
            {
                Debug.Log("SO CONFUSED");
                testPlayers.Clear();
                testPlayers = CreateFakePlayers();
                cheatCards.Clear();
            }
        }
        Debug.Log("do we make it here");
        Debug.Log("CheatCardCount = " + cheatCards.Count);
        if (cheatCards.Count == 0)
        {
            Debug.Log("No possible card to cheat with");
            cheating = false;
        }
        else cheating = true;
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for (int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }

    public void Cheat_RiverV2()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();

        int roundCount = 0;
        for (int possibleCard = 0; possibleCard < cardsInDeck.Count; possibleCard++)
        {
            Debug.Log("possibleCard = " + possibleCard);
            Debug.Log("Cards in deck = " + cardsInDeck.Count);
            foreach (Player fakePlayer in testPlayers)
            {
                fakePlayer.Cards.Clear();
            }
            cheatCards.Clear();
            fakeDeck.Clear();

            for (int playerPos = 0; playerPos < table.players.Length; playerPos++)
            {
                Debug.Log("outer loop");
                for (int playerCard = 0; playerCard < table.players[playerPos].Cards.Count; playerCard++)
                {
                    Debug.Log("inner loop");
                    CardType testCard = new CardType(table.players[playerPos].Cards[playerCard].rank, table.players[playerPos].Cards[playerCard].suit);
                    testPlayers[playerPos].Cards.Add(testCard);
                }
            }

            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType testCard = new CardType(cardsInDeck[i].rank, cardsInDeck[i].suit);
                fakeDeck.Add(testCard);
            }
            Debug.Log("testplayer.card = " + testPlayer.Cards.Count);
            Debug.Log("fakeDeck has this many cards " + fakeDeck.Count + " and we're pulling card number " + possibleCard);
            roundCount++;
            CardType card = cardsInDeck[possibleCard];
            fakeDeck.Remove(card);
            foreach (Player fakePlayer in testPlayers)
            {
                fakePlayer.Cards.Add(card);
            }
            cheatCards.Add(card);
            foreach (Player fakePlayer in testPlayers)
            {
                if (fakePlayer.PlayerState == PlayerState.Playing)
                {
                    Debug.Log("Fakeplayer #" + fakePlayer.SeatPos + " has " + fakePlayer.Cards.Count + " cards");
                    fakePlayer.EvaluateMyHand(GameState.River);
                    Debug.Log("DID WE MAKE IT HERE");
                }
            }
            for (int i = testPlayers.Count - 1; i >= 0; i--)
            {
                if (testPlayers[i].PlayerState != PlayerState.Playing)
                {
                    Debug.Log("Removing at " + i);
                    testPlayers.RemoveAt(i);
                }
            }
            List<Player> sortedPlayers = new List<Player>(testPlayers.
                                     OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
                                     ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
                                     ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
            Debug.Log("Sorted Player 0 = ");
            Debug.Log(sortedPlayers[0].SeatPos + " = " + seatPos);
            if (sortedPlayers[0].SeatPos == seatPos)
            {
                Debug.Log("HERE?");
                break;
            }
            //else if (sortedPlayers[0].Hand.HandValues.Total == sortedPlayers[1].Hand.HandValues.Total)
            //{
            //    Debug.Log("WHERE?");
            //    bestHand = true;
            //    break;
            //}
            else
            {
                Debug.Log("SO CONFUSED");
                testPlayers.Clear();
                testPlayers = CreateFakePlayers();
                cheatCards.Clear();
            }
        }
        Debug.Log("do we make it here");
        Debug.Log("CheatCardCount = " + cheatCards.Count);
        if (cheatCards.Count == 0)
        {
            Debug.Log("No possible card to cheat with");
            cheating = false;
        }
        else cheating = true;
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for (int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }
    
    public void Cheat_Turn()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();

        PokerHand currentHand = cheatingTarget.Hand.HandValues.PokerHand;
        PokerHand targetHand = currentHand + 1;

        for(int i = 0; i < cardsInDeck.Count; i++)
        {
            for (int j = 0; j < table.players[seatPos].Cards.Count; j++)
            {
                CardType playerCard = new CardType(table.players[seatPos].Cards[j].rank, table.players[seatPos].Cards[j].suit);
                testPlayer.Cards.Add(playerCard);
            }
            for (int k = 0; k < cardsInDeck.Count; k++)
            {
                CardType deckCard = new CardType(cardsInDeck[k].rank, cardsInDeck[k].suit);
                fakeDeck.Add(deckCard);

            }
            CardType card = cardsInDeck[i];
            testPlayer.Cards.Add(card);
            cheatCards.Add(card);
            testPlayer.EvaluateMyHand(GameState.Turn);
            currentHand = testPlayer.Hand.HandValues.PokerHand;
            if (currentHand >= targetHand)
            {
                break;
            }
            else
            {
                testPlayer.Cards.Clear();
                cheatCards.Clear();
                fakeDeck.Clear();
            }
        }
        if (cheatCards.Count == 0)
        {
            Debug.Log("No possible card to cheat with");
            cheating = false;
        }
        else cheating = true;
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for (int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }

    public void Cheat_River()
    {
        int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
        cheatingTarget = table.players[seatPos];
        Debug.Log("Cheating with Player " + seatPos);
        List<Player> testPlayers = CreateFakePlayers();
        Player testPlayer = testPlayers[seatPos];
        List<CardType> fakeDeck = new List<CardType>();

        PokerHand currentHand = cheatingTarget.Hand.HandValues.PokerHand;
        PokerHand targetHand = currentHand + 1;

        for (int i = 0; i < 500; i++)
        {
            for (int j = 0; j < table.players[seatPos].Cards.Count; j++)
            {
                CardType playerCard = new CardType(table.players[seatPos].Cards[j].rank, table.players[seatPos].Cards[j].suit);
                testPlayer.Cards.Add(playerCard);
            }
            for (int k = 0; k < cardsInDeck.Count; k++)
            {
                CardType deckCard = new CardType(cardsInDeck[k].rank, cardsInDeck[k].suit);
                fakeDeck.Add(deckCard);

            }
            CardType card = cardsInDeck[Random.Range(0, cardsInDeck.Count)];
            testPlayer.Cards.Add(card);
            cheatCards.Add(card);
            testPlayer.EvaluateMyHand(GameState.River);
            currentHand = testPlayer.Hand.HandValues.PokerHand;
            if (currentHand >= targetHand) break;
            else
            {
                testPlayer.Cards.Clear();
                cheatCards.Clear();
                fakeDeck.Clear();
            }
        }
        if (cheatCards.Count == 0)
        {
            Debug.Log("No possible card to cheat with");
            cheating = false;
        }
        else cheating = true;
        Debug.Log("CheatCards.count = " + cheatCards.Count);
        for (int i = 0; i < cheatCards.Count; i++)
        {
            Debug.Log("Cheat cards " + i + " is a " + cheatCards[i].rank + " of " + cheatCards[i].suit + "s");
            TakeCheatCardFromDeck(cheatCards[i]);
        }
    }

    public List<Player> CreateFakePlayers()
    {
        List<Player> fakePlayers = new List<Player>();

        for(int i = 0; i < table.players.Length; i++)
        {
            fakePlayers.Add(new Player(i, 0, PlayerEmotion.Content, PlayerState.Playing));
        }
        for(int i = 0; i < table.players.Length; i++)
        {
            fakePlayers[i].PlayerState = table.players[i].PlayerState;
        }

        return fakePlayers;
    }

    public List<Player> CreateDuplicatePlayers()
    {
        List<Player> fakePlayers = new List<Player>();

        for (int i = 0; i < table.players.Length; i++)
        {
            fakePlayers.Add(new Player(i, 0, PlayerEmotion.Content, PlayerState.Playing));
        }
        for (int i = 0; i < table.players.Length; i++)
        {
            fakePlayers[i].PlayerState = table.players[i].PlayerState;
            fakePlayers[i].holeCards = CopyPlayerCards(table.players[i]);
        }

        return fakePlayers;
    }

    public List<CardType> CopyPlayerCards(Player player)
    {
        List<CardType> cards = new List<CardType>();

        for(int i = 0; i < 2; i++)
        {
            cards.Add(new CardType(player.holeCards[i].rank, player.holeCards[i].suit));
        }

        return cards;
    }

    public void DealCards() //happens on click of the button
    {
        /*Only deal cards if we have money from people && there is no current player that needs to act
         * Throughout this function, the first thing we do is check the game state
         * then before we deal out the flop, turn, and river, we burn a card, and add whatever we've dealt to the board
         * Pre-Flop and Flop are both more complicated case because they involve multiple cards
         */
        if (table.pot != 0 && playerToAct == null)
        {
            if (table.gameState == GameState.PreFlop)
            {
                //cycle through the players
                int i = SeatsAwayFromDealerAmongstLivePlayers(1 + dealtCardIndex); //) % ActivePlayerCount();
                //for (int i = 0; i < table.numActivePlayers; i++)
                //{
                if (table.players[i].PlayerState == PlayerState.Playing) //check if the player is still in the game
                {
                    if (table.players[i] == cheatingTarget)
                    {
                        CardType cheatCard = cheatCards[cheatingTarget.holeCards.Count];
                        //TakeCheatCardFromDeck(cheatCard);
                        table.players[i].Cards.Add(cheatCard);
                        table.players[i].holeCards.Add(cheatCard);
                        Services.UIManager.SetCardImage(table.playerDestinations[i], table.players[i].holeCards);
                    }
                    else
                    {
                        //Once we know the player is in the game, we grab two cards from the deck
                        //and pass them to the player, as well as our UI manager
                        //then we have the players look at the cards to evaluate their hands
                        //table.players[i].holeCards = new List<CardType> { TakeCardFromDeck(), TakeCardFromDeck() };
                        CardType drawnCard = TakeCardFromDeck();
                        table.players[i].Cards.Add(drawnCard);
                        table.players[i].holeCards.Add(drawnCard);
                        Services.UIManager.SetCardImage(table.playerDestinations[i], table.players[i].holeCards);
                    }
                }
                //}
                dealtCardIndex++;
                int cardCount = 0;
                for(int j = 0; j < table.players.Length; j++)
                {
                    if (table.players[j].PlayerState == PlayerState.Playing)
                    {
                        cardCount += table.players[j].holeCards.Count;
                        if(cardCount == Services.DealerManager.ActivePlayerCount() * 2)
                        {
                            foreach(Player player in table.players)
                            {
                                if (player.PlayerState == PlayerState.Playing)
                                {
                                    player.EvaluateMyHand(table.gameState);
                                }
                            }
                            dealtCardIndex = 0;
                            cheatingTarget = null;
                            BalanceTalkAndCheatScales();
                            cheating = false;
                            influencingTable = false;
                            cheatCards.Clear();
                            ChooseNextPlayer();
                        }
                    }
                }
            }
            else if (table.gameState == GameState.Flop)
            {
                //we do the same thing as the above for the Flop, except this time we burn a card first
                //the only difference now is that we send the cards to the flop outside the For Loop
                if(dealtCardIndex == 0)
                {
                    BurnCard();
                }
                else
                {
                    if (cheating)
                    {
                        CardType cheatCard = cheatCards[dealtCardIndex - 1];
                        //TakeCheatCardFromDeck(cheatCard);
                        List<CardType> flopCard = new List<CardType> { cheatCard };
                        AddCardToBoard(flopCard);
                        Services.UIManager.SetCardImage(Destination.flop, flopCard);
                        for (int j = 0; j < table.numActivePlayers; j++)
                        {
                            if (table.players[j].PlayerState == PlayerState.Playing)
                            {
                                GiveCardToPlayer(table.playerDestinations[j], flopCard);
                            }
                        }
                    }
                    else
                    {
                        CardType drawnCard = TakeCardFromDeck();
                        List<CardType> flopCard = new List<CardType> { drawnCard };
                        AddCardToBoard(flopCard);
                        Services.UIManager.SetCardImage(Destination.flop, flopCard);
                        for (int j = 0; j < table.numActivePlayers; j++)
                        {
                            if (table.players[j].PlayerState == PlayerState.Playing)
                            {
                                GiveCardToPlayer(table.playerDestinations[j], flopCard);
                            }
                        }
                    }
                }
                dealtCardIndex++;
                if(dealtCardIndex == 4)
                {
                    cheatCards.Clear();
                    cheating = false;
                    for(int k = 0; k < table.numActivePlayers; k++)
                    {
                        if(table.players[k].PlayerState == PlayerState.Playing)
                        {
                            table.players[k].EvaluateMyHand(table.gameState);
                            table.players[k].actedThisRound = false;
                        }
                    }
                    dealtCardIndex = 0;
                    cheatingTarget = null;
                    BalanceTalkAndCheatScales();
                    cheating = false;
                    influencingTable = false;
                    cheatCards.Clear();
                    ChooseNextPlayer();
                    Services.PlayerUI.FindSuccessesInDeck();
                }
            }
            else if (table.gameState == GameState.Turn)
            {
                //the turn and the river are both more simplified versions of the above
                //since they represent only one card
                BurnCard();
                if (cheating)
                {
                    CardType cheatCard = cheatCards[0];
                    //TakeCheatCardFromDeck(cheatCard);
                    List<CardType> turn = new List<CardType> { cheatCard };
                    AddCardToBoard(turn);
                    Services.UIManager.SetCardImage(Destination.turn, turn);
                    for (int j = 0; j < table.numActivePlayers; j++)
                    {
                        if (table.players[j].PlayerState == PlayerState.Playing)
                        {
                            GiveCardToPlayer(table.playerDestinations[j], turn);
                            table.players[j].EvaluateMyHand(table.gameState);
                            table.players[j].actedThisRound = false;
                        }
                    }
                }
                else
                {
                    List<CardType> turn = new List<CardType> { TakeCardFromDeck() };
                    AddCardToBoard(turn);
                    for (int i = 0; i < table.numActivePlayers; i++)
                    {
                        if (table.players[i].PlayerState == PlayerState.Playing)
                        {
                            GiveCardToPlayer(table.playerDestinations[i], turn);
                            table.players[i].EvaluateMyHand(table.gameState);
                            table.players[i].actedThisRound = false;
                        }
                    }
                    Services.UIManager.SetCardImage(Destination.turn, turn);
                }
                dealtCardIndex = 0;
                cheatingTarget = null;
                BalanceTalkAndCheatScales();
                cheating = false;
                influencingTable = false;
                cheatCards.Clear();
                ChooseNextPlayer();
            }
            else if (table.gameState == GameState.River)
            {
                BurnCard();
                if (cheating)
                {
                    CardType cheatCard = cheatCards[0];
                    //TakeCheatCardFromDeck(cheatCard);
                    List<CardType> river = new List<CardType> { cheatCard };
                    AddCardToBoard(river);
                    Services.UIManager.SetCardImage(Destination.river, river);
                    for (int j = 0; j < table.numActivePlayers; j++)
                    {
                        if (table.players[j].PlayerState == PlayerState.Playing)
                        {
                            GiveCardToPlayer(table.playerDestinations[j], river);
                            table.players[j].EvaluateMyHand(table.gameState);
                            table.players[j].actedThisRound = false;
                        }
                    }
                    Services.UIManager.SetCardImage(Destination.river, river);
                }
                else
                {
                    List<CardType> river = new List<CardType> { TakeCardFromDeck() };
                    AddCardToBoard(river);
                    for (int i = 0; i < table.numActivePlayers; i++)
                    {
                        if (table.players[i].PlayerState == PlayerState.Playing)
                        {
                            GiveCardToPlayer(table.playerDestinations[i], river);
                            #region this is me setting hands at the end
                            //INSERTING BULLSHIT DEBUG CODE HERE
                            //if(i == 2 || i == 0)
                            //{
                            //    table.players[i].Cards.Clear();
                            //    table.players[i].Cards = straightFlush;
                            //}
                            //else if(i == 4)
                            //{
                            //    table.players[i].Cards.Clear();
                            //    table.players[i].Cards = fullHouse;
                            //}
                            //else if(i == 500)
                            //{
                            //    table.players[i].Cards.Clear();
                            //    table.players[i].Cards = fullHouse;
                            //}
                            //else if(i == 3)
                            //{
                            //    table.players[i].Cards.Clear();
                            //    table.players[i].Cards = trips;
                            //}
                            //END BULLSHIT DEBUG CODE
                            #endregion
                            table.players[i].EvaluateMyHand(table.gameState);
                            table.players[i].actedThisRound = false;
                        }
                    }
                    Services.UIManager.SetCardImage(Destination.river, river);
                }
                dealtCardIndex = 0;
                cheatingTarget = null;
                BalanceTalkAndCheatScales();
                cheating = false;
                influencingTable = false;
                cheatCards.Clear();
                ChooseNextPlayer();
                if (playerToAct == null)
                {
                    ChooseWinner();
                }
            }
        }
    }

    public void ChooseNextPlayer()
    {
        if (!OnlyAllInPlayersLeft())
        {
            if (table.gameState == GameState.PreFlop)
            {
                if (ActivePlayerCount() == 2)
                {
                    if (table.gameState == GameState.PreFlop)
                    {
                        playerToAct = PlayerSeatsAwayFromDealerAmongstLivePlayers(0);
                        playerToAct.decisionState = PlayerDecisionState.ToCall;
                    }
                    else
                    {
                        playerToAct = PlayerSeatsAwayFromDealerAmongstLivePlayers(1);
                        playerToAct.decisionState = PlayerDecisionState.ToCall;
                    }
                }
                else
                {
                    playerToAct = PlayerSeatsAwayFromDealerAmongstLivePlayers(3);
                    playerToAct.decisionState = PlayerDecisionState.ToCall;
                }
            }
            else
            {
                playerToAct = FindFirstPlayerToAct(1);
                playerToAct.decisionState = PlayerDecisionState.ToCall;
            }
        }
        else
        {
            table.gameState++;
        }
    }

    public void ChooseWinner()
    {
        if (table.gameState == GameState.CleanUp)
        {
            DetermineWinner();
            RevealCards();
            foreach (Player player in table.players)
            {
                if (player.PlayerState == PlayerState.Winner)
                {
                    Debug.Log("player " + (player.SeatPos) +
                              " is the " + player.PlayerState +
                              " with a " + player.Hand.HandValues.PokerHand +
                              " with a highCard of " + player.Hand.HandValues.HighCard +
                              " and a handTotal of " + player.Hand.HandValues.Total);
                }
            }
            SetAwardPlayer_Amount();
        }
    }

    public void BalanceTalkAndCheatScales()
    {
        if (!cheating)
        {
            if(cheatCount != 0) cheatCount--;
        }
        if (!influencingTable)
        {
            if(talkCount != 0) talkCount--;
        }
    }

    public void AwardPlayer()
    {
        if (table.gameState == GameState.CleanUp && table.pot != 0)
        {
            int seatPos = 0;
            if (!automate)
            {
                seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
            }
            else
            {
                for(int i = 0; i < table.players.Length; i++)
                {
                    if(table.players[i].amountToAward > 0)
                    {
                        seatPos = i;
                    }
                }
            }

            if (table.players[seatPos].amountToAward != 0)
            {
                table.players[seatPos].ChipCount += table.players[seatPos].amountToAward;
                table.pot -= table.players[seatPos].amountToAward;
                table.players[seatPos].amountToAward = 0;
            }
            if (table.pot == 0)
            {
                ChangeEmotions();
                for (int i = 0; i < table.players.Length; i++)
                {
                    if (table.players[i].ChipCount <= 0)
                    {
                        table.players[i].PlayerState = PlayerState.Eliminated;
                    }
                }
            }
        }
    }

    public void ActionIsOnYou() //So basically this whole thing is just "Collect Money from the Active Player. Then SetNextPlayer.
    {
        if (playerToAct != null)
        {
            Services.PlayerUI.FindSuccessesInDeck();
            playerToAct.DetermineHandStrength(playerToAct.holeCards[0], playerToAct.holeCards[1]);
        }
    }

    public void NextHand() //click this and it resets everything for the next round; 
    {
        if (table.gameState == GameState.CleanUp && table.pot == 0)
        {
            roundCount++;
            cardsInDeck.Clear();
            PopulateCardDeck();
            table.gameState = GameState.PreFlop;
            table.burn.Clear();
            table.board.Clear();
            numberOfWinners = 0;
            lastBet = 0;
            PlayerRank.Clear();
            raisesInRound = 0;
            dealtCardIndex = 0;
            cheating = false;
            influencingTable = false;
            cheatCards.Clear();
            int eliminatedPlayers = 0;
            for (int i = 0; i < table.players.Length; i++)
            {
                table.players[i].Cards.Clear();
                table.players[i].holeCards.Clear();
                table.players[i].currentBet = 0;
                table.players[i].timesRaisedThisRound = 0;
                table.players[i].lastAction = PlayerAction.None;
                table.players[i].actedThisRound = false;
                table.players[i].awardedThisRound = false;
                table.players[i].playerIsAllIn = false;
                table.players[i].amountToRaise = 0;
                table.players[i].HandStrength = 0;
                EndOfRoundRuleChecks(i);
                if (table.players[i].PlayerState != PlayerState.Eliminated)
                {
                    table.players[i].PlayerState = PlayerState.Playing;
                    table.players[i].decisionState = PlayerDecisionState.None;
                    //Debug.Log("Player " + table.players[i].SeatPos + " has " + table.players[i].ChipCount + " at the top of the hand");
                }
                else
                {
                    eliminatedPlayers++;
                    table.players[i].decisionState = PlayerDecisionState.Eliminated;
                }
            }
            foreach(Player player in table.players)
            {
                player.SetMaxWinnings();
            }
            if (eliminatedPlayers == 4)
            {
                table.gameState = GameState.GameOver;
                Debug.Log("GAME OVER");
                //for (int i = 0; i < table.players.Length; i++)
                //{
                //    if (table.players[i].PlayerState == PlayerState.Playing)
                //    {
                //        Debug.Log("The winner is player " + i + " and they have a chipstack of " + table.players[i].ChipCount);
                //    }
                //}
            }
            else
            {
                Services.UIManager.ResetAllCardsImages();
                PassDealerButton();
                SetBlinds();
                playerToAct = null;
            }
        }
    }

    public void EndOfRoundRuleChecks(int seatPos)
    {
        //checking short to big
        if (table.players[seatPos].ChipCount < 500) table.players[seatPos].shortStack = true;
        else if (table.players[seatPos].shortStack)
        {
            if (table.players[seatPos].ChipCount > 6000) table.players[seatPos].bigStack = true;
        }

        //checking big to short
        if (table.players[seatPos].ChipCount > 6000) table.players[seatPos].bigStack = true;
        else if (table.players[seatPos].bigStack)
        {
            if (table.players[seatPos].ChipCount < 500) table.players[seatPos].shortStack = true;
        }
    }

    public void DetermineWinner()
    {
        List<Player> playersInHand = new List<Player>();
        for (int i = 0; i < table.players.Length; i++)
        {
            if (table.players[i].PlayerState == PlayerState.Playing)
            {
                playersInHand.Add(table.players[i]);
            }
        }
        EvaluatePlayersOnShowdown(playersInHand);
    }

    public void EvaluatePlayersOnShowdown(List<Player> playersToEvaluate)
    {
        List<Player> sortedPlayers = new List<Player>(playersToEvaluate.
            OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
            ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
            ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));

        sortedPlayers[0].PlayerState = PlayerState.Winner;

        PlayerRank = new List<List<Player>>();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (PlayerRank.Count == 0)
            {
                PlayerRank.Add(new List<Player>() { sortedPlayers[i] });
            }
            else if (sortedPlayers[i].Hand.HandValues.PokerHand == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.PokerHand)
            {
                if (sortedPlayers[i].Hand.HandValues.Total == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.Total)
                {
                    if (sortedPlayers[i].Hand.HandValues.HighCard == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.HighCard)
                    {
                        PlayerRank[PlayerRank.Count - 1].Add(sortedPlayers[i]);
                    }
                    else
                    {
                        PlayerRank.Add(new List<Player>() { sortedPlayers[i] });
                    }
                }
                else
                {
                    PlayerRank.Add(new List<Player>() { sortedPlayers[i] });
                }
            }
            else
            {
                PlayerRank.Add(new List<Player>() { sortedPlayers[i] });
            }
        }
        for (int i = 0; i < PlayerRank.Count; i++)
        {
            if (i == 0)
            {
                foreach (Player player in PlayerRank[0])
                {
                    player.PlayerState = PlayerState.Winner;
                    player.ChipCountToCheckWhenWinning = player.ChipCount;
                }
            }
            else
            {
                foreach (Player player in PlayerRank[i])
                {
                    player.PlayerState = PlayerState.Loser;
                }
            }
        }
        numberOfWinners = PlayerRank[0].Count;
    }

    public void SetNextPlayer() //So this basically just is the Dealer pointing at the next player. 
    {
        if (playerToAct != null) //first we check that it's not null
        {
            int currentPlayerSeatPos = playerToAct.SeatPos; //then we get our currentSeatPos
            Player nextPlayer; //setUp a holder for who the next player will be based on currentPlayer;
            bool roundFinished = true; //in the for loop, we set roundFinished to false. but if all the criteria are met, then there IS no next player, and the round is over. 
            for (int i = 1; i < table.players.Length; i++)
            {
                //so now we go through each player and check: Have they acted, or do they need to act? Are the playing? CAN they play? and not everyone is all in, correct?
                nextPlayer = table.players[(currentPlayerSeatPos + i) % table.players.Length];
                #region //debug scripts
                if (nextPlayer.PlayerState == PlayerState.Playing)
                {
                    //Debug.Log("nextPlayer = " + nextPlayer.SeatPos);
                    //Debug.Log("nextPlayer.actedThisRound = " + nextPlayer.actedThisRound);
                    //Debug.Log("nextPlayer.currentBet = " + nextPlayer.currentBet + " and lastBet = " + lastBet);
                    //Debug.Log("nextPlayer.chipCount = " + nextPlayer.ChipCount);
                    //Debug.Log("nextPlayer.PlayerState = " + nextPlayer.PlayerState);
                }
                #endregion 
                if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < lastBet) 
                    && nextPlayer.PlayerState == PlayerState.Playing 
                    && nextPlayer.ChipCount != 0 && !OnlyAllInPlayersLeft())
                {
                    //once we get a player that meets all the criteria, we set roundFinished to false, so we don't end the round
                    roundFinished = false;
                    playerToAct = nextPlayer; //and we set the playerToAct;
                    playerToAct.decisionState = PlayerDecisionState.ToCall;
                    break;
                }
            }
            if (roundFinished)
            {
                Debug.Log("round finished no one should do anything anymore, setting playerToAct = null");
                playerToAct = null;
                lastBet = 0;
                foreach (Player player in table.players)
                {
                    player.currentBet = 0;
                    player.lastAction = PlayerAction.None;
                    if (player.PlayerState == PlayerState.Eliminated)
                    {
                        player.decisionState = PlayerDecisionState.Eliminated;
                    }
                    //else player.decisionState = PlayerDecisionState.None;
                }
                if (OnlyAllInPlayersLeft())
                {
                    RevealCards();
                }
                if (table.gameState == GameState.PreFlop)
                {
                    table.gameState = GameState.Flop;
                }
                else if (table.gameState == GameState.Flop)
                {
                    table.gameState = GameState.Turn;
                }
                else if(table.gameState == GameState.Turn)
                {
                    table.gameState = GameState.River;
                }
                else if(table.gameState == GameState.River)
                {
                    table.gameState = GameState.CleanUp;
                    Debug.Log("Entering cleanup");
                    //if players are in hand, reveal their cards
                    RevealCards();
                    ChooseWinner();
                }
            }
        }
    }

    public void RevealCards()
    {
        for (int i = 0; i < table.players.Length; i++)
        {
            if (table.players[i].PlayerState != PlayerState.NotPlaying && table.players[i].PlayerState != PlayerState.Eliminated)
            {
                for (int j = 0; j < 2; j++)
                {
                    Services.UIManager.SetCardImage(table.playerDestinations[i], table.players[i].holeCards);
                }
            }
        }
    }

    public bool OnlyAllInPlayersLeft() //checks if it's only all in players, and returns true or false
    {
        float allInPlayers = 0;

        for (int i = 0; i < table.players.Length; i++)
        {
            if (table.players[i].playerIsAllIn) allInPlayers++;
        }
        //Debug.Log("AllInPlayerCount = " + allInPlayers);
        //Debug.Log("ActivePlayerCount = " + ActivePlayerCount());
        if (allInPlayers == ActivePlayerCount())
        {
            return true;
        }
        else if ((ActivePlayerCount() - allInPlayers) == 1)
        {
            return true;
        }
        else return false;
    }

    public void InsultPlayer() //Click a button and it cycles through to the next emotion. Binary choice.
    {
        if (!influencingTable)
        {
            influencingTable = true;
            int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
            if (table.players[seatPos].PlayerEmotion == PlayerEmotion.OnTilt)
            {
                table.players[seatPos].PlayerEmotion = PlayerEmotion.OnTilt;
            }
            else
            {
                //int randomNum = Random.Range(1, 4);
                talkCount++;
                table.players[seatPos].PlayerEmotion += 1;
                if (table.players[seatPos].PlayerEmotion >= PlayerEmotion.OnTilt)
                {
                    table.players[seatPos].PlayerEmotion = PlayerEmotion.OnTilt;
                }
            }
        }
    }

    public void FlatterPlayer() //Click a button and it cycles through to the next emotion. Binary choice.
    {
        if (!influencingTable)
        {
            influencingTable = true;
            int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject);
            if (table.players[seatPos].PlayerEmotion == PlayerEmotion.Joyous)
            {
                table.players[seatPos].PlayerEmotion = PlayerEmotion.Joyous;
            }
            else
            {
                talkCount++;
                //int randomNum = Random.Range(1, 4);
                table.players[seatPos].PlayerEmotion -= 1;
                if (table.players[seatPos].PlayerEmotion <= PlayerEmotion.Joyous)
                {
                    table.players[seatPos].PlayerEmotion = PlayerEmotion.Joyous;
                }
            }
        }
    }

    public void ChangeEmotions()
    {
        foreach (Player player in table.players)
        {
            if (player.awardedThisRound)
            {
                player.lossCount = 0;
                if (player.PlayerEmotion == PlayerEmotion.Joyous)
                {
                    player.PlayerEmotion = PlayerEmotion.Joyous;
                }
                else player.PlayerEmotion -= 1;
            }
            else
            {
                player.lossCount++;
                if (player.PlayerEmotion == PlayerEmotion.OnTilt)
                {
                    player.PlayerEmotion = PlayerEmotion.OnTilt;
                }
                else player.PlayerEmotion += 1;
            }
        }
    }

    public void SetAwardPlayer_Amount() //Click a button and give a player the Pot. Winners get happier, losers sadder. We also split the pot here which is a whole ass thing. 
    {
        if (table.gameState == GameState.CleanUp && table.pot != 0)
        {
            EndOfHand_PotAmount = table.pot;
            //Debug.Log("POT AMOUNT AT THE BEGINNIGN OF AWARD = " + table.pot);
            bool potActive = false;
            if (numberOfWinners == 1)
            {
                //so if we only have one winner. EZPZ. Everyone else is a loser, we award the winner. 
                for (int i = 0; i < table.players.Length; i++)
                {
                    if (table.players[i].PlayerState == PlayerState.Winner)
                    {
                        table.players[i].awardedThisRound = true;
                        if (table.players[i].MaxAmountToWin < EndOfHand_PotAmount) //if there's a side pot
                        {
                            //table.players[i].ChipCount += table.players[i].MaxAmountToWin; //give this player the max winnings
                            table.players[i].amountToAward += table.players[i].MaxAmountToWin;
                            //table.pot -= table.players[i].MaxAmountToWin; //take that from the pot
                            EndOfHand_PotAmount -= table.players[i].MaxAmountToWin;
                            table.players[i].PlayerState = PlayerState.NotPlaying;
                            potActive = true; //and announce that there's still an active pot
                            //Debug.Log("There's still money left in the pot and it equals " + table.pot);
                        }
                        else //else it's a single winner in which case, give it to them
                        {
                            int amountToAward = EndOfHand_PotAmount;
                            //table.pot -= amountToAward;
                            table.players[i].amountToAward += amountToAward;
                            Debug.Log("Player " + table.players[i].SeatPos +
                                      " is the winner and is recieving " + EndOfHand_PotAmount +
                                      " and their chipCount + pot = " + table.players[i].ChipCount);
                            //table.pot = 0;
                        }
                    }
                }
                int index = 1;
                while (potActive)
                {
                    Debug.Log("Before split, EndofHand_PotAmount = " + EndOfHand_PotAmount);
                    foreach (Player player in PlayerRank[index])
                    {
                        player.PlayerState = PlayerState.Winner;
                        player.awardedThisRound = true;
                    }
                    SplitChips(index, EndOfHand_PotAmount);
                    foreach (Player player in PlayerRank[index])
                    {
                        player.PlayerState = PlayerState.NotPlaying;
                    }
                    Debug.Log("After splot, EndofHand_PotAmount = " + EndOfHand_PotAmount);
                    if(EndOfHand_PotAmount == 0) potActive = false;
                    else
                    {
                        index++;
                    }
                }
            }
            else //splitting chips amongst rank 0 players
            {
                //BUT IF WE HAVE MULTIPLE WINNERS [IN THE FIRST SIDE POT]
                //In Poker, if we have multiple winners, we split the pot among the winners.
                //In situations where the pot cannot be split evenly, then the remaining chip is given
                //      to the player who is closest to the left of the dealer
                Debug.Log("We have " + numberOfWinners + " winners");
                Debug.Log("PlayerRank[0].count = " + PlayerRank[0].Count);
                SplitChips(0, EndOfHand_PotAmount);
                Debug.Log("We're making it here");
                //table.pot = 0;
                #region old code that I might use still
                //I'm going to end up automating this to test some stuff.
                //int seatPos = table.GetSeatPosFromTag(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject) - 1;
                //table.players[seatPos].ChipCount += table.pot;
                //table.pot = 0;

                //for (int i = 0; i < table.numActivePlayers; i++)
                //{
                //    if (i == seatPos)
                //    {
                //        if (table.players[seatPos].PlayerEmotion == PlayerEmotion.Joyous)
                //        {
                //            table.players[seatPos].PlayerEmotion = PlayerEmotion.Joyous;
                //        }
                //        else table.players[seatPos].PlayerEmotion -= 1;
                //    }
                //    else
                //    {
                //        if (table.players[i].PlayerEmotion == PlayerEmotion.OnTilt)
                //        {
                //            table.players[i].PlayerEmotion = PlayerEmotion.OnTilt;
                //        }
                //        else table.players[i].PlayerEmotion += 1;
                //    }
                //}
                #endregion //Code for clicking on indiviudal winners
            }
            //Debug.Assert(EndOfHand_PotAmount == 0, "Table.pot = " + EndOfHand_PotAmount);
            //ChangeEmotions();
            //for (int i = 0; i < table.players.Length; i++)
            //{
            //    if (table.players[i].ChipCount <= 0)
            //    {
            //        table.players[i].PlayerState = PlayerState.Eliminated;
            //    }
            //}
        }
    }

    public void SplitChips(int PlayerRankPos, int potAmount)
    {
        if (table.pot < 0) table.pot = 0;
        int amountToSplit = EndOfHand_PotAmount;
        //Debug.Log("Splitting" + amountToSplit + " chips");
        List<int> chipStacks = new List<int>(); //so we make a list of ChipStacks
        int maxWinnings = 0;
        for (int i = 0; i < PlayerRank[PlayerRankPos].Count; i++)
        {
            maxWinnings += PlayerRank[PlayerRankPos][i].MaxAmountToWin;
        }
        while (chipStacks.Count() < PlayerRank[PlayerRankPos].Count)
        {
            int playerCount = 0;
            chipStacks.Add(playerCount); //we want to add a number of chips = to the number of winners
        }
        //Debug.Log("We have " + chipStacks.Count + " chipstacks ready to go");
        //int tempPot = table.pot; //we want to have a tempPot just to not mess with the actual pot
        int cycleCount = amountToSplit / (int)table.lowestChipDenomination; //this is how many times we need to divy up chips. We divide the pot by the lowest chip denomination, since that would be the remainder.
        int moneyAwarded = 0;
        while (cycleCount > 0) //while we're still going through the proper number of cycles
        {
            for (int i = 0; i < chipStacks.Count; i++) //we cycle through the chipStacks
            {
                cycleCount--; //we lower the cycle. Think of one hand placing chips on each stack
                if (amountToSplit != 0) //if we still have money in the pot
                {
                    if (moneyAwarded < maxWinnings)
                    {
                        moneyAwarded += (int)table.lowestChipDenomination;
                        amountToSplit -= (int)table.lowestChipDenomination; //take the money out of the pot
                        EndOfHand_PotAmount -= (int)table.lowestChipDenomination;
                        chipStacks[i] += (int)table.lowestChipDenomination; //and place it into the chipStack
                    }
                    else
                    {
                        Debug.Log("breaking out of this cycle");
                        Debug.Log("table.pot = " + EndOfHand_PotAmount);
                        Debug.Log("moneyAwarded = " + moneyAwarded);
                        Debug.Log("maxWinnings = " + maxWinnings);
                        break;
                    }
                }
            }
        }
        //Now the fun part. We order the chipStacks from highest to lowest
        //We do this because we want to give the highest chipstack (the one with the remainder) to the player(s) closest to the left of the dealer.
        List<int> sortedChips = new List<int>(chipStacks.
            OrderByDescending(biggestStack => biggestStack));

        //so now we cycle through the chipStacks
        if (PlayerRank[PlayerRankPos].Count >= 2 && amountToSplit != 0)
        {
            for (int chipStack = 0; chipStack < sortedChips.Count; chipStack++)
            {
                //Debug.Log("chipstack " + chipStack + " has " + chipStacks[chipStack]);
                //for the first Chipstack, we want to find the winning player closest to the dealer
                for (int winningPlayer = 0; winningPlayer < table.players.Length; winningPlayer++)
                {
                    Player playerToAward = PlayerSeatsAwayFromDealerAmongstLivePlayers(winningPlayer + 1);
                    //so we check: is the player to the left of the dealer a winner?
                    if (PlayerSeatsAwayFromDealerAmongstLivePlayers(winningPlayer + 1).PlayerState == PlayerState.Winner)
                    {
                       // Debug.Log("is this the last debug?");
                        //THE PROBLEM IS THE INDEX OF SORTED CHIPS
                        //Debug.Log("Is the problem sortedChips[chipStack]? sortedChips[chipStack] = " + sortedChips[chipStack]);
                        //Debug.Log("Chipstacks.Count = " + chipStacks.Count + " and we're looking at chipStack + " + chipStack);
                        if (sortedChips[chipStack] > playerToAward.MaxAmountToWin && amountToSplit != 0)
                        {
                            //Debug.Log("pot before difference: " + amountToSplit);
                            int difference = sortedChips[chipStack] - playerToAward.MaxAmountToWin;
                            sortedChips[chipStack] = playerToAward.MaxAmountToWin;
                            //table.pot += difference;
                            //Debug.Log("pot after difference: " + table.pot);
                            if (sortedChips[chipStack + 1] != 0)
                            {
                                //Debug.Log("sortedChips[chipStack + 1] = " + sortedChips[chipStack + 1]);
                                //Debug.Log("Difference = " + difference);
                                sortedChips[chipStack + 1] += difference;
                            }
                            else
                            {
                                EndOfHand_PotAmount += difference;
                                //Debug.Log("Adding difference to table: " + difference);
                            }
                        }
                        //Debug.Log("Will give " + chipStacks[chipStack] + " to " + playerToAward.SeatPos);
                        //if so, then we give them the first stack, and we move onto the next stack. 
                        playerToAward.amountToAward += sortedChips[chipStack];
                       // Debug.Log("ARE WE GETTING PASS playerToAward.ChipCount += sortedChips[chipStack]?");
                        //if (chipStack < sortedChips.Count) chipStack++;
                    }
                }
                //if (table.pot != 0)
                //{
                //    Debug.Log("POT NOT EQUAL TO ZERO IT'S EQUAL TO: " + table.pot);
                //    for (int i = 0; i < table.players.Length; i++)
                //    {
                //        if (table.players[i].PlayerState == PlayerState.Winner)
                //        {
                //            Debug.Log("Giving these extra chips to player " + i);
                //            table.players[i].ChipCount += table.pot;
                //            table.pot = 0;
                //        }
                //    }
                //}
            }

        }
        else
        {
            //Debug.Log("PlayerRank[PlayerRankPos].Count >= 2? In fact, it = " + PlayerRank[PlayerRankPos].Count);
            //Debug.Log("amountToSplit != 0 and instead = " + amountToSplit);
            int test = 0;
            for (int i = 0; i < table.players.Length; i++)
            {
                if(table.players[i].PlayerState == PlayerState.Winner)
                {
                    test++;
                }
            }
            //Debug.Log("there are " + test + " winner(s). THERE CAN ONLY BE 1");
            for (int i = 0; i < table.players.Length; i++)
            {
                if (table.players[i].PlayerState == PlayerState.Winner)
                {
                    //Debug.Log("IS THIS THE ISSUE?");
                   // Debug.Log("i = " + i);
                    //Debug.Log("sortedChips[0] = " + sortedChips[0]);
                    table.players[i].amountToAward += sortedChips[0];
                   // Debug.Log("Awarded player " + i);
                }
            }
        }
    }

    public void PassDealerButton() //This Passes the dealer button to the next player using a series of functions
    {
        Player nextDealer = PlayerSeatsAwayFromDealerAmongstLivePlayers(1);
        table.DealerPosition = nextDealer.SeatPos;
        Services.UIManager.SetDealerPositionUI(table.DealerPosition);

    }

    public CardType TakeCardFromDeck() //Grab a random card from the deck
    {
        CardType card;
        int cardPos = Random.Range(0, cardsInDeck.Count);
        card = cardsInDeck[cardPos];
        cardsInDeck.Remove(card);
        return card;
    }

    public void TakeCheatCardFromDeck(CardType card)
    {
        for(int i = 0; i < cardsInDeck.Count; i++)
        {
            CardType cardInDeck = cardsInDeck[i];
            if((cardInDeck.rank == card.rank) & (cardInDeck.suit == card.suit))
            {
                cardsInDeck.Remove(cardInDeck);
            }
        }
    }

    public void BurnCard() //Throw a card into the burn
    {
        List<CardType> burnCard = new List<CardType> { TakeCardFromDeck() };
        table.burn.Add(burnCard[0]);
        Services.UIManager.SetCardImage(Destination.burn, burnCard);

    }

    public void AddCardToBoard(List<CardType> board)
    {
        foreach(CardType card in board)
        {
            table.board.Add(card);
        }
    }

    public void GiveCardToPlayer(Destination des, List<CardType> cards) //sends the card to the proper player
    {
        foreach (CardType card in cards)
        {
            table.players[(int)des].Cards.Add(card);
        }
}

    public void PopulateCardDeck() //this creates a card deck. We don't shuffle the deck since we don't need to.
    {
        cardsInDeck = new List<CardType>();
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
                cardsInDeck.Add(new CardType(rank, suit));
            }
        }
    }

    public List<CardType> CreateFakeDeck() //this creates a card deck. We don't shuffle the deck since we don't need to.
    {
        List<CardType> fakeDeck = new List<CardType>();
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
                fakeDeck.Add(new CardType(rank, suit));
            }
        }
        return fakeDeck;
    }

    public Player FindFirstPlayerToAct(int distance) //this gives us the first player to act. We need this since the first player is going to change depending on blinds and who is in the pot.
    {
        //Debug.Log("FindingFirstPlayerToAct");
        Player player;
        player = PlayerSeatsAwayFromDealerAmongstLivePlayers(distance);
        if (player.PlayerState == PlayerState.NotPlaying || player.playerIsAllIn || player.PlayerState == PlayerState.Eliminated || player.currentBet > 0)
        {
            for (int i = 0; i < table.players.Length; i++)
            {
                Player nextPlayer = table.players[(player.SeatPos + i) % table.players.Length];
                if (nextPlayer.PlayerState != PlayerState.NotPlaying && !nextPlayer.playerIsAllIn && nextPlayer.PlayerState != PlayerState.Eliminated)
                {
                    player = nextPlayer;
                    break;
                }
            }
        }
        return player;
    }
    
    public Player PlayerSeatsAwayFromDealerAmongstLivePlayers(int distance) //this function basically uses another function to return a player. not sure why it's separated like this, but I'm going with it.
    {
        table = Services.TableManager;
        return table.players[SeatsAwayFromDealerAmongstLivePlayers(distance)];
    }
   
    public int SeatsAwayFromDealerAmongstLivePlayers(int distance)//this is the raw function that I can use to say "okay, who is the next player. It gets complicated, so let's look inside.
    {
        int playersInLine = 0; //we start by creating a line of players. it's 0 right now.
        int index = 0; //and we're at the 0 index.
        distance = distance % ActivePlayerCount(); //distance = 3, and P@T = 3, so distance = 0
        //Debug.Log("distance = " + distance);
        Debug.Assert(ActivePlayerCount() > 0); //we should never be looking at this if there's no one at the table.
        while (playersInLine <= distance) //while 0 <= 0
        {
            Player player = table.players[SeatsAwayFromDealer(index)]; //player = players[distance from the player we've passed]
            if (player.PlayerState != PlayerState.Eliminated)//if the player has not been eliminated
            {
                if (playersInLine == distance)//and players in line = distance
                {
                    return player.SeatPos; //then this is our player
                }
                playersInLine += 1;
            }
            index += 1;
        }
        //should never get here
        Debug.Assert(false);
        return 0;
    }

    public int SeatsAwayFromDealer(int distance) //this is just an easy Modulo.
    {
        return (table.DealerPosition + distance) % table.players.Length;
    }

    public int ActivePlayerCount()//QoL function for determining how many active players are around;
    {
        TableManager tm = Services.TableManager;
        int activePlayers = 0;
        for (int i = 0; i < tm.players.Length; i++)
        {
            if (tm.players[i].PlayerState != PlayerState.Eliminated && tm.players[i].PlayerState != PlayerState.NotPlaying)
            {
                activePlayers++;
            }
        }
        return activePlayers;
    }

    public int LivePlayerCount()
    {
        TableManager tm = Services.TableManager;
        int livePlayers = 0;
        for (int i = 0; i < tm.players.Length; i++)
        {
            if (tm.players[i].PlayerState != PlayerState.Eliminated)
            {
                livePlayers++;
            }
        }
        return livePlayers;
    }

    public int EliminatedPlayerCount()
    {
        TableManager tm = Services.TableManager;
        int eliminated = 0;
        for (int i = 0; i < tm.players.Length; i++)
        {
            if (tm.players[i].PlayerState == PlayerState.Eliminated)
            {
                eliminated++;
            }
        }
        return eliminated;
    }

    public List<Player> PlayersInGame()
    {
        List<Player> playersInGame = new List<Player>();

        for(int i = 0; i < table.players.Length; i++)
        {
            //if (table.players[i].PlayerState == PlayerState.Playing)
            //{
                playersInGame.Add(table.players[i]);
            //}
        }

        return playersInGame;
    }

    public void SetBlinds()
    {
        Player small;
        Player big;
        if(ActivePlayerCount() == 2) 
        {
            small = PlayerSeatsAwayFromDealerAmongstLivePlayers(0);
            big = PlayerSeatsAwayFromDealerAmongstLivePlayers(1);
        }
        else
        {
            small = PlayerSeatsAwayFromDealerAmongstLivePlayers(1);
            big = PlayerSeatsAwayFromDealerAmongstLivePlayers(2);
        }
        small.decisionState = PlayerDecisionState.SmallBlind;
        big.decisionState = PlayerDecisionState.BigBlind;

        CollectBlinds(small, table.smallBlind);
        CollectBlinds(big, table.bigBlind);
        lastBet = table.bigBlind;
    }

    public void CollectBlinds(Player player, int bet)
    {
        if(bet >= player.ChipCount)
        {
            bet = player.ChipCount;
            player.playerIsAllIn = true;
            Debug.Log("player " + player.SeatPos + " is all in");
        }
        player.currentBet += bet;
        player.ChipCount -= bet;
        table.pot += bet;
        //if (bet > lastBet)
        //{
        //    lastBet = bet;
        //}
    }

}
