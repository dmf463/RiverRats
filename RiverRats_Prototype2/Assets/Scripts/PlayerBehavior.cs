using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BehaviorTree;
using System;

public class PlayerBehaviour
{

    private Tree<Player> FCR_Tree;
    private Tree<Player> preflop_FCR_Tree;

    /*
     * PLAYER BEHAVIOUR
     */
    #region Player Behaviour Code

    public int DetermineRaiseAmount(Player player)
    {
        int raise = 0;
        int minimumRaise = Services.DealerManager.lastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.TableManager.bigBlind;
        if (Services.TableManager.gameState >= GameState.Turn) minimumRaise = Services.TableManager.pot / (UnityEngine.Random.Range(3, 6));
        int remainder = minimumRaise % (int)Services.TableManager.lowestChipDenomination;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + (int)Services.TableManager.lowestChipDenomination;
        }

        if (Services.TableManager.gameState == GameState.PreFlop)
        {
            if (((player.ChipCount - Services.DealerManager.lastBet) < (Services.TableManager.bigBlind * 4)) && player.HandStrength > 12f)
            {
                raise = player.ChipCount;
            }
            else
            {
                if (Services.DealerManager.lastBet == Services.TableManager.bigBlind)
                {
                    int randoNum = UnityEngine.Random.Range(1, 4);
                    raise = Services.TableManager.bigBlind * randoNum;//(3 + Services.DealerManager.ActivePlayerCount());
                }
                else raise = Services.DealerManager.lastBet * 2;
            }
            if (raise > player.ChipCount) raise = player.ChipCount;
        }
        else
        {
            modifier += Services.DealerManager.ActivePlayerCount(); //the more players there are the more unsafe your hand is, so bet bigger
            if (Services.DealerManager.ActivePlayerCount() > 2)
            {
                if (Services.TableManager.DealerPosition == player.SeatPos) modifier += 2; //if you're not heads up and you're the dealer, you're in good position
                else
                {
                    if (Services.DealerManager.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                    else if (Services.DealerManager.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                    else if (Services.DealerManager.ActivePlayerCount() - Services.DealerManager.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.DealerManager.ActivePlayerCount() >= 3) modifier += 2; //good position
                }
            }
            if ((player.ChipCount - Services.DealerManager.lastBet) > (Services.TableManager.bigBlind * 4)) modifier += 1; //if you can cover the next few hands thats good
            else modifier -= 1; //if you can't that's bad.

            if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand < PokerHand.OnePair) modifier += 3; //if you have a good HS but have less than OnePair, you're probably on a draw, so bet bigger
            else if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand > PokerHand.Flush && Services.TableManager.gameState < GameState.River) modifier = 1; //if you have a good HS and you have better than a flush, slow play it 

            if (player.HandStrength > .6) modifier += 1; //if you have a decent hand
            else modifier -= 1; //if you have a "meh" hand

            modifier -= Services.DealerManager.raisesInRound; //cut down on how much you're betting based on betting history
            if (modifier <= 0) modifier = 1; //if it's less than zero, make it 1
            raise = minimumRaise * Mathf.Abs(modifier);
            //Debug.Log("Player " + player.SeatPos + " is raising " + raise + " because of a modifier = " + modifier);
            if (raise > player.ChipCount) raise = player.ChipCount;
        }
        return raise;
    }
    #endregion

    public List<Player> RankedPlayerHands(Player me)
    {
        List<Player> playersToEvaluate = new List<Player>();
        playersToEvaluate.Add(me);
        for (int i = 0; i < Services.TableManager.players.Length; i++)
        {
            if (Services.TableManager.players[i].lastAction != PlayerAction.None && Services.TableManager.players[i] != me)
            {
                Services.TableManager.players[i].percievedHandStrength = Services.TableManager.players[i].HandStrength + (AdjustHandStrength(Services.TableManager.players[i]));
                playersToEvaluate.Add(Services.TableManager.players[i]);
            }
        }
        me.percievedHandStrength = me.HandStrength;
        List<Player> sortedPlayers = new List<Player>(playersToEvaluate.OrderByDescending(bestHand => bestHand.percievedHandStrength));
        return sortedPlayers;
    }

    public float AdjustHandStrength(Player opponent)
    {
        float betMod = 0.25f;
        float otherBetMod = 0;
        float exponent = .25f;
        float mod = UnityEngine.Random.Range(-0.2f, 0.2f);
        mod += 0.1f * (Services.DealerManager.ActivePlayerCount() - Services.DealerManager.SeatsAwayFromDealerAmongstLivePlayers(opponent.SeatPos));
        mod += betMod * Mathf.Pow(opponent.currentBet, exponent) + otherBetMod;
        return mod;
    }

    public void PreFlop_FCR_Neutral(Player player)
    {
        /*
         * Bad Hand <= 4
           Decent Hand >4 && <=8
           Good Hand >8 && <12
           Great Hand >=12
           IsChipLeader
           InPosition
           BeforeRiver
           BetIsZero
           BetPreflop
           RaisedAlready
           SomeoneHasRaised
         */

        int randomNum = UnityEngine.Random.Range(0, 100);
        //randomNum += IsInPosition_Mod(player, 10);
        //randomNum += IsChipLeader_Mod(player, 10);
        //randomNum += BeforeRiver_Mod(player, 10);
        //randomNum += BetIsZero_Mod(player, 10);
        //randomNum += BetPreFlop_Mod(player, 10);
        //randomNum += RaisedAlready_Mod(player, 10);

        preflop_FCR_Tree = new Tree<Player>(new Selector<Player>(
        #region bad hand
            new Sequence<Player>(
                new HasABadHand_PreFlop(),//BAD HANDS
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 5),//RAISE, BLUFE
                        new HasEnoughMoney(),
                        new Not<Player>(new SomeoneHasRaised()),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //CALL. No way we're calling 
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new IsBigBlind(),
                        new Call() //I mean, we'll check it on the big blind. 
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 40),
                        new HasEnoughMoney(),
                        new IsSmallBlind(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
        #endregion
        #region Decent hand: The Average Hand. MAKES SENSE. 
            new Sequence<Player>(
                    new HasADecentHand_Preflop(), //DECENT HAND
                    new Selector<Player>(
                        new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 5), //RAISE, BLUFF
                        new HasEnoughMoney(),
                        new Not<Player>(new SomeoneHasRaised()),
                        new Raise()
                        ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 20), //CALL FOR SHITS AND GIGLES
                            new HasEnoughMoney(),
                            new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) != 0),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new IsSmallBlind(),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Fold()
                            )
                        )
                    ),
        #endregion
        #region good hand
            new Sequence<Player>(
                new HasAGoodHand_Preflop(),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new AllIn()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) <= 0),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 40),
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Call()
                        )
                    )
                ),
        #endregion
        #region great hand
                new Sequence<Player>(
                    new HasAGreatHand_PreFlop(),
                    new Selector<Player>(
                        new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new AllIn()
                        ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 30),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Not<Player>(new RaisedAlready()),
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 70),
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Call()
                            )
                        )
                    )
        #endregion
                ));
        preflop_FCR_Tree.Update(player);
    }

    public void FCR_Neutral(Player player, float returnRate)
    {
        int randomNum = UnityEngine.Random.Range(0, 100);

        FCR_Tree = new Tree<Player>(new Selector<Player>(
            //LOW RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 0.8),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 5), //RAISE, BLUFF
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0) //CALL
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
            //MEDIUM RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.0),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 5), //CALL
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 20), //RAISE
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
            //HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.3),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(),
                        new Condition<Player>(context => randomNum < 60),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 60), //RAISE
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 100), //CALL
                        new Call()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
            //VERY HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate >= 1.3),
                new Selector<Player>(
                        new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //CALL
                        new Call()
                        ),
                        new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0),//RAISE
                        new Raise()
                        ),
                        new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0),//FOLD
                        new Fold()
                        ),
                        new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new BeingPutAllIn(),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 30),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(),
                        new Condition<Player>(context => randomNum < 70),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
             //FALL BACK SEQUENCE
             new Sequence<Player>(
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Call()
                      ),
                  new Sequence<Player>(
                      new Not<Player>(new BetIsZero()),
                      new Fold()
                      )
                  )
              )
            ));
        FCR_Tree.Update(player);
    }

    public void PreFlop_FCR_Conservative(Player player)
    {
        /*
         * Bad Hand <= 4
           Decent Hand >4 && <=8
           Good Hand >8 && <12
           Great Hand >=12
           IsChipLeader
           InPosition
           BeforeRiver
           BetIsZero
           BetPreflop
           RaisedAlready
           SomeoneHasRaised
         */

        int randomNum = UnityEngine.Random.Range(0, 100);
        //randomNum += IsInPosition_Mod(player, 10);
        //randomNum += IsChipLeader_Mod(player, 10);
        //randomNum += BeforeRiver_Mod(player, 10);
        //randomNum += BetIsZero_Mod(player, 10);
        //randomNum += BetPreFlop_Mod(player, 10);
        //randomNum += RaisedAlready_Mod(player, 10);

        preflop_FCR_Tree = new Tree<Player>(new Selector<Player>(
        #region bad hand
            new Sequence<Player>(
                new HasABadHand_PreFlop(),//BAD HANDS
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0),//RAISE, BLUFE
                        new HasEnoughMoney(),
                        new Not<Player>(new SomeoneHasRaised()),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //CALL. No way we're calling 
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new IsBigBlind(),
                        new Call() //I mean, we'll check it on the big blind. 
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 10),
                        new HasEnoughMoney(),
                        new IsSmallBlind(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
        #endregion
        #region Decent hand: The Average Hand. MAKES SENSE. 
            new Sequence<Player>(
                    new HasADecentHand_Preflop(), //DECENT HAND
                    new Selector<Player>(
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 0), //no way we're bluffing
                            new HasEnoughMoney(),
                            new Not<Player>(new SomeoneHasRaised()),
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 0), //Hell no not calling
                            new HasEnoughMoney(),
                            new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) != 0),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 30), //MAYBE we'll call
                            new IsSmallBlind(),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Fold()
                            )
                        )
                    ),
        #endregion
        #region good hand
            new Sequence<Player>(
                new HasAGoodHand_Preflop(),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new AllIn()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) <= 0),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 10), //MAYBE we'll raise with this hand, but idk luck has been bad.
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //this is the first time I've gotten a hand in forever, I'm not throwing it away unless someone raised
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Call()
                        )
                    )
                ),
        #endregion
        #region great hand
                new Sequence<Player>(
                    new HasAGreatHand_PreFlop(),
                    new Selector<Player>(
                        new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), //no money? great hand? ALL IN. 
                        new AllIn()
                        ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 10), //FINALLY, a good hand. I'm probably going to raise this
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Not<Player>(new RaisedAlready()), //if I haven't raised once. I'M DEFINITELY RAISING
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 80), //Oh you raised on me? FUCK YOU. Raise you back. 
                            new Raise()
                            ),
                        new Sequence<Player>( //Fine, let's see the flop. 
                            new Call()
                            )
                        )
                    )
        #endregion
                ));
        preflop_FCR_Tree.Update(player);
    }

    public void FCR_Conservative(Player player, float returnRate)
    {
        int randomNum = UnityEngine.Random.Range(0, 100);

        FCR_Tree = new Tree<Player>(new Selector<Player>(
            //LOW RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 0.8),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //No fucking way we're bluffing
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //Absolutely not calling with this trash
                        new Call()
                        ),
                    new Sequence<Player>( //I mean fine, the bet is zero and I'm in the hand, I guess I'll call. 
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>( //get this shit out of here. 
                        new Fold()
                        )
                    )
                ),
            //MEDIUM RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.0),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(), //you're gonna put me all in?
                        new Not<Player>(new HasHighChanceOfWinning()), //with this shit?
                        new Fold() //fuck you dealer
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //I keep missing the fucking flop
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //Hell no I'm not raising
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(), //I mean fine. You wanna check? I'll check. 
                        new Call()
                        ),
                    new Sequence<Player>( //fuck off with these cards.  
                        new Fold()
                        )
                    )
                ),
            //HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.3),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(), //you're putting me all in
                        new Not<Player>(new HasHighChanceOfWinning()), //with THIS hand?
                        new Fold() //gtfo of here
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(), //you raised?
                        new Condition<Player>(context => randomNum < 30), //man idk with this hand
                        new Call() //sure why not, maybe this time I'll hit
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0), //No fucking way I'm raising. Not with the hands I'm getting. 
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 70), //Best hand I've gotten in a while, but what if I lose again?
                        new Call()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),//you check to me? thank god, I get to see another card! CHECK CHECK CHECK
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold() //I fucking hate my life
                        )
                    )
                ),
            //VERY HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate >= 1.3),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), //fuck, I have no money... 
                        new Not<Player>(new HasHighChanceOfWinning()),//my cards are trash..
                        new BetIsZero(),//but you're checking, so sure, I'll check
                        new Call() //CHECK
                        ),
                    new Sequence<Player>( 
                        new BeingPutAllIn(), //you're putting me all in...
                        new Not<Player>(new HasHighChanceOfWinning()), //with this fucking hand
                        new Fold() //yeah, I'm out
                        ),
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), //I'm so broke...
                        new Not<Player>(new HasHighChanceOfWinning()), //My cards suck
                        new Fold() //I'm out of here
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(), //you people keep RAISING
                        new Condition<Player>(context => randomNum < 40), //god I just need to see this hand through
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 60), //I guess I'll just fucking call, this is my only good hand
                        new Call()
                        ),
                    new Sequence<Player>( //FINALLY Dealer, you give me SOMETHING. I raise, you freaking donkeys. 
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
             //FALL BACK SEQUENCE
             new Sequence<Player>(
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Call()
                      ),
                  new Sequence<Player>(
                      new Not<Player>(new BetIsZero()),
                      new Fold()
                      )
                  )
              )
            ));
        FCR_Tree.Update(player);
    }

    public void PreFlop_FCR_Liberal(Player player)
    {
        /*
         * Liberal players are having fun and seeing a shit ton of hands
         * 
         * Bad Hand <= 4
           Decent Hand >4 && <=8
           Good Hand >8 && <12
           Great Hand >=12
           IsChipLeader
           InPosition
           BeforeRiver
           BetIsZero
           BetPreflop
           RaisedAlready
           SomeoneHasRaised
         */

        int randomNum = UnityEngine.Random.Range(0, 100);
        //randomNum += IsInPosition_Mod(player, 10);
        //randomNum += IsChipLeader_Mod(player, 10);
        //randomNum += BeforeRiver_Mod(player, 10);
        //randomNum += BetIsZero_Mod(player, 10);
        //randomNum += BetPreFlop_Mod(player, 10);
        //randomNum += RaisedAlready_Mod(player, 10);

        preflop_FCR_Tree = new Tree<Player>(new Selector<Player>(
        #region bad hand
            new Sequence<Player>(
                new HasABadHand_PreFlop(),//BAD HANDS
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 10),//Fuck it, bad hand, let's have fun
                        new HasEnoughMoney(),
                        new Not<Player>(new SomeoneHasRaised()),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 20), //hell yeah, let's see the flop 
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new IsBigBlind(),
                        new Call() //I mean, we'll check it on the big blind. 
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 100),
                        new HasEnoughMoney(),
                        new IsSmallBlind(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
        #endregion
        #region Decent hand: The Average Hand. MAKES SENSE. 
            new Sequence<Player>(
                    new HasADecentHand_Preflop(), //DECENT HAND
                    new Selector<Player>(
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 20), //hell yeah, let's bluff
                            new HasEnoughMoney(),
                            new Not<Player>(new RaisedAlready()),
                            new Not<Player>(new SomeoneHasRaised()),
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 50), //let's see these hands
                            new HasEnoughMoney(),
                            new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) != 0),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 100), //MAYBE we'll call
                            new IsSmallBlind(),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Fold()
                            )
                        )
                    ),
        #endregion
        #region good hand
            new Sequence<Player>(
                new HasAGoodHand_Preflop(),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new AllIn()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => (player.ChipCount - Services.DealerManager.lastBet) <= 0),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 50), //LOL PAY TO PLAYYYY
                        new Not<Player>(new RaisedAlready()),
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 0),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(),
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Call()
                        )
                    )
                ),
        #endregion
        #region great hand
                new Sequence<Player>(
                    new HasAGreatHand_PreFlop(),
                    new Selector<Player>(
                        new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), //no money? great hand? ALL IN. 
                        new AllIn()
                        ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 20),
                            new Call()
                            ),
                        new Sequence<Player>(
                            new Not<Player>(new RaisedAlready()), //if I haven't raised once. I'M DEFINITELY RAISING
                            new Raise()
                            ),
                        new Sequence<Player>(
                            new Condition<Player>(context => randomNum < 50), //Oh you raised on me? FUCK YOU. Raise you back. 
                            new HasHighChanceOfWinning(),
                            new Raise()
                            ),
                        new Sequence<Player>( //Fine, let's see the flop. 
                            new Call()
                            )
                        )
                    )
        #endregion
                ));
        preflop_FCR_Tree.Update(player);
    }

    public void FCR_Liberal(Player player, float returnRate)
    {
        int randomNum = UnityEngine.Random.Range(0, 100);

        FCR_Tree = new Tree<Player>(new Selector<Player>(
            //LOW RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 0.8),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 10), //maybe a little bluff, as a treat
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 30), //let's play poker, I call
                        new Call()
                        ),
                    new Sequence<Player>( //free money
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>( //eh, maybe next time
                        new Fold()
                        )
                    )
                ),
            //MEDIUM RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.0),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(), //you're gonna put me all in?
                        new Not<Player>(new HasHighChanceOfWinning()), //ugh fine
                        new Fold() //love you dealer, but no
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 30), //let's see them cards
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 50), //Hell yeah I'm raising
                        new HasEnoughMoney(),
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(), //I mean fine. You wanna check? I'll check. 
                        new Call()
                        ),
                    new Sequence<Player>( //fuck off with these cards.  
                        new Fold()
                        )
                    )
                ),
            //HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate < 1.3),
                new Selector<Player>(
                    new Sequence<Player>(
                        new BeingPutAllIn(), //you're putting me all in
                        new Not<Player>(new HasHighChanceOfWinning()), //again?!
                        new Fold() //fine, fold
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(), //you raised?
                        new Condition<Player>(context => randomNum < 40), //I love poker
                        new Call() //LES PLAYYYY
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 70), //Pay to play. 
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 100), //Best hand I've gotten in a while, but what if I lose again?
                        new Call()
                        ),
                    new Sequence<Player>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Fold() 
                        )
                    )
                ),
            //VERY HIGH RETURN RATE
            new Sequence<Player>(
                new Condition<Player>(context => returnRate >= 1.3),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), 
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new BetIsZero(),
                        new Call() //CHECK
                        ),
                    new Sequence<Player>(
                        new BeingPutAllIn(), //you're putting me all in...
                        new Not<Player>(new HasHighChanceOfWinning()), 
                        new Fold() 
                        ),
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()), 
                        new Not<Player>(new HasHighChanceOfWinning()), 
                        new Fold() //I'm out of here
                        ),
                    new Sequence<Player>(
                        new SomeoneHasRaised(), 
                        new Condition<Player>(context => randomNum < 20), 
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 30), //I guess I'll just fucking call, this is my only good hand
                        new Call()
                        ),
                    new Sequence<Player>( //FINALLY Dealer, you give me SOMETHING. I raise, you freaking donkeys. 
                        new Raise()
                        ),
                    new Sequence<Player>(
                        new Fold()
                        )
                    )
                ),
             //FALL BACK SEQUENCE
             new Sequence<Player>(
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Call()
                      ),
                  new Sequence<Player>(
                      new Not<Player>(new BetIsZero()),
                      new Fold()
                      )
                  )
              )
            ));
        FCR_Tree.Update(player);
    }



    ///////////////////////////////
    ///////////NODES///////////////
    ///////////////////////////////

    /////////CONDITIONS///////////

    #region IsChipLeader
    private class IsChipLeader : Node<Player>
    {
        public override bool Update(Player player)
        {
            for (int i = 0; i < Services.TableManager.players.Length; i++)
            {
                if (player != Services.TableManager.players[i] && player.ChipCount < Services.TableManager.players[i].ChipCount)
                    //Debug.Log("Is not chip leader");
                    return false;
            }
            //Debug.Log("Is chip leader");
            return true;
        }
    }

    private bool IsChipLeader_Bool(Player player)
    {
        for (int i = 0; i < Services.TableManager.players.Length; i++)
        {
            if (player != Services.TableManager.players[i] && player.ChipCount < Services.TableManager.players[i].ChipCount)
                //Debug.Log("Is not chip leader");
                return false;
        }
        //Debug.Log("Is chip leader");
        return true;
    }

    private int IsChipLeader_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (IsChipLeader_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    #region InPosition?
    private class IsInPosition : Node<Player>
    {
        public override bool Update(Player player)
        {
            bool inPosition;
            if (Services.DealerManager.PlayerSeatsAwayFromDealerAmongstLivePlayers(Services.DealerManager.ActivePlayerCount() - 1) == player ||
                Services.TableManager.players[Services.TableManager.DealerPosition] == player)
            {
                //Debug.Log("Player " + player.SeatPos + " is in position");
                inPosition = true;
            }
            else
            {
                //Debug.Log("not in position");
                inPosition = false;
            }
            return inPosition;
        }
    }

    public bool IsInPosition_Bool(Player player)
    {
        bool inPosition;
        if (Services.DealerManager.PlayerSeatsAwayFromDealerAmongstLivePlayers(Services.DealerManager.ActivePlayerCount() - 1) == player ||
            Services.TableManager.players[Services.TableManager.DealerPosition] == player)
        {
            //Debug.Log("Player " + player.SeatPos + " is in position");
            inPosition = true;
        }
        else
        {
            //Debug.Log("not in position");
            inPosition = false;
        }
        return inPosition;
    }

    public int IsInPosition_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (IsInPosition_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    #region BeforeRiver
    private class BeforeRiver : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.TableManager.gameState < GameState.River) Debug.Log("before river");
            //else Debug.Log("not before river");
            return Services.TableManager.gameState < GameState.River;
        }
    }

    private bool BeforeRiver_Bool(Player player)
    {
        return Services.TableManager.gameState < GameState.River;
    }

    private int BeforeRiver_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (BeforeRiver_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    #region BetIsZero
    private class BetIsZero : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.DealerManager.lastBet == 0) Debug.Log("best is zero");
            // else Debug.Log("Bet is not zero");
            return Services.DealerManager.lastBet == 0;
        }
    }

    private bool BetIsZero_Bool(Player player)
    {
        return Services.DealerManager.lastBet == 0;
    }

    private int BetIsZero_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if(BetIsZero_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    #region BetPreflop
    private class BetPreFlop : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (Services.TableManager.gameState == GameState.Flop && player.lastAction == PlayerAction.Raise)
            {
                //Debug.Log("has best pre flop");
            }
            //else Debug.Log("has not bet preflop");
            return Services.TableManager.gameState == GameState.Flop && player.lastAction == PlayerAction.Raise;
        }
    }

    public bool BetPreFlop_Bool(Player player)
    {
        if (Services.TableManager.gameState == GameState.Flop && player.lastAction == PlayerAction.Raise)
        {
            //Debug.Log("has best pre flop");
        }
        //else Debug.Log("has not bet preflop");
        return Services.TableManager.gameState == GameState.Flop && player.lastAction == PlayerAction.Raise;
    }

    private int BetPreFlop_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (BetPreFlop_Bool(player)) mod += modAmount;
        return mod;
    }

    #endregion

    #region RaisedAlready
    private class RaisedAlready : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (player.lastAction == PlayerAction.Raise)
            {
                //Debug.Log("has raised already");
            }
            //else Debug.Log("has not raised"); 
            return player.lastAction == PlayerAction.Raise;
        }
    }

    private bool RaisedAlready_Bool(Player player)
    {
        if (player.lastAction == PlayerAction.Raise)
        {
            //Debug.Log("has raised already");
        }
        //else Debug.Log("has not raised"); 
        return player.lastAction == PlayerAction.Raise;
    }

    private int RaisedAlready_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (RaisedAlready_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    #region SomeoneHasRaise
    private class SomeoneHasRaised : Node<Player>
    {
        public override bool Update(Player player)
        {
            foreach (Player p in Services.TableManager.players)
            {
                if (p != player && p.lastAction == PlayerAction.Raise)
                {
                    //Debug.Log("Someone has raised already");
                    return true;
                }
            }
            //Debug.Log("Nobody has raised");
            return false;
        }
    }

    private bool SomeoneHasRaised_Bool(Player player)
    {
        foreach (Player p in Services.TableManager.players)
        {
            if (p != player && p.lastAction == PlayerAction.Raise)
            {
                //Debug.Log("Someone has raised already");
                return true;
            }
        }
        //Debug.Log("Nobody has raised");
        return false;
    }

    private int SomeoneHasRaise_Mod(Player player, int modAmount)
    {
        int mod = 0;
        if (SomeoneHasRaised_Bool(player)) mod += modAmount;
        return mod;
    }
    #endregion

    private class IsOnALoseStreak : Node<Player>
    {
        public override bool Update(Player player)
        {
            return player.lossCount > 5;
        }
    }

    private class HasMoreMoneyThanOpponent : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (Services.DealerManager.ActivePlayerCount() == 2)
            {
                for (int i = 0; i < Services.TableManager.players.Length; i++)
                {
                    if (Services.TableManager.players[i] != player && Services.TableManager.players[i].PlayerState == PlayerState.Playing)
                    {
                        //Debug.Log("HasMoreMoneyThanOpponent");
                        if (player.ChipCount > Services.TableManager.players[i].ChipCount) return true;
                    }
                }
            }
            //Debug.Log("Does not have more money than oppoent");
            return false;
        }
    }

    private class IsBigBlind : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.DealerManager.lastBet - player.currentBet == 0) Debug.Log("is big blind");
            //else Debug.Log("is not big blind");
            return Services.DealerManager.lastBet - player.currentBet == 0;
        }
    }

    private class IsSmallBlind : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.DealerManager.lastBet - player.currentBet == Services.TableManager.smallBlind) Debug.Log("is small blind");
            //else Debug.Log("is not small blind");
            return Services.DealerManager.lastBet - player.currentBet == Services.TableManager.smallBlind;
        }
    }

    private class HasEnoughMoney : Node<Player>
    {
        public override bool Update(Player player)
        {
            if((player.ChipCount - Services.DealerManager.lastBet) > Services.TableManager.bigBlind * 4)
            {
                //Debug.Log("Has enough money");
            }
            else
            {
                //Debug.Log("Doesn't have enough money");
            }
            return (player.ChipCount - Services.DealerManager.lastBet) > Services.TableManager.bigBlind * 4;
        }
    }

    private class BeingPutAllIn : Node<Player>
    {
        public override bool Update(Player player)
        {
            return (player.ChipCount - Services.DealerManager.lastBet) <= 0;
        }
    }

    private class HasHighChanceOfWinning : Node<Player>
    {
        public override bool Update(Player player)
        {
            return (player.HandStrength > 0.5);
        }
    }

    private class HasABadHand_PreFlop : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (player.HandStrength <= 4) Debug.Log("has a bad hand");
            //else Debug.Log("doesn't have a bad hand");
            return player.HandStrength <= 4;
        }
    }

    private class HasADecentHand_Preflop : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (player.HandStrength > 4 && player.HandStrength <= 8)
            {
                //Debug.Log("has a decent hand pre flop");
            }
            //else Debug.Log("Does not have a decent hand pre flop");
            return player.HandStrength > 4 && player.HandStrength <= 8;
        }
    }

    private class HasAGoodHand_Preflop : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (player.HandStrength > 8 && player.HandStrength < 12)
            {
                //Debug.Log("has a decent hand pre flop");
            }
            //else Debug.Log("Does not have a decent hand pre flop");
            return player.HandStrength > 9 && player.HandStrength < 12;
        }
    }

    private class HasAGreatHand_PreFlop : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (player.HandStrength >= 12) Debug.Log("Has a great hand");
            //else Debug.Log("doesn't have a GREAT hand");
            return player.HandStrength >= 12;
        }
    }

    private class HasABadHand : Node<Player>
    {
        public override bool Update(Player player)
        {
            List<Player> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            //if (rankedPlayers.Count >= Services.DealerManager.ActivePlayerCount() / 2)
            //{
            //    Debug.Log("Does not have the best hand");
            //    return player != rankedPlayers[0];
            //}
            //Debug.Log("Player + " + player.SeatPos + " has a HS of " + player.HandStrength);
            return player.HandStrength < .2f;
        }
    }

    private class HasAGoodHand : Node<Player>
    {
        public override bool Update(Player player)
        {
            List<Player> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            //if (rankedPlayers.Count >= Services.DealerManager.ActivePlayerCount() / 2)
            //{

            //    if (player != rankedPlayers[0] && player.HandStrength > .3f)
            //    {
            //        Debug.Log("has a good hand with an HS of + " + player.HandStrength);
            //    }
            //    else Debug.Log("Does not have a good hand with an HS of " + player.HandStrength);
            //    return player != rankedPlayers[0] && player.HandStrength > .3f;
            //}
            //Debug.Log("Player + " + player.SeatPos + " has a HS of " + player.HandStrength);
            return player.HandStrength > .4f;
        }
    }

    private class HasAGreathand : Node<Player>
    {
        public override bool Update(Player player)
        {
            List<Player> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            //if (rankedPlayers.Count >= Services.DealerManager.ActivePlayerCount() / 2)
            //{
            //    if (player == rankedPlayers[0]) Debug.Log("Has the best hand");
            //    else Debug.Log("Does not have the best hand");
            //    return player == rankedPlayers[0];
            //}
            //Debug.Log("Player + " + player.SeatPos + " has a HS of " + player.HandStrength);
            return player.HandStrength > .65f;
        }
    }

    //private class LostManyHandsToOpponent : Node<Player>
    //{
    //    public override bool Update(Player player)
    //    {
    //        if (Services.DealerManager.ActivePlayerCount() == 2)
    //        {
    //            for (int i = 0; i < Services.TableManager.players.Length; i++)
    //            {
    //                if (Services.TableManager.players[i] != player && Services.TableManager.players[i].PlayerState == PlayerState.Playing)
    //                {
    //                    if (player.playersLostAgainst[Services.TableManager.players[i].SeatPos] > 10) return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }
    //}

    //////////ACTIONS//////////
  
    private class Fold : Node<Player>
    {
        public override bool Update(Player player)
        {
            //Debug.Log("Player " + player.SeatPos + " is folding");
            player.Fold();
            //player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class Call : Node<Player>
    {
        public override bool Update(Player player)
        {
            //Debug.Log("Player " + player.SeatPos + " is calling");
            player.Call();
            //player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class Raise : Node<Player>
    {
        public override bool Update(Player player)
        {
            //Debug.Log("Player " + player.SeatPos + " is raising");
            player.Raise();
            //player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class AllIn : Node<Player>
    {
        public override bool Update(Player player)
        {
            //Debug.Log("Player " + player.SeatPos + " is going all in");
            player.amountToRaise = player.ChipCount;
            player.Raise();
            //player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    public void FCR_Old(Player player)
    {
        FCR_Tree = new Tree<Player>(new Selector<Player>(
              //BLUFF
              new Sequence<Player>(
                  new HasABadHand(),
                  new HasEnoughMoney(),
                  new BetIsZero(),
                  new IsInPosition(),
                  new Raise()
                  ),
              //CONTINUATION
              new Sequence<Player>(
                  new BetPreFlop(),
                  new Condition<Player>(context => player.Hand.HandValues.PokerHand <= PokerHand.OnePair),
                  new BetIsZero(),
                  new HasEnoughMoney(),
                  new Raise()
                  ),
              //SLOW PLAY
              new Sequence<Player>(
                  new HasAGreathand(),
                  new Condition<Player>(context => Services.DealerManager.ActivePlayerCount() <= 3),
                  new BeforeRiver(),
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Raise()
                          ),
                      new Sequence<Player>(
                          new Not<Player>(new BetIsZero()),
                          new Call()
                          )
                      )
                  ),
              //POSITION PLAY
              new Sequence<Player>(
                  new IsInPosition(),
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Raise()
                      ),
                      new Sequence<Player>(
                          new Not<Player>(new BetIsZero()),
                          new HasAGoodHand(),
                          new Call()
                      )
                  )
              ),
              //RAISE
              new Sequence<Player>(
                  //new HasEnoughMoney(),
                  new Not<Player>(new RaisedAlready()),
                  new HasAGreathand(),
                  new Raise()
              ),
               //CALL
               new Sequence<Player>(
                   //new HasEnoughMoney(),
                   new Selector<Player>(
                       new Sequence<Player>(
                           new HasAGoodHand(),
                           new Not<Player>(new BetIsZero()),
                           new Call()
                           ),
                       new Sequence<Player>(
                           new HasAGreathand(),
                           new Not<Player>(new BetIsZero()),
                           new Call()
                           )
                    )
               ),
              //FOLD
              new Sequence<Player>(
                  new Selector<Player>(
                      new Sequence<Player>(
                          new BetIsZero(),
                          new Call()
                      ),
                  new Sequence<Player>(
                      new Not<Player>(new BetIsZero()),
                      new Fold()
                      )
                  )
              ),
              new Fold()
              ));
        FCR_Tree.Update(player);
    }

    //public void PreFlopFoldCallRaise(Player player)
    //{
    //    if (((player.ChipCount - Services.DealerManager.lastBet) < (Services.TableManager.bigBlind * 4)) && player.HandStrength > 12)
    //    {
    //        player.AllIn();
    //    }
    //    else if (player.HandStrength > 12 && player.timesRaisedThisRound == 0) player.Raise();
    //    else if (player.HandStrength < 4)
    //    {
    //        if ((Services.DealerManager.lastBet - player.currentBet == 0) ||
    //            ((Services.DealerManager.lastBet - player.currentBet == Services.TableManager.smallBlind) &&
    //               Services.DealerManager.ActivePlayerCount() == 2) &&
    //               ((player.ChipCount - Services.DealerManager.lastBet) > (Services.TableManager.bigBlind * 4))) player.Call();
    //        else player.Fold();
    //    }
    //    else
    //    {
    //        if (Services.DealerManager.raisesInRound > 1 && player.HandStrength < 8) player.Fold();
    //        else player.Call();
    //    }
    //    //player.turnComplete = true;
    //    player.actedThisRound = true;
    //}

    public void Preflop_FCR_Neutral_Old(Player player)
    {
        preflop_FCR_Tree = new Tree<Player>(new Selector<Player>(

          //PROTECT YOUR STACK
          new Sequence<Player>(
              new Not<Player>(new HasEnoughMoney()),
              new HasAGreatHand_PreFlop(),
              new AllIn() //ACTION
              ),
          //RAISE ON A GOOD HAND
          new Sequence<Player>(
              //new NeedToProtectStack(),
              new HasAGreatHand_PreFlop(),
              new Not<Player>(new RaisedAlready()),
              new Raise() //ACTION
              ),
          //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
          new Sequence<Player>(
              //new NeedToProtectStack(),
              new Selector<Player>(
                  new Sequence<Player>(
                      new IsSmallBlind(),
                      new Call()//ACTION
                      ),
                  new Sequence<Player>(
                      new IsBigBlind(),
                      new Call()
                      ),
                  new Sequence<Player>(
                      //new Not<Player>(new HasAGreatHand_PreFlop()),
                      new Not<Player>(new HasABadHand_PreFlop()),
                      new Call()
                      )
                  )
              ),
          //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
          new Sequence<Player>(
              new SomeoneHasRaised(),
              new Condition<Player>(p => player.HandStrength > 6),
              new Call()
              ),
          //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
          new Sequence<Player>(
              new IsBigBlind(),
              new Call()
              ),
          new Fold()
          ));
        preflop_FCR_Tree.Update(player);
    }
}


