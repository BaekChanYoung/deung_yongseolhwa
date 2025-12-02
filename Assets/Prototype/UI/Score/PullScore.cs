using TMPro;
using UnityEngine;

enum PullMode
{
    LastScore,
    MaxScore
}

public class PullScore : MonoBehaviour
{
    [SerializeField]
    PullMode pullMode;

    [ReadOnly]
    [SerializeField]
    int score;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch(pullMode)
        {
            case PullMode.LastScore:
                GetComponent<TextMeshProUGUI>().text = GameManager.instance.pullScore().ToString();
                break;
            case PullMode.MaxScore:
                GetComponent<TextMeshProUGUI>().text = GameManager.instance.PullMaxScore().ToString();
                break;
        }
    }
}
