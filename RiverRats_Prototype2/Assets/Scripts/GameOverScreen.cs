using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameOverReasons { VIPWins, VIPEliminated, CheatingTooMuch, TalkingTooMuch}

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverScreen;
    public Text winLoseText;
    public Text gameOverReason;
    const string WIN_TEXT = "You Win!";
    const string LOSE_TEXT = "You Lose";
    const string VIP_WIN_REASON_TEXT = "The VIP Won the Game!";
    const string VIP_LOSE_REASON_TEXT = "The VIP Lost the Game";
    const string TALK_LOSE_REASON_TEXT = "You Were Caught Trying to Manipulate Players";
    const string CHEAT_LOSE_REASON_TEXT = "You Were Caught Cheating";

    public Text score;
    public Text vipRule;
    public GameObject vipImage;

    public Text rule0;
    public GameObject rule0Image;

    public Text rule1;
    public GameObject rule1Image;

    public Text rule2;
    public GameObject rule2Image;

    public Text rule3;
    public GameObject rule3Image;

    // Start is called before the first frame update
    void Start()
    {
        Services.GameOverScreen = this;
        if (Services.TableManager.gameState == GameState.GameOver) TurnOnScreen();
        else TurnOffScreen();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GameOver(GameOverReasons gameOver)
    {
        Services.TableManager.gameState = GameState.GameOver;
        //Services.GameRules.UpdateRuleStatus();
        SetGameOverScreen(gameOver);
        TurnOnScreen();
    }


    public void SetGameOverScreen(GameOverReasons gameOver)
    {
        if (gameOver == GameOverReasons.VIPWins) winLoseText.text = WIN_TEXT;
        else winLoseText.text = LOSE_TEXT;

        gameOverReason.text = GetWinLoseReasonText(gameOver);

        vipRule.text = GetRuleText(0);

        rule0.text = GetRuleText(1);
        rule1.text = GetRuleText(2);
        rule2.text = GetRuleText(3);
        rule3.text = GetRuleText(4);

        score.text = GetScore();
        SetSuccessFailImages();
    }

    public string GetRuleText(int ruleNum)
    {
        Rule chosenRule = Services.GameRules.ChosenRules[ruleNum];

        return chosenRule.RuleText + " : " + chosenRule.RuleScore;

    }

    public void SetSuccessFailImages()
    {
        if (Services.GameRules.ChosenRules[0].RuleState == RuleState.Successful)
        {
            vipImage.GetComponent<Image>().sprite = Services.PrefabDB.SuccessMark.GetComponent<Image>().sprite;
        }
        else vipImage.GetComponent<Image>().sprite = Services.PrefabDB.FailMark.GetComponent<Image>().sprite;

        if (Services.GameRules.ChosenRules[1].RuleState == RuleState.Successful)
        {
            rule0Image.GetComponent<Image>().sprite = Services.PrefabDB.SuccessMark.GetComponent<Image>().sprite;
        }
        else rule0Image.GetComponent<Image>().sprite = Services.PrefabDB.FailMark.GetComponent<Image>().sprite;

        if (Services.GameRules.ChosenRules[2].RuleState == RuleState.Successful)
        {
            rule1Image.GetComponent<Image>().sprite = Services.PrefabDB.SuccessMark.GetComponent<Image>().sprite;
        }
        else rule1Image.GetComponent<Image>().sprite = Services.PrefabDB.FailMark.GetComponent<Image>().sprite;

        if (Services.GameRules.ChosenRules[3].RuleState == RuleState.Successful)
        {
            rule2Image.GetComponent<Image>().sprite = Services.PrefabDB.SuccessMark.GetComponent<Image>().sprite;
        }
        else rule2Image.GetComponent<Image>().sprite = Services.PrefabDB.FailMark.GetComponent<Image>().sprite;

        if (Services.GameRules.ChosenRules[4].RuleState == RuleState.Successful)
        {
            rule3Image.GetComponent<Image>().sprite = Services.PrefabDB.SuccessMark.GetComponent<Image>().sprite;
        }
        else rule3Image.GetComponent<Image>().sprite = Services.PrefabDB.FailMark.GetComponent<Image>().sprite;
    }

    public string GetScore()
    {
        string scoreString;
        int rawScore = 0;

        for (int i = 0; i < Services.GameRules.CompletedRules.Count; i++)
        {
            rawScore += Services.GameRules.ChosenRules[i].RuleScore;
        }

        scoreString = rawScore.ToString();

        return scoreString;
    }

    public string GetWinLoseReasonText(GameOverReasons gameOver)
    {
        string winLoseText = "";
        switch (gameOver)
        {
            case GameOverReasons.VIPWins:
                winLoseText = VIP_WIN_REASON_TEXT;
                break;
            case GameOverReasons.VIPEliminated:
                winLoseText = VIP_LOSE_REASON_TEXT;
                break;
            case GameOverReasons.CheatingTooMuch:
                winLoseText = CHEAT_LOSE_REASON_TEXT;
                break;
            case GameOverReasons.TalkingTooMuch:
                winLoseText = TALK_LOSE_REASON_TEXT;
                break;
            default:
                break;
        }
        return winLoseText;
    }

    public void TurnOnScreen()
    {
        gameOverScreen.SetActive(true);
    }

    public void TurnOffScreen()
    {
        gameOverScreen.SetActive(false);
    }
}
