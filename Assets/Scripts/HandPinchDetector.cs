using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class HandPinchDetector : MonoBehaviour
{
    public float pinchThreshold = 0.7f;
    public OVRHand ovrHand;
    public GameObject ovrHandSaveable;
    private bool pinchDetected = false;
    public string savePath = "Assets/SavedHandPositions/";
    public string prefabName = "SavedHandPositionPrefab";

    float pinchInterval = 0.5f;
    int pinchCount = 0;
    float lastPinchTime = 0f;

    void Update()
    {
        float currentPinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        if (currentPinchStrength >= pinchThreshold)
        {
            if (!pinchDetected)
            {
                pinchDetected = true;
                if (Time.time - lastPinchTime <= pinchInterval)
                {
                    pinchCount++;
                    if (pinchCount == 2)
                    {
                        Debug.Log($"Double pinch detected on {ovrHand.gameObject.name}!");
                        SaveHandPrefab();
                        pinchCount = 0; // Reset pinch count after saving
                    }
                }
                else
                {
                    pinchCount = 1;
                }
            }
            lastPinchTime = Time.time;
        }
        else
        {
            pinchDetected = false;
        }
    }


    private void SaveHandPrefab()
    {
        //wenn wir absolute position wollen
        //Vector3 handPosition = ovrHandSaveable.transform.position;
        Quaternion handRotation = ovrHandSaveable.transform.rotation;

        GameObject newHandPrefab = Instantiate(ovrHandSaveable.gameObject, Vector3.zero, handRotation);

        string uniqueIdentifier = Guid.NewGuid().ToString();
        string prefabFileName = $"{prefabName}_{DateTime.Now:yyyyMMdd_HHmmss}_{uniqueIdentifier}.prefab";
        string prefabFilePath = $"{savePath}{prefabFileName}";

        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(newHandPrefab, prefabFilePath);
        Debug.Log($"Prefab saved at: {prefabFilePath}");
        #endif

        Destroy(newHandPrefab);
    }
}
