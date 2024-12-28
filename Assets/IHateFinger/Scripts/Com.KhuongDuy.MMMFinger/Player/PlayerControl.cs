using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

namespace Com.KhuongDuy.MMMFinger
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer playerSpriteRenderer;

        public GameObject ThemeSelecter;

        private Sprite moveLeftSprite;
        private Sprite moveRightSprite;
        private Sprite moveStraightSprite;

        public Sprite ChristmasMoveLeftSprite;
        public Sprite ChristmasMoveRightSprite;
        public Sprite ChristmasMoveStraightSprite;
        public Sprite JungleMoveLeftSprite;
        public Sprite JungleMoveRightSprite;
        public Sprite JungleMoveStraightSprite;

        public Text CoinsTxt;

        private Vector3 m_fingerPosition;
        private bool m_fingerOnScreen;


        private PointerEventData pointer;
        private List<RaycastResult> raycastResult;

        private Vector3 previousPosition;
        private float movementSmoothingFactor = 1f; 
        private float directionCheckTimer = 0f;
        private float directionUpdateInterval = 0.1f; 

        public static PlayerControl Instance = null;

        public float padding;

        public bool FingerOnScreen
        {
            get { return m_fingerOnScreen; }
            set { m_fingerOnScreen = value; }
        }

        int coinsCollected = 0;
        public GameObject CoinCollect;

        public ThemeSoundUpdater[] sounds;

        private PlayerControl() { }

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

        void Start()
        {
            pointer = new PointerEventData(EventSystem.current);
            raycastResult = new List<RaycastResult>();
            previousPosition = transform.position;
            LoadTheme();
        }

        public void LoadTheme()
        {
            string theme = PlayerPrefs.GetString("Theme", "Jungle");            
            if(theme == "Jungle")
            {                
                moveLeftSprite = JungleMoveLeftSprite;
                moveRightSprite = JungleMoveRightSprite;
                moveStraightSprite = JungleMoveStraightSprite;
            }

            if (theme == "Christmas")
            {
                moveLeftSprite = ChristmasMoveLeftSprite;
                moveRightSprite = ChristmasMoveRightSprite;
                moveStraightSprite = ChristmasMoveStraightSprite;
            }

            GetComponent<SpriteRenderer>().sprite = moveStraightSprite;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && GameController.Instance.GameState != GAMESTATE.OVER && !FindAnyObjectByType<InterstitialAdManager>()._isAdOpen)
            {
                pointer.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                EventSystem.current.RaycastAll(pointer, raycastResult);

                bool isButtonPressed = false;
                foreach (var result in raycastResult)
                {
                    if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                    {
                        isButtonPressed = true;
                        GameBegin();
                        break;
                    }
                }

                if (!isButtonPressed)
                {
                    if (!m_fingerOnScreen)
                    {
                        m_fingerOnScreen = true;

                        if (raycastResult.Count == 0)
                        {
                            FindAnyObjectByType<Banner>().DestroyAd();
                        }
                        else
                        {
                            UIManager.Instance.StartGame();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (m_fingerOnScreen)
                {
                    if (GameController.Instance.GameState == GAMESTATE.START)
                    {
                        m_fingerOnScreen = false;
                        UIManager.Instance.finger2nd.SetActive(true);
                    }
                    else if (GameController.Instance.GameState == GAMESTATE.PLAY)
                    {
                        Die();
                    }
                }
            }

            if (m_fingerOnScreen && GameController.Instance.GameState == GAMESTATE.PLAY)
            {
                FingerMove();
                CheckCollision();

                GameController.Instance.CalculateScore();
                GameController.Instance.SpawnEnemy();
            }
        }

        public void ThemeSelected(string theme)
        {
            PlayerPrefs.SetString("Theme", theme);

            foreach (ThemeSoundUpdater s in sounds)
            {
                s.UpdateSound();
            }

            ThemeSoundUpdater[] SoundUpdater = GameObject.FindObjectsOfType<ThemeSoundUpdater>();
            foreach (ThemeSoundUpdater t in SoundUpdater)
            {
                t.gameObject.GetComponent<AudioSource>().Stop();
                t.UpdateSound();
                if (t.gameObject.GetComponent<AudioSource>().playOnAwake)
                {
                    t.gameObject.GetComponent<AudioSource>().Play();
                }
            }

            ThemeManager[] themeManagers = GameObject.FindObjectsOfType<ThemeManager>();
            foreach (ThemeManager t in themeManagers)
            {
                t.LoadTheme();
            }
            ThemeSelecter.SetActive(false);
            LoadTheme();
            GameBegin();
            UIManager.Instance.StartGame();
            transform.position = Vector3.zero;
        }

        public void GameBegin()
        {
            if (!GameController.Instance.soundPlay.isPlaying)
            {
                GameController.Instance.soundPlay.Play();
            }

            if (UIManager.Instance.finger2nd.activeInHierarchy)
            {
                UIManager.Instance.finger2nd.SetActive(false);
            }

            GameController.Instance.GameState = GAMESTATE.PLAY;
            GameController.Instance.gameJustStarted = true;

            Vector3 newPos = pointer.position;
            newPos.y += padding;
            newPos.z = 0;

            transform.position = newPos;
        }

        private void FingerMove()
        {
            m_fingerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_fingerPosition.y += padding;
            m_fingerPosition.z = 0.0f;
            transform.position = Vector3.Lerp(transform.position, m_fingerPosition, movementSmoothingFactor); // Smooth movement

            directionCheckTimer += Time.deltaTime;
            if (directionCheckTimer >= directionUpdateInterval)
            {
                UpdateSpriteBasedOnDirection();
                directionCheckTimer = 0f;
            }
        }

        private void UpdateSpriteBasedOnDirection()
        {
            Vector3 currentPosition = transform.position;
            float deltaX = currentPosition.x - previousPosition.x;

            if (Mathf.Abs(deltaX) > 0.1f) // Avoid micro adjustments
            {
                if (deltaX > 0)
                {
                    playerSpriteRenderer.sprite = moveRightSprite;
                }
                else if (deltaX < 0)
                {
                    playerSpriteRenderer.sprite = moveLeftSprite;
                }
            }
            else
            {
                playerSpriteRenderer.sprite = moveStraightSprite;
            }

            previousPosition = currentPosition;
        }
        private void CheckCollision()
        {
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

            if (boxCollider == null)
            {
                Debug.LogError("BoxCollider2D is missing on this GameObject.");
                return;
            }

            Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0);

            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    Die();
                    return; // Exit after detecting one collision
                }
                if(col.CompareTag("Coin"))
                {
                    col.gameObject.SetActive(false);
                    StartCoroutine(ActiveObject(col.gameObject, 8f));
                    int haveCoins = PlayerPrefs.GetInt("Coins");
                    haveCoins++;
                    coinsCollected++;
                    if (PlayerPrefs.GetInt("IsSoundDisabled") == 0)
                    {
                        GameObject c = Instantiate(CoinCollect);
                        Destroy(c, 1f);
                    }
                    PlayerPrefs.SetInt("Coins", haveCoins);
                    CoinsTxt.text = coinsCollected.ToString();
                }
            }
        }

        IEnumerator ActiveObject(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            obj.SetActive(true);
        }


        private void Die()
        {
            m_fingerOnScreen = false;

            if (!GameController.Instance.soundDie.isPlaying)
            {
                GameController.Instance.soundPlay.Stop();
                GameController.Instance.soundDie.Play();
            }

            GameController.Instance.GameState = GAMESTATE.OVER;

            if (Random.Range(1, 3) == 2)
            {
                FindAnyObjectByType<Banner>().LoadAd();
            }
            else
            {
                FindAnyObjectByType<InterstitialAdManager>().ShowInterstitialAd();
            }

            UIManager.Instance.Die();
        }
    }
}
