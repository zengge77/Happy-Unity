using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restart : MonoBehaviour
{
    public GameLogic gameLogic;

    public void Reset()
    {
        gameLogic.CreatrCandyArea();
    }
}
