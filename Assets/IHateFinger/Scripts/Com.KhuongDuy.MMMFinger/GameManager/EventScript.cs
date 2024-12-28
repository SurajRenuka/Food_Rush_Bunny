using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Com.KhuongDuy.MMMFinger
{
    public class EventScript : MonoBehaviour
    {
        // Constructor
        private EventScript() { }

        // Animation event when game just started
        public void CheckHold()
        {
            if (PlayerControl.Instance.FingerOnScreen)
            {
                PlayerControl.Instance.GameBegin();
            }
        }

        // Animation event when game over
        public void ResetGame()
        {
            Debug.Log("Play");
            SceneManager.LoadScene("Play");
        }
    }
}
