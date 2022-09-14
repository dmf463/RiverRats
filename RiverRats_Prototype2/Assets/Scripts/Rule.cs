using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rule
{
    public string RuleName;
    public int NoTargetPlayer;
    public int TargetPlayer0;
    public int TargetPlayer1;
    public int TargetPlayer2;
    public int TargetPlayer3;
    public int TargetPlayer4;

    public Rule(string ruleName)
    {
        RuleName = ruleName;
    }
    
    public Rule(string ruleName, int targetPlayer0)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
    }

    public Rule(string ruleName, int targetPlayer0, int targetPlayer1)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        TargetPlayer1 = targetPlayer1;
    }
}
