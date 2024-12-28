using UnityEngine;
using System.Collections;

namespace Com.KhuongDuy.MMMFinger
{
    /// <summary>
    /// Manage behaviour of the first type enemies
    /// </summary>
    public class Enemy1 : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Rotation speed around its own axis")]
        private float m_rotateSpeed = 1.2f;

        [SerializeField]
        private float m_direction = -1.0f;

        [HideInInspector]
        public Transform target;                    // this is used if 'rotatingAroundAnObject' = true
        [HideInInspector]
        public Transform entity;                    // this is used if 'isShadow' = true

        [HideInInspector]
        public float speedRotateAroundAnObject = 1.0f;       // this is used if 'rotatingAroundAnObject' = true

        [HideInInspector]
        public bool rotatingAroundAnObject;
        [HideInInspector]
        public bool isShadow;

        // Constructor
        private Enemy1() { }

        // Behaviour messages
        void Update()
        {
            transform.Rotate(Vector3.forward * m_direction * 180.0f * Time.deltaTime * m_rotateSpeed);

            if (rotatingAroundAnObject)
            {
                transform.RotateAround(target.position, transform.forward, Time.deltaTime * speedRotateAroundAnObject);
            }

            if (isShadow)
            {
                transform.position = entity.position + new Vector3(0.088f, -0.119f, 0.0f);
            }
        }
    }
}
