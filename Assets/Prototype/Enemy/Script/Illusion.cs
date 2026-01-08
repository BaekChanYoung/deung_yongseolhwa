using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Illusion : MonoBehaviour
{
    [SerializeField]
    ParticleSystem HitEffect;

    void HitIllusion()
    {
        //Instantiate(HitEffect,transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            HitIllusion();
        }
    }
}
