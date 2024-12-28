using UnityEngine;

public class AnimationSpeedControl : MonoBehaviour
{
    public Animation anim; 
    public string clipName; 
    public float playbackSpeed = 0.1f; 

    void Update()
    {     
        if (anim == null)
            anim = GetComponent<Animation>();

        if (anim != null && anim[clipName] != null)
        {
            anim[clipName].speed = playbackSpeed;
        }
    }
}
