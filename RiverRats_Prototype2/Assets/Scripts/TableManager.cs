using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script is the table script. It manages what goes ONTO the table, or what is on the table. It's more of a management class than anything. We have players at the table, money on the table. Basically if it's on the table, it'll probs be here. 
 */
public enum Destination { player1, player2, player3, player4, player5, flop, turn, river, burn}
public enum GameState { PreFlop = 0, Flop, Turn, River, Showdown, CleanUp, GameOver };
public enum Chips { Five = 5, Ten = 10, TwentyFive = 25, Fifty = 50, Hundred = 100, FiveHundred = 500, Thousand = 1000 };
public class TableManager : MonoBehaviour
{
    public Player[] players; //number of players
    public List<CardType> board = new List<CardType>(); //holds the board cards. Not currently used, but maybe one day.
    public List<CardType> burn = new List<CardType>(); //holds the burn cards. Used right now to determine UI.
    public List<Destination> playerDestinations = new List<Destination> //makes it easier to deal to players
    { Destination.player1, Destination.player2, Destination.player3, Destination.player4, Destination.player5};
    public int numActivePlayers; //how many players are in the hand
    public int blindRound; //what's the blind amount?
    public int smallBlind;
    public int bigBlind;
    public int pot; //what's the pot amount?
    public int DealerPosition; //this is the dealer button
    public Chips lowestChipDenomination;
    public GameState gameState; //what part of the round are we in?

    void Start()
    {
        InitializeTable();
    }

    void Update()
    {
        IncreaseBlinds();
    }

    private void InitializeTable() //setting Services, creating platyers, and setting initial blind/round/gamestate
    {
        Services.TableManager = this;
        Services.PlayerBehaviour = new PlayerBehaviour();
        Services.DealerManager = GameObject.Find("DealerManager").GetComponent<DealerManager>();
        Services.UIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        CreatePlayers();
        foreach (Player player in players)
        {
            player.SetMaxWinnings();
            //Debug.Log("Max winnings = " + player.MaxAmountToWin);
        }
        //blindRound = 100;
        pot = 0;
        gameState = GameState.PreFlop;
        lowestChipDenomination = Chips.Five;
        //smallBlind = 100;
        //bigBlind = 200;
        SetDealerPositionAtGameStart(0);
        #region
        //Debug.Log("Little Blind is on Player " + (Services.DealerManager.PlayerSeatsAwayFromDealerAmongstLivePlayers(1).SeatPos + 1));
        //Debug.Log("Big Blind is on Player " + (Services.DealerManager.PlayerSeatsAwayFromDealerAmongstLivePlayers(2).SeatPos + 1));
        //Debug.Log("First player to act is " + (Services.DealerManager.FindFirstPlayerToAct(3).SeatPos + 1));
        #endregion just some button debug
    }

    private void CreatePlayers() //Creates each player and assigns seat, chipcount, and emotional state
    {
        players = new Player[numActivePlayers];
        for (int i = 0; i < numActivePlayers; i++)
        {
            players[i] = new Player(i, Services.DealerManager.startingChipStack, PlayerEmotion.Content, PlayerState.Playing);
            Services.UIManager.chipHolders[i].GetComponentInChildren<Text>().text = players[i].ChipCount.ToString();
            if (numActivePlayers == 5)
            {
                players[i].playersLostAgainst = new List<int>(5)
                {
                     0, 0, 0, 0, 0
                };
            }
        }
        Services.UIManager.SetPlayerHoleCards();
    }

    public void IncreaseBlinds()
    {
        if(Services.DealerManager.LivePlayerCount() == 5)
        {
            smallBlind = 25;
            bigBlind = 50;
        }
        else if(Services.DealerManager.LivePlayerCount() == 4)
        {
            smallBlind = 50;
            bigBlind = 100;
        }
        else if (Services.DealerManager.LivePlayerCount() == 3)
        {
            smallBlind = 100;
            bigBlind = 200;
        }
        else if (Services.DealerManager.LivePlayerCount() == 2)
        {
            smallBlind = 200;
            bigBlind = 400;
        }
    }

    public void PlayersLookAtCards() //this is the call for players to evaluate their hands. 
    {
        foreach (Player player in players)
        {
            if(player.PlayerState == PlayerState.Playing)
            {
                player.EvaluateMyHand(gameState);
            }
        }
    }

    public void SetDealerPositionAtGameStart(int pos)
    {
        DealerPosition = pos;
        Services.UIManager.SetDealerPositionUI(pos);
    }
    
    public int GetSeatPosFromTag(GameObject obj) //this is some jank shit so that I can determine what player I'm clicking
    {
        string objTag = obj.tag;
        int seatPos = 0;

        switch(objTag)
        {
            case "P0":
                seatPos = 0;
                break;
            case "P1":
                seatPos = 1;
                break;
            case "P2":
                seatPos = 2;
                break;
            case "P3":
                seatPos = 3;
                break;
            case "P4":
                seatPos = 4;
                break;
        }
        return seatPos;
    }
   
    //this is an ease of life function to find how far away from the dealer button a given player is
    public int SeatsAwayFromDealer(int distance)
    {
        return (DealerPosition + distance) % players.Length;
    }
}
