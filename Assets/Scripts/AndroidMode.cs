using UnityEngine;

public class AndroidMode : MonoBehaviour
{
    void Awake()
    {
        if (!Application.isMobilePlatform && !Application.isEditor)
        {
            gameObject.SetActive(false);
        }
    }
}