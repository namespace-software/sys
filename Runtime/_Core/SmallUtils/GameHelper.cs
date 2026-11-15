using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHelper : MonoBehaviour
{
    public static GameHelper Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this) Destroy(this);
        }
    }

    public static T FindFirstObjectOfType<T>() where T : Object
    {
        return FindFirstObjectByType<T>();
    }
}