using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRAnchorController : MonoBehaviour
{
    public Transform rightHand;
    public Transform leftHand;

    private AnchorManager anchorManager;

    private void Start()
    {
        anchorManager = AnchorManager.Instance;
        if (anchorManager == null)
        {
            Debug.LogError("AnchorManager instance not found in the scene.");
        }
    }

    private void Update()
    {
        if (anchorManager == null) return;

        // Create anchor at right hand position on right trigger press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            anchorManager.CreateSpatialAnchor("AnchorAgent", rightHand.position, rightHand.rotation);
        }

        // Save all anchors on left trigger press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            anchorManager.SaveAnchors();
        }

        // Load all anchors on right grip press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            anchorManager.LoadAllAnchors();
        }

        // Erase all anchors on left grip press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            anchorManager.EraseAllAnchors();
        }
    }
}

