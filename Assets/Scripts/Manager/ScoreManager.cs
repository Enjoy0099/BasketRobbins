using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public int combo = 0;

    public void AddScore(bool isSwish)
    {
        if (isSwish)
        {
            combo++;
            int bonus = combo * 2;
            score += 10 + bonus;

            Debug.Log("SWISH! Combo x" + combo);
        }
        else
        {
            combo = 0;
            score += 10;

            Debug.Log("Normal Score");
        }

        Debug.Log("Total Score: " + score);
    }
}
