using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour
{
    public Player targetPlayer;
    public List<Rule> RulesList;
    public List<Rule> ChosenRules = new List<Rule>();
    public List<Rule> CompletedRules = new List<Rule>();
    public List<Rule> FailedRules = new List<Rule>();

    // Start is called before the first frame update
    void Start()
    {
        Services.GameRules = this;
        RulesList = this.gameObject.GetComponent<CSV_Parser>().OrganizedRules;
        AddRuleDescription();
        targetPlayer = Services.TableManager.players[Random.Range(0, 5)];
        ProhibitTargePlayerRules(targetPlayer);
        Debug.Log("VIP = Player" + targetPlayer.SeatPos);
        Debug.Log("rules list count = " + RulesList.Count);
        ChooseRules();
    }

    // Update is called once per frame
    void Update()
    {
        /*
         * What I want to do is:
         * Each frame, look through the game state and determine:
         *        is this rule successful?
         *        is it failed?
         *        is it in progress?
         */
        UpdateRuleStatus();
    }

    private void UpdateRuleStatus()
    {
        foreach (Rule rule in ChosenRules)
        {
            if (!rule.RuleCompleted)
            {
                if (CheckRuleState(rule) == RuleState.Successful)
                {
                    CompletedRules.Add(rule);
                    rule.RuleCompleted = true;
                    if (rule.RuleName == RuleType.TargetPlayer) Services.UIManager.VIPSuccess.SetActive(true);
                    else if (rule == ChosenRules[1]) Services.UIManager.ruleZeroCheck.SetActive(true);
                    else if (rule == ChosenRules[2]) Services.UIManager.ruleOneCheck.SetActive(true);
                    else if (rule == ChosenRules[3]) Services.UIManager.ruleTwoCheck.SetActive(true);
                    Debug.Log("Rule Completed: " + rule.RuleText);
                }
                else if (CheckRuleState(rule) == RuleState.Failed)
                {
                    rule.RuleCompleted = true;
                    if (rule.RuleName == RuleType.TargetPlayer) Services.UIManager.VIPFail.SetActive(true);
                    else if (rule == ChosenRules[1]) Services.UIManager.ruleZeroFail.SetActive(true);
                    else if (rule == ChosenRules[2]) Services.UIManager.ruleOneFail.SetActive(true);
                    else if (rule == ChosenRules[3]) Services.UIManager.ruleTwoFail.SetActive(true);
                    Debug.Log("Rule Failed: " + rule.RuleText);
                    FailedRules.Add(rule);
                }
            }
        }
    }

    private RuleState CheckRuleState(Rule rule)
    {
        RuleState state = RuleState.Active;

        if(rule.RuleName == RuleType.TargetPlayer)
        {
            if(targetPlayer.PlayerState == PlayerState.Eliminated)
            {
                state = RuleState.Failed;
            }
            else if(Services.TableManager.gameState == GameState.GameOver)
            {
                if(targetPlayer.PlayerState != PlayerState.Eliminated)
                {
                    state = RuleState.Successful;
                }
            }
        }
        else if(rule.RuleName == RuleType.Hate)
        {
            PlayerEmotion target0 = rule.TargetPlayer0.PlayerEmotion;
            PlayerEmotion target1 = rule.TargetPlayer1.PlayerEmotion;

            if (target0 == PlayerEmotion.OnTilt && target1 == PlayerEmotion.Joyous)
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Angry && target1 == PlayerEmotion.Happy)
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Annoyed && target1 == PlayerEmotion.Amused)
            {
                state = RuleState.Successful;
            }
            else state = RuleState.Active;
        }
        else if(rule.RuleName == RuleType.Like)
        {
            PlayerEmotion target0 = rule.TargetPlayer0.PlayerEmotion;
            PlayerEmotion target1 = rule.TargetPlayer1.PlayerEmotion;
            if (target0 == PlayerEmotion.Amused && target1 == PlayerEmotion.Amused) //Ammused:Amused
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Amused && target1 == PlayerEmotion.Happy) //Amused:Happy
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Amused && target1 == PlayerEmotion.Joyous) //Amused:Joyous
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Happy && target1 == PlayerEmotion.Amused) //Happy:Amused
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Happy && target1 == PlayerEmotion.Happy) //Happy:Happy
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Happy && target1 == PlayerEmotion.Joyous) //Happy:Joyous
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Joyous && target1 == PlayerEmotion.Amused) //Joyous:Amused
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Joyous && target1 == PlayerEmotion.Happy) //Joyous:Happy
            {
                state = RuleState.Successful;
            }
            else if (target0 == PlayerEmotion.Joyous && target1 == PlayerEmotion.Joyous) //Joyous:Joyous
            {
                state = RuleState.Successful;
            }
            else state = RuleState.Active;
        }
        else if(rule.RuleName == RuleType.NoNegative)
        {
            //if they hit a neg state, FAIL
            if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed ||
               rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
               rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt)
            {
                state = RuleState.Failed;
            }
            //if they get eliminated...
            else if (rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                //but they're in a good mood, SUCCESS!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Content ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous)
                {
                    state = RuleState.Successful;
                }
                //but they're in a bad mood, FAIL!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt ||
                    rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
                    rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed)
                {
                    state = RuleState.Failed;
                }
            }
            //if the game ENDS...
            else if (Services.TableManager.gameState == GameState.GameOver)
            {
                //but they're in a good mood, SUCCESS!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Content ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous)
                {
                    state = RuleState.Successful;
                }
                //but they're in a bad mood, FAIL!
                else if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed)
                {
                    state = RuleState.Failed;
                }
            }
            else state = RuleState.Active;
        }
        else if(rule.RuleName == RuleType.NoPositive)
        {
            //if they hit a POS state, FAIL
            if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
               rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy ||
               rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous)
            {
                state = RuleState.Failed;
            }
            //if they get eliminated...
            else if (rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                //but they're in a bad mood, SUCCESS!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Content ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt)
                {
                    state = RuleState.Successful;
                }
                //but they're in a good mood, FAIL!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
                    rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy ||
                    rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous)
                {
                    state = RuleState.Failed;
                }
            }
            //if the game ENDS...
            else if (Services.TableManager.gameState == GameState.GameOver)
            {
                //but they're in a bad mood, SUCCESS!
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Content ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed)
                {
                    state = RuleState.Successful;
                }
                //but they're in a good mood, FAIL!
                else if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
                     rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy)
                {
                    state = RuleState.Failed;
                }
            }
            else state = RuleState.Active;
        }
        else if(rule.RuleName == RuleType.OutNegative)
        {
            if(rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Annoyed ||
                   rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Angry ||
                   rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.OnTilt)
                {
                    state = RuleState.Successful;
                }
                else state = RuleState.Failed;
            }
        }
        else if(rule.RuleName == RuleType.OutPositive)
        {
            if (rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                if (rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Amused ||
                   rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Happy ||
                   rule.TargetPlayer0.PlayerEmotion == PlayerEmotion.Joyous)
                {
                    state = RuleState.Successful;
                }
                else state = RuleState.Failed;
            }
        }
        else if(rule.RuleName == RuleType.RoundFiveChips)
        {
            if(Services.DealerManager.roundCount == 6)
            {
                if (rule.TargetPlayer0.ChipCount >= 3000 && rule.TargetPlayer0.ChipCount <= 4000)
                {
                    state = RuleState.Successful;
                }
                else state = RuleState.Failed;
            }
        }
        else if(rule.RuleName == RuleType.RoundTenChips)
        {
            if (Services.DealerManager.roundCount == 6)
            {
                if (rule.TargetPlayer0.ChipCount >= 3000 && rule.TargetPlayer0.ChipCount <= 4000)
                {
                    state = RuleState.Successful;
                }
                else state = RuleState.Failed;
            }
        }
        else if(rule.RuleName == RuleType.ShortToBig)
        {
            if(rule.TargetPlayer0.shortStack && rule.TargetPlayer0.bigStack)
            {
                state = RuleState.Successful;
                rule.TargetPlayer0.shortStack = false;
                rule.TargetPlayer0.bigStack = false;
            }
            else if(rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                state = RuleState.Failed;
            }
            else if(Services.TableManager.gameState == GameState.GameOver)
            {
                state = RuleState.Failed;
            }
        }
        else if(rule.RuleName == RuleType.BigToShort)
        {
            if (rule.TargetPlayer0.bigStack && rule.TargetPlayer0.shortStack)
            {
                state = RuleState.Successful;
                rule.TargetPlayer0.shortStack = false;
                rule.TargetPlayer0.bigStack = false;
            }
            else if (rule.TargetPlayer0.PlayerState == PlayerState.Eliminated)
            {
                state = RuleState.Failed;
            }
            else if (Services.TableManager.gameState == GameState.GameOver)
            {
                state = RuleState.Failed;
            }
        }
        
        return state;
    }

    private void ChooseRules()
    {
        Rule VIP = new Rule(RuleType.TargetPlayer)
        {
            RuleText = "Player " + targetPlayer.SeatPos + " Must Win the Game"
        };
        ChosenRules.Add(VIP);
        while (ChosenRules.Count < 4)
        {
            int randomNum = Random.Range(0, RulesList.Count);
            for(int i = 0; i < RulesList.Count; i++)
            {
                if(i == randomNum)
                {
                    if (!ChosenRules.Contains(RulesList[randomNum]))
                    {
                        if (!RulesList[randomNum].TargetPlayerProhibited)
                        {
                            ChosenRules.Add(RulesList[randomNum]);
                            BanRules(randomNum);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < ChosenRules.Count; i++)
        {
            Debug.Log("Rule " + i + " is " + ChosenRules[i].RuleName);
            Debug.Log("Rule" + i + " = " + ChosenRules[i].RuleText);
        }
    }

    private void AddRuleDescription()
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleType.Hate)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should hate player " + RulesList[i].TargetPlayer1.SeatPos + " at some point in the game");
            }
            else if (RulesList[i].RuleName == RuleType.Like)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should like player " + RulesList[i].TargetPlayer1.SeatPos + " at some point in the game");
            }
            else if (RulesList[i].RuleName == RuleType.NoNegative)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should never be in a negative emotional state");
            }
            else if (RulesList[i].RuleName == RuleType.NoPositive)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should never be in a positive emotional state");
            }
            else if (RulesList[i].RuleName == RuleType.OutNegative)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should be eliminated while in a negative emotional state");
            }
            else if (RulesList[i].RuleName == RuleType.OutPositive)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should be eliminated while in a positive emotional state");
            }
            else if (RulesList[i].RuleName == RuleType.RoundFiveChips)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should have between 3000-4000 chips by the end of ROUND 5");
            }
            else if (RulesList[i].RuleName == RuleType.RoundTenChips)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should have between 3000-4000 chips by the end of ROUND 10");
            }
            else if (RulesList[i].RuleName == RuleType.ShortToBig)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should go from having less than 500 chips to more than 6000 at some point in the game");
            }
            else if (RulesList[i].RuleName == RuleType.BigToShort)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should go from more than 6000 to less than 500 chips at some point in the game");
            }
        }
    }

    private void ProhibitTargePlayerRules(Player target)
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleType.Hate ||
                RulesList[i].RuleName == RuleType.Like ||
                RulesList[i].RuleName == RuleType.NoNegative ||
                RulesList[i].RuleName == RuleType.NoPositive ||
                RulesList[i].RuleName == RuleType.BigToShort ||
                RulesList[i].RuleName == RuleType.ShortToBig ||
                RulesList[i].RuleName == RuleType.RoundFiveChips ||
                RulesList[i].RuleName == RuleType.RoundTenChips)
            {
                RulesList[i].TargetPlayerProhibited = false;
            }
            else if (RulesList[i].RuleName == RuleType.OutNegative || RulesList[i].RuleName == RuleType.OutPositive)
            {
                if (RulesList[i].TargetPlayer0.SeatPos == target.SeatPos)
                {
                    RulesList[i].TargetPlayerProhibited = true;
                }
            }
        }
    }

    private void BanRules(int randomNum)
    {
        //if we get a rule for a player to never be positive, we don't want to also get a rule saying to never be negative
        if (RulesList[randomNum].RuleName == RuleType.NoPositive)
        {
            for (int banRule = 0; banRule < RulesList.Count; banRule++)
            {
                if (RulesList[banRule].RuleName == RuleType.NoNegative &&
                    RulesList[banRule].TargetPlayer0.SeatPos == RulesList[randomNum].TargetPlayer0.SeatPos)
                {
                    RulesList[banRule].TargetPlayerProhibited = true;
                }
            }
        }
        //if we get a rule for a player to never be negative, we don't want to also get a rule saying to never be posiyive
        else if (RulesList[randomNum].RuleName == RuleType.NoNegative)
        {
            for (int banRule = 0; banRule < RulesList.Count; banRule++)
            {
                if (RulesList[banRule].RuleName == RuleType.NoPositive &&
                    RulesList[banRule].TargetPlayer0.SeatPos == RulesList[randomNum].TargetPlayer0.SeatPos)
                {
                    RulesList[banRule].TargetPlayerProhibited = true;
                }
            }
        }
        //if we ask for a player to go out on a negative, we can't ask them to go out on a positive!
        else if (RulesList[randomNum].RuleName == RuleType.OutNegative)
        {
            for (int banRule = 0; banRule < RulesList.Count; banRule++)
            {
                if (RulesList[banRule].RuleName == RuleType.OutPositive &&
                    RulesList[banRule].TargetPlayer0.SeatPos == RulesList[randomNum].TargetPlayer0.SeatPos)
                {
                    RulesList[banRule].TargetPlayerProhibited = true;
                }
            }
        }
        //if we ask for a player to go out on a positive, we can't ask them to go out on a negative!
        else if (RulesList[randomNum].RuleName == RuleType.OutPositive)
        {
            for (int banRule = 0; banRule < RulesList.Count; banRule++)
            {
                if (RulesList[banRule].RuleName == RuleType.OutNegative &&
                    RulesList[banRule].TargetPlayer0.SeatPos == RulesList[randomNum].TargetPlayer0.SeatPos)
                {
                    RulesList[banRule].TargetPlayerProhibited = true;
                }
            }
        }
    }
}
