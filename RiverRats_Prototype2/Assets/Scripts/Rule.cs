using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuleType { NULL, Hate, Like }
public enum RuleState { Active, Successful, Failed}

public class Rule
{
    public RuleType RuleName;
    public string RuleText;
    public RuleState RuleState;
    public bool TargetPlayerProhibited;
    public int NoTargetPlayer;
    public int TargetPlayer0;
    public int TargetPlayer1;
    public int TargetPlayer2;
    public int TargetPlayer3;
    public int TargetPlayer4;

    public Rule(RuleType ruleName)
    {
        RuleName = ruleName;
        RuleState = RuleState.Active;
    }
    
    public Rule(RuleType ruleName, int targetPlayer0)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        RuleState = RuleState.Active;
    }

    public Rule(RuleType ruleName, int targetPlayer0, int targetPlayer1)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        TargetPlayer1 = targetPlayer1;
        RuleState = RuleState.Active;
    }
}