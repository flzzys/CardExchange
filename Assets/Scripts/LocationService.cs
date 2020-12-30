using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//位置，经纬度高度
public class Location {
    //纬度
    public float latitude;
    //经度
    public float longitude;
    //高度
    public float altitude;

    public override string ToString() {
        return string.Format("({0}, {1}, {2})", latitude, longitude, altitude);
    }
}

public class LocationService : Singleton<LocationService> {
    //机体是否支持位置服务，不过手机询问权限前也会当成不支持
    public static bool enableLocationService {
        get {
            return Input.location.isEnabledByUser;
        }
    }

    //位置服务启动了
    public bool running;

    //启动位置服务
    public void StartService(Action<bool> onComplete = null) {
        StartCoroutine(StartServiceCor(onComplete));
    }
    IEnumerator StartServiceCor(Action<bool> onComplete = null) {
        Input.location.Start();

        //不支持位置服务
        if (!enableLocationService) {
            onComplete?.Invoke(false);

            yield break;
        }

        int timeoutLimit = 20;

        while (Input.location.status == LocationServiceStatus.Initializing && timeoutLimit > 0) {
            yield return new WaitForSeconds(1);
            timeoutLimit--;
        }

        //超时或者失败，返回失败信息
        if (timeoutLimit < 1 || Input.location.status == LocationServiceStatus.Failed) {
            onComplete?.Invoke(false);

            yield break;
        }
        //成功启动
        running = true;
        onComplete?.Invoke(true);
    }

    //停止位置服务
    public void StopService() {
        Input.location.Stop();

        running = false;
    }

    //获取当前位置，返回是否成功和位置。如果服务未启动就启动
    public static void GetPos(Action<bool, Location> onComplete = null) {
        Location location;

        //位置服务已启动
        if (instance.running) {
            location = new Location {
                altitude = Input.location.lastData.altitude,
                latitude = Input.location.lastData.latitude,
                longitude = Input.location.lastData.longitude
            };

            onComplete?.Invoke(true, location);
        } else {
            //位置服务未启动，启动
            instance.StartService(success => {
                if (success) {
                    location = new Location {
                        altitude = Input.location.lastData.altitude,
                        latitude = Input.location.lastData.latitude,
                        longitude = Input.location.lastData.longitude
                    };

                    onComplete?.Invoke(true, location);

                    //获取位置后关闭位置服务
                    instance.StopService();
                } else {
                    onComplete?.Invoke(false, default);
                }
            });
        }

    }

    //根据两地经纬度计算距离，单位为米
    public static float GetDistance(Location loc1, Location loc2) {
        float lat1, lon1, lat2, lon2;
        lat1 = loc1.latitude;
        lon1 = loc1.longitude;
        lat2 = loc2.latitude;
        lon2 = loc2.longitude;

        var R = 6378.137; // Radius of earth in KM
        var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
        var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
        var a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
        Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
        Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var d = R * c;
        return (float)d * 1000;
    }
}
