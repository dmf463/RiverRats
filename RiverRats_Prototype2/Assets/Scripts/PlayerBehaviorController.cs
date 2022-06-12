using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviorController : MonoBehaviour
{
    [Header("On Tilt/Joyous")]
    [Range(0.0f, 100.0f)]
    public float raiseMod;
    [Range(0.0f, 100.0f)]
    public float callMod;
    [Range(0.0f, 100.0f)]
    public float foldMod;

    // Start is called before the first frame update
    void Start()
    {
        Services.PlayerBehaviorController = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
