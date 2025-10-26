using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DownModeChangeButten : MonoBehaviour
{
    DownMovementMode DownMode;

    [SerializeField]
    float SmoothDownScale;

    void Start()
    {
        DownMode = DownMovementMode.SmoothDamp;
    }

    public void ModeChange()
    {
        if (DownMode == DownMovementMode.moveToword)
            DownMode = DownMovementMode.SmoothDamp;    
        else if (DownMode == DownMovementMode.SmoothDamp)
            DownMode = DownMovementMode.moveToword;

        GameManager.instance.player.DownMode = DownMode;
        GameManager.instance.enemy.DownMode = DownMode;
        GameManager.instance.background.DownMode = DownMode;

        if (DownMode == DownMovementMode.SmoothDamp)
        {
            GameManager.instance.player.PlayerDownDuration *= SmoothDownScale;
            GameManager.instance.enemy.EnemyDownDuration *= SmoothDownScale;
            GameManager.instance.background.BackgroundDownDuration *= SmoothDownScale;
        }
        if (DownMode == DownMovementMode.moveToword)
        {
            GameManager.instance.player.PlayerDownDuration /= SmoothDownScale;
            GameManager.instance.enemy.EnemyDownDuration /= SmoothDownScale;
            GameManager.instance.background.BackgroundDownDuration /= SmoothDownScale;
        }
    }
}
