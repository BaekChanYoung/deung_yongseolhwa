using TMPro;
using UnityEngine;

enum PullScoreMode
{
    LastScore,
    MaxScore,

}

public class PullScore : MonoBehaviour
{
    [SerializeField]
    PullScoreMode pullMode;

    [ReadOnly]
    [SerializeField]
    int score;

    TextMeshProUGUI scoreText;

    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(pullMode)
        {
            case PullScoreMode.LastScore:
                scoreText.text = GameManager.instance.pullScore().ToString();
                break;
            case PullScoreMode.MaxScore:
                scoreText.text = GameManager.instance.PullMaxScore().ToString();
                break;
        }
    }
}
