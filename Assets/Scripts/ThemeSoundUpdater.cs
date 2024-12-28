using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeSoundUpdater : MonoBehaviour
{
    public AudioClip ChristmasAudio;
    public AudioClip JungleAudio;


    public void UpdateSound()
    {
        string theme = PlayerPrefs.GetString("Theme");
        if (theme == "Jungle")
        {
            GetComponent<AudioSource>().clip = JungleAudio;
        }
        if (theme == "Christmas")
        {
            GetComponent<AudioSource>().clip = ChristmasAudio;
        }
    }
}
