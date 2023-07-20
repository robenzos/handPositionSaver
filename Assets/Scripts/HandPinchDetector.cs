using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPinchDetector : MonoBehaviour
{
    public float pinchThreshold = 0.02f;
    public GameObject xrHandLeft;
    public GameObject xrHandRight;
    public GameObject xrHandLeftIndexTip;
    public GameObject xrHandLeftThumbTip;
    public GameObject xrHandRightIndexTip;
    public GameObject xrHandRightThumbTip;
    private bool pinchDetected = false;
    public string savePath = "Assets/SavedHandPositions/";
    public string prefabName = "SavedHandPositionPrefab";
    public Image handSavedIndicator;
    private bool leftPinchDetected = false;
    private bool rightPinchDetected = false;
    private bool otherHandReadyToSave = false;
    private float timeBetweenPinches = 0.5f; // Adjust this time interval as needed
    private float lastPinchTime = 0f;

    void Start()
    {
        handSavedIndicator.CrossFadeAlpha(0f, 0f, false);
    }

    void Update()
    {
        if (xrHandLeft == null)
        {
            xrHandLeft = GameObject.Find("LeftHand(Clone)");
        }
        if (xrHandRight == null)
        {
            xrHandRight = GameObject.Find("RightHand(Clone)");
        }
        if (xrHandLeftIndexTip == null)
        {
            xrHandLeftIndexTip = GameObject.Find("L_IndexTip");
        }
        if (xrHandLeftThumbTip == null)
        {
            xrHandLeftThumbTip = GameObject.Find("L_ThumbTip");
        }
        if (xrHandRightIndexTip == null)
        {
            xrHandRightIndexTip = GameObject.Find("R_IndexTip");
        }
        if (xrHandRightThumbTip == null)
        {
            xrHandRightThumbTip = GameObject.Find("R_ThumbTip");
        }

        // Detect pinches on the left hand
        bool newLeftPinch = CheckPinch(xrHandLeftIndexTip.transform.position, xrHandLeftThumbTip.transform.position);
        if (newLeftPinch && !leftPinchDetected)
        {
            leftPinchDetected = true;
            lastPinchTime = Time.time;
            StartCoroutine(ResetPinchStateAfterDelay());
            Debug.Log("Pinch detected on the left hand!");
            if (otherHandReadyToSave)
            {
                SaveHandPrefab(xrHandRight.gameObject);
                otherHandReadyToSave = false;
            }
            else
            {
                otherHandReadyToSave = true;
            }
        }
        else if (!newLeftPinch && leftPinchDetected)
        {
            leftPinchDetected = false;
        }

        // Detect pinches on the right hand
        bool newRightPinch = CheckPinch(xrHandRightIndexTip.transform.position, xrHandRightThumbTip.transform.position);
        if (newRightPinch && !rightPinchDetected)
        {
            rightPinchDetected = true;
            lastPinchTime = Time.time;
            StartCoroutine(ResetPinchStateAfterDelay());
            Debug.Log("Pinch detected on the right hand!");
            if (otherHandReadyToSave)
            {
                SaveHandPrefab(xrHandLeft.gameObject);
                otherHandReadyToSave = false;
            }
            else
            {
                otherHandReadyToSave = true;
            }
        }
        else if (!newRightPinch && rightPinchDetected)
        {
            rightPinchDetected = false;
        }
    }

    private bool CheckPinch(Vector3 indexTipPosition, Vector3 thumbTipPosition)
    {
        float distance = Vector3.Distance(indexTipPosition, thumbTipPosition);
        return distance < pinchThreshold;
    }

    private IEnumerator ResetPinchStateAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenPinches);
        otherHandReadyToSave = false;
    }

    private void SaveHandPrefab(GameObject handObject)
    {    // Get the original hand's position and rotation
        Vector3 handPosition = handObject.transform.position;
        Quaternion handRotation = handObject.transform.rotation;

        // Create a new empty GameObject as a parent for the hand's hierarchy
        GameObject handParentObject = new GameObject("HandParent");
        handParentObject.transform.position = handPosition;
        handParentObject.transform.rotation = handRotation;

        // Set the handObject as a child of the new parent object
        handObject.transform.SetParent(handParentObject.transform);

        // Scale the new parent object to match the hand's scale
        handParentObject.transform.localScale = handObject.transform.lossyScale;

        // Save the hand's prefab
        string uniqueIdentifier = Guid.NewGuid().ToString();
        string prefabFileName = $"{prefabName}_{DateTime.Now:yyyyMMdd_HHmmss}_{uniqueIdentifier}.prefab";
        string prefabFilePath = $"{savePath}{prefabFileName}";

        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(handParentObject, prefabFilePath);
        Debug.Log($"Prefab saved at: {prefabFilePath}");
        #endif

        // Reset the parent object and its child (handObject) to their original hierarchy
        handObject.transform.SetParent(null);
        Destroy(handParentObject);
        DisplayImageSaveIndicator();
    }

    private void DisplayImageSaveIndicator()
    {
        StartCoroutine(DisplayImageForOneSecond());
    }

    private IEnumerator DisplayImageForOneSecond()
    {
        handSavedIndicator.CrossFadeAlpha(1f, 0.5f, false);
        yield return new WaitForSeconds(1f);
        handSavedIndicator.CrossFadeAlpha(0f, 0.5f, false);
    }
}
