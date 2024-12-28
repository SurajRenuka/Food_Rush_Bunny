using UnityEngine;
using System.Collections;

namespace Com.KhuongDuy.MMMFinger
{
    public class RunDown : MonoBehaviour
    {
        // Constructor
        private RunDown() { }

        // Behaviour messages
        void Update()
        {
            if (GameController.Instance.GameState == GAMESTATE.PLAY)
            {
                transform.Translate(0.0f, GameController.Instance.SpeedRunDown * -Time.deltaTime, 0.0f);

                ResetPosition();
            }
        }

        private void ResetPosition()
        {
            switch (tag)
            {
                case "Floor":
                    if (transform.position.y <= -17.0f)
                        transform.position = new Vector3(transform.position.x, transform.position.y + 33.5f, 0f);                    
                    break;

                case "FinishLine":
                    if (transform.position.y <= -7.0f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 7.0f, 0.0f);
                    }
                    break;

                case "Group1":
                case "Group2":
                    if (transform.position.y <= -8.5f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 8.5f, 0.0f);
                    }
                    break;

                case "Group3":
                    if (transform.position.y <= -13.5f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 8.5f, 0.0f);
                    }
                    break;

                case "Group4":
                case "Group5":
                case "Group7":
                    if (transform.position.y <= -9.5f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 9.0f, 0.0f);
                    }
                    break;

                case "Group6":
                case "Group9":
                case "Group10":
                    if (transform.position.y <= -13.5f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 13.0f, 0.0f);
                    }
                    break;

                case "Group8":
                    if (transform.position.y <= -11.5f)
                    {
                        gameObject.SetActive(false);
                        transform.position = new Vector3(0.0f, 11.5f, 0.0f);
                    }
                    break;
            }
        }
    }
}
