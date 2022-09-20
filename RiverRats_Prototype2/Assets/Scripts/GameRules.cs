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
            if(CheckRuleState(rule) == RuleState.Successful)
            {
                CompletedRules.Add(rule);
                if (rule.RuleName == RuleType.TargetPlayer) Services.UIManager.VIPSuccess.SetActive(true);
                else if (rule == ChosenRules[0]) Services.UIManager.ruleOneCheck.SetActive(true);
                else if (rule == ChosenRules[1]) Services.UIManager.ruleTwoCheck.SetActive(true);
                Debug.Log("Rule Completed: " + rule.RuleText);
            }
            else if(CheckRuleState(rule) == RuleState.Failed)
            {
                if (rule.RuleName == RuleType.TargetPlayer) Services.UIManager.VIPFail.SetActive(true);
                else if (rule == ChosenRules[0]) Services.UIManager.ruleOneFail.SetActive(true);
                else if (rule == ChosenRules[1]) Services.UIManager.ruleTwoFail.SetActive(true);
                Debug.Log("Rule Failed: " + rule.RuleText);
                FailedRules.Add(rule);
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
        else if (rule.RuleName == RuleType.NoPositive)
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
        else if (rule.RuleName == RuleType.OutPositive)
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
        return state;
    }

    private void ChooseRules()
    {
        while (ChosenRules.Count < 2)
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
                        }
                    }
                }
            }
        }
        Rule VIP = new Rule(RuleType.TargetPlayer);
        VIP.RuleText = "VIP Player Must Win the Game";
        ChosenRules.Add(VIP);
        for (int i = 0; i < ChosenRules.Count; i++)
        {
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
                     " should hate player " + RulesList[i].TargetPlayer1.SeatPos);
            }
            else if (RulesList[i].RuleName == RuleType.Like)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0.SeatPos +
                     " should like player " + RulesList[i].TargetPlayer1.SeatPos);
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
                     " should never be in a posituve emotional state");
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
        }
    }

    private void ProhibitTargePlayerRules(Player target)
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleType.Hate ||
                RulesList[i].RuleName == RuleType.Like ||
                RulesList[i].RuleName == RuleType.NoNegative ||
                RulesList[i].RuleName == RuleType.NoPositive)
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
}
