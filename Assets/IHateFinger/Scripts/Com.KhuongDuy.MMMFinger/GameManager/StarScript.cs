using UnityEngine;
using System.Collections;

namespace Com.KhuongDuy.MMMFinger
{
    public class StarScript : MonoBehaviour
    {
        private Rigidbody2D rg2D;

        private Vector3 m_originalPos;

        // Constructor
        private StarScript() { }

        // Behaviour messages
        void Awake()
        {
            rg2D = GetComponent<Rigidbody2D>();
        }

        // Behaviour messages
        void Start()
        {
            m_originalPos = transform.localPosition;
        }

        // Behaviour messages
        void Update()
        {
            transform.Rotate(Vector3.forward * 180.0f * Time.deltaTime);
        }

        // Behaviour messages
        void OnEnable()
        {
            float velocityUp = Random.Range(3.0f, 5.0f);
            float velocityHorizontal = Random.Range(0.3f, 0.65f);

            int dir = Random.value <= 0.5f ? -1 : 1;

            rg2D.linearVelocity = new Vector2(velocityHorizontal * dir, velocityUp);

            float angle = Random.Range(0.0f, 360.0f);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Behaviour messages
        void OnDisable()
        {
            transform.localPosition = m_originalPos;
        }
    }
}
