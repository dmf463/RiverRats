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
                if (rule == ChosenRules[0]) Services.UIManager.ruleOneCheck.SetActive(true);
                else if (rule == ChosenRules[1]) Services.UIManager.ruleTwoCheck.SetActive(true);
                Debug.Log("Rule Completed: " + rule.RuleText);
            }
            else if(CheckRuleState(rule) == RuleState.Failed)
            {
                FailedRules.Add(rule);
            }
        }
    }

    private RuleState CheckRuleState(Rule rule)
    {
        RuleState state = RuleState.Active;
        PlayerEmotion target0 = rule.TargetPlayer0.PlayerEmotion;
        PlayerEmotion target1 = rule.TargetPlayer1.PlayerEmotion;

        if(rule.RuleName == RuleType.Hate)
        {
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
                        ChosenRules.Add(RulesList[randomNum]);
                    }
                }
            }
        }

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
        }
    }

    private void ProhibitTargePlayerRules()
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleType.Hate ||
                RulesList[i].RuleName == RuleType.Like)
            {
                RulesList[i].TargetPlayerProhibited = false;
            }
        }
    }
}
