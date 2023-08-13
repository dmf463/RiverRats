using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverScript : MonoBehaviour
{
    public GameObject message; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowUI()
    {
        //Debug.Log("Hovering");
        message.SetActive(true);
    }

    public void HideUI()
    {
        //Debug.Log("stop hovering");
        message.SetActive(false);
    }
}
