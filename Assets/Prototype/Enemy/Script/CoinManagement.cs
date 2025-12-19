using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManagement : MonoBehaviour
{
    [SerializeField]
    public int coin;

    void Awake()
    {

    }

    public void DropCoin()
    {
        Debug.Log("적이 코인을 떨굼" + coin);
        PlayerDataManager.instance.TakeCoin(coin);
    }

    public void addCoin(int addcoin)
    {
        coin += addcoin;
    }
}
