using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{

    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    GameObject gameObj = new GameObject(typeof(T).ToString());
                    instance = gameObj.AddComponent<T>();
                }
            }
            return instance;
        }
    }


    public virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

}// CLASS
