using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string sfxID;           
    public AudioClip clip;        
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;
 
    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.sfxID == name)
            {
                return soundEffect.clip;
            }
        }
        return null;
    }
}
