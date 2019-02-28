using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using EasyAR;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private AndroidJavaObject plugin;
    private bool isAbleToMove = false;
    private bool isFinished = false;
    private float speed = 0.4f;

    private GameObject currentTarget;
    private int targetIndex = 0;

    void Start()
    {
#if UNITY_ANDROID
        plugin = new AndroidJavaClass("jp.kshoji.unity.sensor.UnitySensorPlugin").CallStatic<AndroidJavaObject>("getInstance");
        plugin.Call("startSensorListening", "accelerometer");
        plugin.Call("startSensorListening", "light");
#endif
    }

    void Update()
    {
        // this is dirty, but Easy AR needs it on each update:

        var target1 = GameObject.FindGameObjectWithTag("Target1");
        var target2 = GameObject.FindGameObjectWithTag("Target2");
        var target3 = GameObject.FindGameObjectWithTag("Target3");

        List<GameObject> targets = new List<GameObject> { target1, target2, target3 };
        var isSeeingAllTargets = targets.TrueForAll(t => t && t.activeSelf);

        var headlights = GameObject.Find("Headlights").GetComponent<Light>();

        #if UNITY_ANDROID
        if (plugin != null)
        {
            float[] lightValues = plugin.Call<float[]>("getSensorValues", "light");
            float[] accValues = plugin.Call<float[]>("getSensorValues", "accelerometer");

            if (lightValues != null)
            {
                headlights.intensity = lightValues[0] < 5.0f ? 2 : 0;
            }

            if (accValues != null)
            {
                isAbleToMove = accValues[1] < 9.3f; //use y value to act as gas pedal

            }
        }
        #endif

        if (isSeeingAllTargets && isAbleToMove && !isFinished)
        {
            currentTarget = targets[targetIndex];
            transform.LookAt(currentTarget.transform);

            var hasArrived = Vector3.Distance(transform.position, currentTarget.transform.position) < 0.01f;
            if (hasArrived)
            {
                targetIndex++;
                if (targetIndex == targets.Count)
                {
                    isFinished = true;
                }
            }

            if (isAbleToMove) { Move(); }
        }


    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collider)
    {
        // can do anim. here, but this was more fun:
        gameObject.GetComponent<AudioSource>().Play();
    }
}