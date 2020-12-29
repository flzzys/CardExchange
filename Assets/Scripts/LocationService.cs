using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocationService : Singleton<LocationService> {
    public static bool enableLocationService {
        get {
            return Input.location.isEnabledByUser;
        }
    }

    public bool running;

    public void StartService(Action<bool> onComplete = null) {
        StartCoroutine(StartServiceCor(onComplete));
    }

    IEnumerator StartServiceCor(Action<bool> onComplete = null) {
        Input.location.Start();

        int timeoutLimit = 20;

        while (Input.location.status == LocationServiceStatus.Initializing && timeoutLimit > 0) {
            yield return new WaitForSeconds(1);
            timeoutLimit--;
        }

        if(timeoutLimit < 1 || Input.location.status == LocationServiceStatus.Failed) {
            onComplete?.Invoke(false);
            
            yield break;
        }

        running = true;
        onComplete?.Invoke(true);
    }

    public void StopService() {
        Input.location.Stop();

        running = false;
    }

    public struct Location {
        //纬度
        public float latitude;
        //经度
        public float longitude;
        //高度
        public float altitude;
    }

    //获取当前位置
    public static void GetPos(Action<Location> onComplete = null) {
        instance.GetPosCor(onComplete);
    }
    public IEnumerator GetPosCor(Action<Location> onComplete = null) {
        yield return null;

        Location location = new Location();

        onComplete?.Invoke(location);
    }
}
