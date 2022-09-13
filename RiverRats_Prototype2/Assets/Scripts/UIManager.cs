using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * The whole point of this Script is to control the on screen UI. If it's about the UI it should be in here. We don't want to control any of the actual gameplay logic, just the visualization of that object. 
 */
public class UIManager : MonoBehaviour
{
    private TaskManager tm;
    [HideInInspector]
    public bool dealPulseReady;
    public GameObject dealButton;
    public GameObject dealBorder;
    
    [HideInInspector]
    public TableManager table; //easy reference to the table, since the UI is doing that LABOR
    public List<GameObject> chipHolders = new List<GameObject>(); //list of objs where we visualize player chip amount.
    public List<GameObject> emotionalState = new List<GameObject>(); //list of objs where we visualize player emotions. 
    public List<GameObject> betAmounts = new List<GameObject>(); //list of objs that visualize how much they're betting
    public List<GameObject> maxBets = new List<GameObject>();
    public List<GameObject> handStrengths = new List<GameObject>();
    public GameObject blinds; //obj visualizing the blinds
    public GameObject potSize; //obj visualizing the pot
    public GameObject gameState;

    public List<Sprite> spadeSprites;//all of these
    public List<Sprite> heartSprites;//sprites were dragged
    public List<Sprite> diamondSprites;//and dropped into the inspector from
    public List<Sprite> clubSprites;// various sprite sheets
    public List<Sprite>[] cardSprites;// they were then combined to form cards
    public Sprite cardBack; //the back of a card
    private Color cardColor; //color of the card to control transparency

    public List<GameObject> dealerPositions; //This holds all the places the dealer button can be
    public GameObject flop1, flop2, flop3; //objs to visualize the flop
    public List<GameObject> Flop; //holds the flop objs, for ease of us
    public GameObject turn; //visualizes the turn
    public GameObject river; //visualize the river
    public GameObject burn1, burn2, burn3; //visualize each burn card
    public List<GameObject> Burn; //same as the flop, makes likee easier
    [HideInInspector]
    public List<GameObject> allGameCardObjs = new List<GameObject>(); //a list that holds ALL game objs on the UI
    public List<GameObject>[] playerHoleCards = new List<GameObject>[5]
    {
        new List<GameObject>(), new List<GameObject>(), new List<GameObject>(), new List<GameObject>(), new List<GameObject>()
    };//these visualize each of the players 2 cards
    public GameObject cheatCounter;
    public GameObject avgScore_HS;
    public List<GameObject> playerToActBorder;
    private Color borderColor_on;
    private Color borderColor_off;
    private Color targetPlayerColor;



    void Start()
    {
        tm = new TaskManager();
        InitializeBoardUI();
    }

    void Update()
    {
        tm.Update();
        UpdateTextOnScreen();
        PlayerToActUI();
    }

    public void SetBorderImage()
    {
        borderColor_on = playerToActBorder[0].GetComponent<Image>().color;
        borderColor_off = new Color(borderColor_on.r, borderColor_on.g, borderColor_on.b, 0);
        targetPlayerColor = Color.blue;

        foreach(GameObject obj in playerToActBorder)
        {
            obj.GetComponent<Image>().color = borderColor_off;
        }
    }

    public void DealPulse()
    {
        LerpObjectColor_Image lerpUp = new LerpObjectColor_Image(dealButton, Color.white, Color.red, Easing.FunctionType.Linear, 1f);

        tm.Do(lerpUp);
    }
    
    private void InitializeBoardUI() //combine all the sprites, set gameobjects
    {
        table = Services.TableManager;
        cardSprites = new List<Sprite>[4]
        {
            spadeSprites,
            heartSprites,
            diamondSprites,
            clubSprites
        };
        cardColor = flop1.GetComponent<Image>().color;
        Flop = new List<GameObject>
        {
            flop1, flop2, flop3
        };
        Burn = new List<GameObject>
        {
            burn1, burn2, burn3
        };
        for(int i = 0; i < 3; i++)
        {
            FindAllCardGameObjects(Flop[i]);
            FindAllCardGameObjects(Burn[i]);
        }
        FindAllCardGameObjects(turn);
        FindAllCardGameObjects(river);
        SetBorderImage();
    }

    private void UpdateTextOnScreen() //for all the things that need to update, keep them updated
    {
        for (int i = 0; i < table.numActivePlayers; i++)
        {
            chipHolders[i].GetComponentInChildren<Text>().text = table.players[i].ChipCount.ToString();
            if (table.players[i].PlayerState != PlayerState.Eliminated)
            {
                emotionalState[i].GetComponentInChildren<Text>().text = table.players[i].PlayerEmotion.ToString();
                betAmounts[i].GetComponent<Text>().text = table.players[i].currentBet.ToString();
                maxBets[i].GetComponent<Text>().text = table.players[i].MaxAmountToWin.ToString();
                handStrengths[i].GetComponent<Text>().text = table.players[i].HandStrength.ToString();
            }
            else
            {
                emotionalState[i].GetComponentInChildren<Text>().text = "Eliminated";
                betAmounts[i].GetComponent<Text>().text = "0";
                maxBets[i].GetComponent<Text>().text = "0";
            }
        }
        gameState.GetComponent<Text>().text = table.gameState.ToString();
        blinds.GetComponent<Text>().text = table.blindRound.ToString();
        potSize.GetComponent<Text>().text = table.pot.ToString();
        cheatCounter.GetComponent<Text>().text = Services.DealerManager.cheatCount.ToString();
        avgScore_HS.GetComponent<Text>().text = Services.DealerManager.averageHS.ToString();
    }
    
    public void SetDealerPositionUI(int pos)
    {
        for(int i = 0; i < dealerPositions.Count; i++)
        {
            if (i == pos)
            {
                dealerPositions[i].SetActive(true);
            }
            else dealerPositions[i].SetActive(false);
        }
    }

    public void TurnPlayerCardImageOff(Destination des)
    {
        for (int player = 0; player < table.numActivePlayers; player++)
        {
            if (des == table.playerDestinations[player])
            {
                for (int card = 0; card < 2; card++)
                {
                    playerHoleCards[player][card].GetComponent<Image>().sprite = cardBack;
                    playerHoleCards[player][card].GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, 0);
                }
            }
        }
    }
    
    public void SetCardImage(Destination des, List<CardType> cards) //Set the card image based on the destination
    {
        DealerManager dm = Services.DealerManager;
        GameState round = Services.TableManager.gameState;

        
         //each time we call this, we check the round so we can determine the specific needs of the round
        if (round == GameState.PreFlop)
        {
            //so basically, each player has two hole cards, and two places to showcase those cards
            //we cycle through each player based on the destination
            //then for each player, we assign the card to one of their hole card objects
            for (int player = 0; player < table.numActivePlayers; player++)
            {
                if (des == table.playerDestinations[player])
                {
                    for (int card = 0; card < table.players[player].holeCards.Count; card++)
                    {
                        //playerHoleCards[player][card].GetComponent<Image>().sprite = GetCardImage(cards[card]);
                        playerHoleCards[player][card].GetComponent<Image>().sprite = cardBack;
                        playerHoleCards[player][card].GetComponent<Image>().color = cardColor;
                    }
                }
            }
        }
        else if(round == GameState.CleanUp || round == GameState.Showdown)
        {
            for (int player = 0; player < table.players.Length; player++)
            {
                if (des == table.playerDestinations[player])
                {
                    for (int card = 0; card < table.players[player].holeCards.Count; card++)
                    {
                        playerHoleCards[player][card].GetComponent<Image>().sprite = GetCardImage(cards[card]);
                        playerHoleCards[player][card].GetComponent<Image>().color = cardColor;
                    }
                }
            }
        }
        //on the burn, we just want to show we burned a card, so we just set it to a card back
        //but it DOES have a card associated with it
        else if(des == Destination.burn)
        {
            int pos = table.burn.Count;
            Burn[pos].GetComponent<Image>().sprite = cardBack;
            Burn[pos].GetComponent<Image>().color = cardColor;
        }
        else if (des == Destination.flop)
        {
            //for the flop and all the rest, we basically just use the list of cards passed to set the proper image.
            int flopPos = table.board.Count - 1;
            //for (int flopPos = 0; flopPos < 3; flopPos++)
            //{
                Flop[flopPos].GetComponent<Image>().sprite = GetCardImage(cards[0]);
                Flop[flopPos].GetComponent<Image>().color = cardColor;
            //}
        }
        else if (des == Destination.turn)
        {
            turn.GetComponent<Image>().sprite = GetCardImage(cards[0]);
            turn.GetComponent<Image>().color = cardColor;
        }
        else if (des == Destination.river)
        {
            river.GetComponent<Image>().sprite = GetCardImage(cards[0]);
            river.GetComponent<Image>().color = cardColor;
        }
    }

    public void PlayerToActUI()
    {
        if (Services.DealerManager.playerToAct != null)
        {
            for (int i = 0; i < chipHolders.Count; i++)
            {
                if (i == Services.DealerManager.playerToAct.SeatPos)
                {
                    if (i != Services.GameRules.targetPlayer.SeatPos)
                    {
                        chipHolders[Services.DealerManager.playerToAct.SeatPos].GetComponent<Text>().color = Color.red;
                        playerToActBorder[Services.DealerManager.playerToAct.SeatPos].GetComponent<Image>().color = new Color(borderColor_on.r, borderColor_on.g, borderColor_on.b, borderColor_on.a);
                    }
                    else
                    {
                        chipHolders[Services.DealerManager.playerToAct.SeatPos].GetComponent<Text>().color = Color.red;
                        playerToActBorder[Services.DealerManager.playerToAct.SeatPos].GetComponent<Image>().color = new Color(targetPlayerColor.r, targetPlayerColor.g, targetPlayerColor.b, borderColor_on.a);
                    }
                }
                else
                {
                    chipHolders[i].GetComponent<Text>().color = Color.white;
                    playerToActBorder[i].GetComponent<Image>().color = new Color(borderColor_off.r, borderColor_off.g, borderColor_off.b, borderColor_off.a);
                }
            }
        }
        else
        {
            foreach(GameObject obj in chipHolders)
            {
                obj.GetComponent<Text>().color = Color.white;
            }
            foreach(GameObject obj in playerToActBorder)
            {
                obj.GetComponent<Image>().color = new Color(borderColor_off.r, borderColor_off.g, borderColor_off.b, borderColor_off.a);
            }
        }
    }
    
    public void ResetAllCardsImages() //we want to set all card alphas to 0, to make it look like their going away
    {
        foreach (GameObject obj in allGameCardObjs)
        {
            obj.GetComponent<Image>().color = new Color(cardColor.r, cardColor.g, cardColor.b, 0);
        }
    }

    private void FindAllCardGameObjects(GameObject obj) //goes through and adds cards to the list
    {
        allGameCardObjs.Add(obj);
        foreach(GameObject o in allGameCardObjs)
        {
            o.GetComponent<Image>().color = new Color(cardColor.r, cardColor.b, cardColor.g, 0);
        }
    }

    public Sprite GetCardImage(CardType card) //so when we pass a card, it looks at the rank and the suit to find the card
    {
        Sprite sprite;
        int suit = 0;
        if (card.suit == SuitType.Spades) suit = 0;
        else if (card.suit == SuitType.Hearts) suit = 1;
        else if (card.suit == SuitType.Diamonds) suit = 2;
        else suit = 3;

        int rank = 0;
        if (card.rank == RankType.Two) rank = 0;
        else if (card.rank == RankType.Three) rank = 1;
        else if (card.rank == RankType.Four) rank = 2;
        else if (card.rank == RankType.Five) rank = 3;
        else if (card.rank == RankType.Six) rank = 4;
        else if (card.rank == RankType.Seven) rank = 5;
        else if (card.rank == RankType.Eight) rank = 6;
        else if (card.rank == RankType.Nine) rank = 7;
        else if (card.rank == RankType.Ten) rank = 8;
        else if (card.rank == RankType.Jack) rank = 9;
        else if (card.rank == RankType.Queen) rank = 10;
        else if (card.rank == RankType.King) rank = 11;
        else rank = 12;


        sprite = cardSprites[suit][rank];

        return sprite;
    }

    public void SetPlayerHoleCards() //janky hard code to create the holecard player list
    {
        TableManager tm = Services.TableManager;
        for (int i = 0; i < tm.numActivePlayers; i++)
        {
            if (tm.players[i].SeatPos == 0)
            {
                playerHoleCards[i].Add(GameObject.Find("P1_HoleCard1"));
                playerHoleCards[i].Add(GameObject.Find("P1_HoleCard2"));
            }
            else if (tm.players[i].SeatPos == 1)
            {
                playerHoleCards[i].Add(GameObject.Find("P2_HoleCard1"));
                playerHoleCards[i].Add(GameObject.Find("P2_HoleCard2"));

            }
            else if (tm.players[i].SeatPos == 2)
            {
                playerHoleCards[i].Add(GameObject.Find("P3_HoleCard1"));
                playerHoleCards[i].Add(GameObject.Find("P3_HoleCard2"));
            }
            else if (tm.players[i].SeatPos == 3)
            {
                playerHoleCards[i].Add(GameObject.Find("P4_HoleCard1"));
                playerHoleCards[i].Add(GameObject.Find("P4_HoleCard2"));
            }
            else
            {
                playerHoleCards[i].Add(GameObject.Find("P5_HoleCard1"));
                playerHoleCards[i].Add(GameObject.Find("P5_HoleCard2"));
            }
        }
        for(int i = 0; i < Services.TableManager.numActivePlayers; i++)
        {
            foreach(GameObject obj in playerHoleCards[i])
            {
                FindAllCardGameObjects(obj);
            }
        }
    }
}
