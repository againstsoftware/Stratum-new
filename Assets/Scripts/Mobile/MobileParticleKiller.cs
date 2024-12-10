
using System;
using UnityEngine;

public class MobileParticleKiller : MonoBehaviour
{
    private void Awake()
    {
        if (MobileDetection.Mobile)
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            {
                Destroy(ps);
            }
        }
    }
}
