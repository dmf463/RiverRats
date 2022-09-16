using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuleNames { NULL, Hate, Like }

public class Rule
{
    public RuleNames RuleName;
    public string RuleText;
    public bool TargetPlayerProhibited;
    public int NoTargetPlayer;
    public int TargetPlayer0;
    public int TargetPlayer1;
    public int TargetPlayer2;
    public int TargetPlayer3;
    public int TargetPlayer4;

    public Rule(RuleNames ruleName)
    {
        RuleName = ruleName;
    }
    
    public Rule(RuleNames ruleName, int targetPlayer0)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
    }

    public Rule(RuleNames ruleName, int targetPlayer0, int targetPlayer1)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        TargetPlayer1 = targetPlayer1;
    }
}