using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.KhuongDuy.MMMFinger
{
    /// <summary>
    /// Manage UI
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance = null;

        public Animation faceAnim;

        public GameObject
            mouth,
            hurt,
            blood,
            SoundOnButton,
            SoundOffButton,
            finger2nd;

        public Text
            scoreInGameText,
            currentScoreText,
            bestScoreText;



        // Constructor
        private UIManager() { }

        // Behaviour messages
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        // Behaviour
        void Start()
        {
            SetScoreText(); 
            if (PlayerPrefs.GetInt("IsSoundDisabled") == 0)
            {
                SoundOffButton.SetActive(false);
            }
            else
            {
                SoundOnButton.SetActive(false);
            }
            UpdateSound();
        }

        void UpdateSound()
        {
            AudioSource[] audios = GameObject.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                if (PlayerPrefs.GetInt("IsSoundDisabled") == 1)
                {
                    audio.volume = 0f;
                }
                else
                {
                    audio.volume = 1f;
                }
            }
        }

        public void SoundToggle(bool isOn)
        {
            if (isOn)
            {
                PlayerPrefs.SetInt("IsSoundDisabled", 1);
                SoundOnButton.SetActive(false);
                SoundOffButton.SetActive(true);
            }
            else
            {
                SoundOffButton.SetActive(false);
                SoundOnButton.SetActive(true);
                PlayerPrefs.SetInt("IsSoundDisabled", 0);
            }
            UpdateSound();
        }

        private void SetScoreText()
        {
            if (GameController.Instance.GameState == GAMESTATE.OVER)
            {
                float score = Mathf.Round(GameController.Instance.MovedDistance);

                PlayerPrefs.SetFloat(Constants.LAST_SCORE, score);

                if (score > PlayerPrefs.GetFloat(Constants.BEST_SCORE, 0.0f))
                {
                    PlayerPrefs.SetFloat(Constants.BEST_SCORE, score);
                }
            }

            currentScoreText.text = "Your Score : " + PlayerPrefs.GetFloat(Constants.LAST_SCORE, 0.0f) + "";
            bestScoreText.text = "High Score : " + PlayerPrefs.GetFloat(Constants.BEST_SCORE, 0.0f);
        }

        public void StartGame()
        {
            Debug.Log("Here");
            mouth.SetActive(false);
            faceAnim.Play("start_game_effect");
        }

        public void Die()
        {
            hurt.SetActive(true);

            blood.transform.position = PlayerControl.Instance.transform.position;
            blood.SetActive(true);

            SetScoreText();

            StartCoroutine(GameOverShow());
        }

        private IEnumerator GameOverShow()
        {
            yield return new WaitForSeconds(0.8f);

            hurt.SetActive(false);
            blood.SetActive(false);
            mouth.SetActive(true);
            faceAnim.Play("game_over_effect");
        }
    }
}
