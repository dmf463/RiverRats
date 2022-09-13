using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour
{
    public Player targetPlayer;

    // Start is called before the first frame update
    void Start()
    {
        Services.GameRules = this;
        targetPlayer = Services.TableManager.players[Random.Range(0, 5)];
        Debug.Log("VIP = Player" + targetPlayer.SeatPos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
