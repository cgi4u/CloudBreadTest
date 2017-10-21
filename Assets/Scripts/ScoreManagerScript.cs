using UnityEngine;
using System.Collections;

using Assets.Scripts.CloudBread;

public class ScoreManagerScript : MonoBehaviour {

    public static int Score { get; set; }

    private bool flag;

	// Use this for initialization
	void Start () {
        (Tens.gameObject as GameObject).SetActive(false);
        (Hundreds.gameObject as GameObject).SetActive(false);
        flag = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (previousScore != Score) //save perf from non needed calculations
            {
                if (Score < 10)
                {
                    //just draw units
                    Units.sprite = numberSprites[Score];
                }
                else if (Score >= 10 && Score < 100)
                {
                    (Tens.gameObject as GameObject).SetActive(true);
                    Tens.sprite = numberSprites[Score / 10];
                    Units.sprite = numberSprites[Score % 10];
                }
                else if (Score >= 100)
                {
                    (Hundreds.gameObject as GameObject).SetActive(true);
                    Hundreds.sprite = numberSprites[Score / 100];
                    int rest = Score % 100;
                    Tens.sprite = numberSprites[rest / 10];
                    Units.sprite = numberSprites[rest % 10];
                }
            }
        }
        else if (GameStateManager.GameState == GameState.Dead)
        {
            if (flag)
            {
                flag = false;
                PlayerPrefs.SetInt("bestScore", Score);
                CloudBread cb = new CloudBread();
                cb.CBComUdtMemberGameInfoes(Callback_Success);
            }
        }
	}

    public void Callback_Success(string id, WWW www)
{
    print("[" + id + "] Success");
}

    int previousScore = -1;
    public Sprite[] numberSprites;
    public SpriteRenderer Units, Tens, Hundreds;
}
