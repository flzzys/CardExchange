using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    public static T instance {
        get {
            T[] temps = FindObjectsOfType<T>();

            if(temps.Length == 0) {
                Debug.LogError("未能找到实例: " + typeof(T).ToString());
            }

            return temps[temps.Length - 1];
        }
    }
}
