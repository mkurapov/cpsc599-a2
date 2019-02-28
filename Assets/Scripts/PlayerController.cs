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
    public bool isMoving = true;
    private int speed = 1;

    private GameObject currentTarget;
    private int targetIndex = 0;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        plugin = new AndroidJavaClass("jp.kshoji.unity.sensor.UnitySensorPlugin").CallStatic<AndroidJavaObject>("getInstance");
    }

    void Update()
    {
        // this is dirty, but Easy AR needs it on each update:
        var target1 = GameObject.FindGameObjectWithTag("Target1");
        var target2 = GameObject.FindGameObjectWithTag("Target2");
        var target3 = GameObject.FindGameObjectWithTag("Target3");

        List<GameObject> targets = new List<GameObject> { target1, target2, target3 };
        var isSeeingAllTargets = targets.TrueForAll(t => t && t.activeSelf);

        if (isSeeingAllTargets && isMoving)
        {
            currentTarget = targets[targetIndex];
            transform.LookAt(currentTarget.transform);

            var hasArrived = Vector3.Distance(transform.position, currentTarget.transform.position) < 0.01f;
            if (hasArrived)
            {
                targetIndex++;
                if (targetIndex == targets.Count)
                {
                    isMoving = false;
                }
            }

            if (isMoving) { Move(); }
        }

        Debug.Log("Updating");
        if (plugin != null)
        {
            float[] sensorValue = plugin.Call<float[]>("getSensorValues", "accelerometer");

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