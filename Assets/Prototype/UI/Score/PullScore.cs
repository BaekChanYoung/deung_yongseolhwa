using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PullScore : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    int score;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = GameManager.instance.pullScore().ToString();
    }
}
