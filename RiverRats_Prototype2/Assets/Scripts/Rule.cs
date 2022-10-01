using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RuleType { TargetPlayer, Hate, Like, NoPositive, NoNegative, OutNegative, OutPositive, ShortToBig, BigToShort, RoundFiveChips, RoundTenChips, FinalTwo, FirstOut, ThreeWayWin, ThreeWayLose, VIPBigWin }
public enum RuleState { Active, Successful, Failed}

public class Rule
{
    public RuleType RuleName;
    public string RuleText;
    public int RuleScore;
    public RuleState RuleState;
    public bool RuleCompleted;
    public bool TargetPlayerProhibited;
    public Player TargetPlayer0;
    public Player TargetPlayer1;
    public Player TargetPlayer2;
    public Player TargetPlayer3;
    public Player TargetPlayer4;

    public Rule(RuleType ruleName)
    {
        RuleName = ruleName;
        RuleState = RuleState.Active;
        RuleCompleted = false;
        RuleScore = 100;
    }
    
    public Rule(RuleType ruleName, Player targetPlayer0)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        RuleState = RuleState.Active;
        RuleCompleted = false;
        RuleScore = 100;

    }

    public Rule(RuleType ruleName, Player targetPlayer0, Player targetPlayer1)
    {
        RuleName = ruleName;
        TargetPlayer0 = targetPlayer0;
        TargetPlayer1 = targetPlayer1;
        RuleState = RuleState.Active;
        RuleCompleted = false;
        RuleScore = 100;
    }
}