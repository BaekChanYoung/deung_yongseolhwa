using TMPro;
using UnityEngine;

enum SelectDateMode
{
    MaxScore,
    Coin
}

public class PullPlayerDate : MonoBehaviour
{
    [SerializeField]
    SelectDateMode selectDateMode;

    [ReadOnly]
    [SerializeField]
    int score;

    TextMeshProUGUI dateText;

    void Start()
    {
        dateText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(selectDateMode)
        {
            case SelectDateMode.MaxScore:
                dateText.text = PlayerDataManager.instance.PullMaxScore().ToString();
                break;
            case SelectDateMode.Coin:
                dateText.text = PlayerDataManager.instance.PullCoin().ToString();
                break;
        }
    }
}
