using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using EasyAR;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private AndroidJavaObject plugin;
    private Rigidbody rb;
    private bool isAbleToMove = false;
    private bool isFinished = false;
    private float speed = 0.4f;
    private Light gameLight;

    private GameObject currentTarget;
    private int targetIndex = 0;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        //gameLight = GetComponent<Light>();

#if UNITY_ANDROID
        plugin = new AndroidJavaClass("jp.kshoji.unity.sensor.UnitySensorPlugin").CallStatic<AndroidJavaObject>("getInstance");
        plugin.Call("startSensorListening", "accelerometer");
        plugin.Call("startSensorListening", "light");
        plugin.Call("startSensorListening", "gyroscope");
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

#if UNITY_ANDROID
        if (plugin != null)
        {
            float[] lightValues = plugin.Call<float[]>("getSensorValues", "light");
            float[] accValues = plugin.Call<float[]>("getSensorValues", "accelerometer");

            if (lightValues != null)
            {
                //gameLight.intensity = 0.1f * lightValues[0];
                Debug.Log(lightValues[0]);
                //isAbleToMove = lightValues[0] < 5.0f; //only one value is provided
            }

            if (accValues != null)
            {
                Debug.Log(accValues[1]);
                isAbleToMove = accValues[1] < 8.8f; //use y value to act as gas pedal

                //Debug.Log("acc:" + string.Join(",", new List<float>(gyroValues).ConvertAll(i => i.ToString()).ToArray()[1]) );
            }

            //if (sensorValues != null)
            //    {

            //        Debug.Log("sensorValues:" + string.Join(",", new List<float>(sensorValues).ConvertAll(i => i.ToString()).ToArray()));
            //    }
            //}
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
                    isFinished = false;
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