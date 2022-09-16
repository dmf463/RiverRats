using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour
{
    public Player targetPlayer;
    public List<Rule> RulesList;
    public List<Rule> ActiveRules = new List<Rule>();

    // Start is called before the first frame update
    void Start()
    {
        Services.GameRules = this;
        RulesList = this.gameObject.GetComponent<CSV_Parser>().OrganizedRules;
        AddRuleDescription();
        targetPlayer = Services.TableManager.players[Random.Range(0, 5)];
        Debug.Log("VIP = Player" + targetPlayer.SeatPos);
        ChooseRules();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseRules()
    {
        while(ActiveRules.Count < 2)
        {
            int randomNum = Random.Range(0, RulesList.Count);
            for(int i = 0; i < RulesList.Count; i++)
            {
                if(i == randomNum)
                {
                    if (!ActiveRules.Contains(RulesList[randomNum]))
                    {
                        ActiveRules.Add(RulesList[randomNum]);
                    }
                }
            }
        }

        for (int i = 0; i < ActiveRules.Count; i++)
        {
            Debug.Log("Rule" + i + " = " + ActiveRules[i].RuleText);
        }
    }

    public void AddRuleDescription()
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleNames.Hate)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0 +
                     " should hate player " + RulesList[i].TargetPlayer1);
            }
            else if (RulesList[i].RuleName == RuleNames.Like)
            {
                RulesList[i].RuleText =
                    ("Player " + RulesList[i].TargetPlayer0 +
                     " should like player " + RulesList[i].TargetPlayer1);
            }
        }
    }

    public void ProhibitTargePlayerRules()
    {
        for(int i = 0; i < RulesList.Count; i++)
        {
            if (RulesList[i].RuleName == RuleNames.Hate ||
                RulesList[i].RuleName == RuleNames.Like)
            {
                RulesList[i].TargetPlayerProhibited = false;
            }
        }
    }
}
