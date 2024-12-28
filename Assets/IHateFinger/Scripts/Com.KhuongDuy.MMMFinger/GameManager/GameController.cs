using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com.KhuongDuy.MMMFinger
{
    /// <summary>
    /// Manage game logic
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] m_enemiesArray;
        private GameObject m_lastBornEnemy;

        private List<Animation> m_enemiesAnim;

        private float[] m_timeRespawn;

        private float m_movedDistance;

        [SerializeField]
        [Tooltip("Increase scores received when move")]
        private float m_scaleDistance = 10.0f;

        private bool
            m_wait,
            m_firstRandomEnemy;

        public static GameController Instance = null;

        public GameObject effect;

        public GameObject[] EnemiesArray
        {
            get { return m_enemiesArray; }
            set { m_enemiesArray = value; }
        }

        public AudioSource
            soundPlay,
            soundDie;

        public float MovedDistance
        {
            get { return m_movedDistance;}
            set { m_movedDistance = value; }
        }

        public float SpeedRunDown { get; set; }

        public GAMESTATE GameState { get; set; }

        public bool gameJustStarted { get; set; }

        // Constructor
        private GameController() { }

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

        // Behaviour messages
        void Start()
        {
            Init();
        }

        private void Init()
        {
			Application.targetFrameRate = 60;
            SetEnemiesArray();

            m_timeRespawn = new float[11];

            SetTimeRespawnLvl1();

            GameState = GAMESTATE.START;

            m_movedDistance = 0.0f;
            UIManager.Instance.scoreInGameText.text = m_movedDistance + "";
        }

        private void SetEnemiesArray()
        {
            m_enemiesArray = new GameObject[20];

            for (int i = 1; i <= 20; i++)
            {
                string tag = "";
                if (i <= 10)
                {
                    tag = "Group" + i;
                }
                else
                {
                    tag = "Group" + (i - 10);
                }

                m_enemiesArray[i - 1] = GameObject.FindGameObjectWithTag(tag);
                m_enemiesArray[i - 1].SetActive(false);
            }

            // Get animation of enemies
            m_enemiesAnim = new List<Animation>();
            for (int i = 19; i >= 0; i--)
            {
                Animation anim = m_enemiesArray[i].GetComponent<Animation>();
                if (anim != null)
                {
                    m_enemiesAnim.Add(anim);
                }
            }
        }

        private void SetTimeRespawnLvl1()
        {
            m_timeRespawn[0] = 9.0f;
            m_timeRespawn[1] = 8.8f;
            m_timeRespawn[2] = 7.0f;
            m_timeRespawn[3] = 6.5f;
            m_timeRespawn[4] = 9.2f;
            m_timeRespawn[5] = 8.8f;
            m_timeRespawn[6] = 7.0f;
            m_timeRespawn[7] = 6.0f;
            m_timeRespawn[8] = 8.7f;
            m_timeRespawn[9] = 8.0f;
            m_timeRespawn[10] = 6.2f;

            SpeedRunDown = 2.0f;

            SetAnimationSpeed(0.5f);
        }

        private void SetTimeRespawnLvl2()
        {
            m_timeRespawn[0] = 7.0f;
            m_timeRespawn[1] = 6.8f;
            m_timeRespawn[2] = 5.0f;
            m_timeRespawn[3] = 4.5f;
            m_timeRespawn[4] = 7.2f;
            m_timeRespawn[5] = 6.8f;
            m_timeRespawn[6] = 5.0f;
            m_timeRespawn[7] = 4.0f;
            m_timeRespawn[8] = 6.7f;
            m_timeRespawn[9] = 6.0f;
            m_timeRespawn[10] = 4.2f;

            SpeedRunDown = 2.5f;

            SetAnimationSpeed(0.7f);
        }

        private void SetTimeRespawnLvl3()
        {
            m_timeRespawn[0] = 5.0f;
            m_timeRespawn[1] = 4.8f;
            m_timeRespawn[2] = 3.0f;
            m_timeRespawn[3] = 2.5f;
            m_timeRespawn[4] = 5.2f;
            m_timeRespawn[5] = 4.8f;
            m_timeRespawn[6] = 3.0f;
            m_timeRespawn[7] = 2.0f;
            m_timeRespawn[8] = 4.7f;
            m_timeRespawn[9] = 4.0f;
            m_timeRespawn[10] = 2.2f;

            SpeedRunDown = 3.0f;

            SetAnimationSpeed(0.9f);
        }

        private void SetTimeRespawnLvl4()
        {
            m_timeRespawn[0] = 4.5f;
            m_timeRespawn[1] = 4.3f;
            m_timeRespawn[2] = 2.5f;
            m_timeRespawn[3] = 2.0f;
            m_timeRespawn[4] = 4.7f;
            m_timeRespawn[5] = 4.3f;
            m_timeRespawn[6] = 2.5f;
            m_timeRespawn[7] = 1.5f;
            m_timeRespawn[8] = 4.2f;
            m_timeRespawn[9] = 3.5f;
            m_timeRespawn[10] = 1.7f;

            SpeedRunDown = 3.5f;

            SetAnimationSpeed(1.2f);
        }

        private void SetAnimationSpeed(float speed)
        {
            for (int i = m_enemiesAnim.Count - 1; i >= 0; i--)
            {
                foreach (AnimationState state in m_enemiesAnim[i])
                {
                    state.speed = speed;
                }
            }
        }

        public void CalculateScore()
        {
            m_movedDistance += Time.deltaTime * m_scaleDistance;

            float roundedNumber = Mathf.Round(m_movedDistance);
            UIManager.Instance.scoreInGameText.text = roundedNumber + "";

            // Show the finish line
            if (roundedNumber != 0.0f && roundedNumber % 100.0f == 0.0f)
            {
                effect.gameObject.SetActive(true);
                StartCoroutine(TurnOffEffect(2.5f));
            }

            if (roundedNumber == 250.0f)
            {
                SetTimeRespawnLvl2();
            }
            else if (roundedNumber == 750.0f)
            {
                SetTimeRespawnLvl3();
            }
            else if (roundedNumber == 1500.0f)
            {
                SetTimeRespawnLvl4();
            }
        }

        private IEnumerator TurnOffEffect(float time)
        {
            yield return new WaitForSeconds(time);
            effect.gameObject.SetActive(false);
        }

        public void SpawnEnemy()
        {
            // When game just started
            if (gameJustStarted)
            {
                gameJustStarted = false;

                m_enemiesArray[0].SetActive(true);

                m_enemiesArray[1].transform.position = new Vector3(
                    m_enemiesArray[1].transform.position.x, m_enemiesArray[0].transform.position.y + 10.0f, 0.0f);
                m_enemiesArray[1].SetActive(true);

                m_enemiesArray[2].transform.position = new Vector3(
                    m_enemiesArray[2].transform.position.x, m_enemiesArray[1].transform.position.y + 10.0f, 0.0f);
                m_enemiesArray[2].SetActive(true);

                m_lastBornEnemy = m_enemiesArray[2];

                m_firstRandomEnemy = true;
            }
            else
            {
                // Random Spawn enemies
                if (!m_wait)
                {
                    int indexRandom = 0;

                    while (true)
                    {
                        indexRandom = (int)Mathf.Round(Random.Range(0.0f, 19.0f));

                        if (!m_enemiesArray[indexRandom].activeInHierarchy)
                        {
                            break;
                        }
                    }
                    m_wait = true;

                    if (m_firstRandomEnemy)
                    {
                        m_firstRandomEnemy = false;
                        SetFirstTimeToSpawn(ref indexRandom);
                    }
                    else
                    {
                        SetTimeToSpawn(ref indexRandom);
                    }
                }
            }
        }

        private void SetTimeToSpawn(ref int index)
        {
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                case 10:
                case 11:
                case 12:
                    if (m_lastBornEnemy == m_enemiesArray[5] || m_lastBornEnemy == m_enemiesArray[15]
                        || m_lastBornEnemy == m_enemiesArray[8] || m_lastBornEnemy == m_enemiesArray[18])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[0]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[2] || m_lastBornEnemy == m_enemiesArray[12]
                        || m_lastBornEnemy == m_enemiesArray[7] || m_lastBornEnemy == m_enemiesArray[17]
                        || m_lastBornEnemy == m_enemiesArray[9] || m_lastBornEnemy == m_enemiesArray[19])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[1]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[3] || m_lastBornEnemy == m_enemiesArray[13]
                        || m_lastBornEnemy == m_enemiesArray[4] || m_lastBornEnemy == m_enemiesArray[14]
                        || m_lastBornEnemy == m_enemiesArray[6] || m_lastBornEnemy == m_enemiesArray[16])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[2]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[0] || m_lastBornEnemy == m_enemiesArray[10]
                        || m_lastBornEnemy == m_enemiesArray[1] || m_lastBornEnemy == m_enemiesArray[11])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[3]));
                    }
                    break;

                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                    if (m_lastBornEnemy == m_enemiesArray[5] || m_lastBornEnemy == m_enemiesArray[15]
                        || m_lastBornEnemy == m_enemiesArray[8] || m_lastBornEnemy == m_enemiesArray[18]
                        || m_lastBornEnemy == m_enemiesArray[9] || m_lastBornEnemy == m_enemiesArray[19])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[4]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[2] || m_lastBornEnemy == m_enemiesArray[12]
                        || m_lastBornEnemy == m_enemiesArray[7] || m_lastBornEnemy == m_enemiesArray[17])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[5]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[3] || m_lastBornEnemy == m_enemiesArray[13]
                        || m_lastBornEnemy == m_enemiesArray[4] || m_lastBornEnemy == m_enemiesArray[14]
                        || m_lastBornEnemy == m_enemiesArray[6] || m_lastBornEnemy == m_enemiesArray[16])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[6]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[0] || m_lastBornEnemy == m_enemiesArray[10]
                        || m_lastBornEnemy == m_enemiesArray[1] || m_lastBornEnemy == m_enemiesArray[11])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[7]));
                    }
                    break;

                case 8:
                case 9:
                case 18:
                case 19:
                    if (m_lastBornEnemy == m_enemiesArray[5] || m_lastBornEnemy == m_enemiesArray[15]
                        || m_lastBornEnemy == m_enemiesArray[8] || m_lastBornEnemy == m_enemiesArray[18]
                        || m_lastBornEnemy == m_enemiesArray[9] || m_lastBornEnemy == m_enemiesArray[19])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[8]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[2] || m_lastBornEnemy == m_enemiesArray[12]
                        || m_lastBornEnemy == m_enemiesArray[7] || m_lastBornEnemy == m_enemiesArray[17])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[9]));
                    }
                    else if (m_lastBornEnemy == m_enemiesArray[0] || m_lastBornEnemy == m_enemiesArray[10]
                        || m_lastBornEnemy == m_enemiesArray[1] || m_lastBornEnemy == m_enemiesArray[11]
                        || m_lastBornEnemy == m_enemiesArray[3] || m_lastBornEnemy == m_enemiesArray[13]
                        || m_lastBornEnemy == m_enemiesArray[4] || m_lastBornEnemy == m_enemiesArray[14]
                        || m_lastBornEnemy == m_enemiesArray[6] || m_lastBornEnemy == m_enemiesArray[16])
                    {
                        StartCoroutine(WaitBeforeSpawn(index, m_timeRespawn[10]));
                    }
                    break;
            }
        }

        private void SetFirstTimeToSpawn(ref int index)
        {
            if ((3 <= index && index <= 7) || (10 <= index && index <= 11) || (13 <= index && index <= 17))
            {
                StartCoroutine(WaitBeforeSpawn(index, 18.0f));
            }
            else if (index == 8 || index == 9 || index == 12 || index == 18 || index == 19)
            {
                StartCoroutine(WaitBeforeSpawn(index, 17.6f));
            }
        }

        private IEnumerator WaitBeforeSpawn(int index, float time)
        {
            yield return new WaitForSeconds(time);
            if (GameState == GAMESTATE.PLAY)
            {
                m_enemiesArray[index].SetActive(true);
                m_lastBornEnemy = m_enemiesArray[index];

                m_wait = false;
            }
        }
    }

    public enum GAMESTATE
    {
        START, PLAY, OVER
    }
}
