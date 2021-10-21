using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCurrentState : MonoBehaviour
{
    public Text stateText;

    public void UpdateState(string newState)
    {
        stateText.text = newState;
    }
}
