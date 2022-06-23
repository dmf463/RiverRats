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
        if (Services.TableManager.gameState >= GameState.Turn) minimumRaise = Services.TableManager.pot / 4;
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

    public void Preflop_FCR_Neutral(Player player)
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

    public void PreFlop_FCR_Neutral_V2(Player player)
    {
        /*
         * Bad Hand <= 4
           Decent Hand > 4 <12
           Great Hand >= 12
         */
        int randomNum = UnityEngine.Random.Range(0, 100);
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
                         )
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
                    ),
        #endregion
        #region Decent hand    
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
                            new Condition<Player>(context => randomNum < 40), //CALL FOR SHITS AND GIGLES
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
        #region good hand
            new Sequence<Player>(
                new HasAGoodHand_Preflop(),
                new Selector<Player>(
                    new Sequence<Player>(
                        new Not<Player>(new HasEnoughMoney()),
                        new AllIn()
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
                        new Condition<Player>(context => randomNum < 5), //CALL
                        new HasEnoughMoney(),
                        new Call()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 15), //RAISE
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
                        new Condition<Player>(context => randomNum < 40), //RAISE
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
                        new Not<Player>(new HasEnoughMoney()),
                        new Not<Player>(new HasHighChanceOfWinning()),
                        new Fold()
                        ),
                    new Sequence<Player>(
                        new Condition<Player>(context => randomNum < 30),
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

    ///////////////////////////////
    ///////////NODES///////////////
    ///////////////////////////////

    /////////CONDITIONS///////////
    private class IsOnALoseStreak : Node<Player>
    {
        public override bool Update(Player player)
        {
            return player.lossCount > 5;
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
            if (player.HandStrength > 4 && player.HandStrength <= 10)
            {
                //Debug.Log("has a decent hand pre flop");
            }
            //else Debug.Log("Does not have a decent hand pre flop");
            return player.HandStrength > 4 && player.HandStrength <= 10;
        }
    }

    private class HasAGoodHand_Preflop : Node<Player>
    {
        public override bool Update(Player player)
        {
            if (player.HandStrength > 10 && player.HandStrength < 14)
            {
                //Debug.Log("has a decent hand pre flop");
            }
            //else Debug.Log("Does not have a decent hand pre flop");
            return player.HandStrength > 10 && player.HandStrength < 14;
        }
    }

    private class HasAGreatHand_PreFlop : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (player.HandStrength >= 12) Debug.Log("Has a great hand");
            //else Debug.Log("doesn't have a GREAT hand");
            return player.HandStrength >= 14;
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

    private class BeforeRiver : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.TableManager.gameState < GameState.River) Debug.Log("before river");
            //else Debug.Log("not before river");
            return Services.TableManager.gameState < GameState.River;
        }
    }

    private class BetIsZero : Node<Player>
    {
        public override bool Update(Player player)
        {
            //if (Services.DealerManager.lastBet == 0) Debug.Log("best is zero");
           // else Debug.Log("Bet is not zero");
            return Services.DealerManager.lastBet == 0;
        }
    }

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
}
