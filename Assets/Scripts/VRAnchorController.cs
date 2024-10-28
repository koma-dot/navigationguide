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

        // Create anchor at right hand position on A button press
        if (OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.RTouch))
        {
            Debug.Log("AnchorUIActions: A button pressed");
            anchorManager.CreateSpatialAnchor("AnchorAgent",
                OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch),
                OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        }

        // Save all anchors on B button press on right controller
        if (OVRInput.GetDown(OVRInput.RawButton.B, OVRInput.Controller.RTouch))
        {
            Debug.Log("AnchorUIActions: B button pressed");
            anchorManager.SaveAnchors();
        }

        // Load all anchors on X button press on left controller
        if (OVRInput.GetDown(OVRInput.RawButton.X, OVRInput.Controller.LTouch))
        {
            Debug.Log("AnchorUIActions: X button pressed");
            anchorManager.LoadAllAnchors();
        }

        // Erase all anchors on Y button press on left controller
        if (OVRInput.GetDown(OVRInput.RawButton.Y, OVRInput.Controller.LTouch))
        {
            Debug.Log("AnchorUIActions: Y button pressed");
            anchorManager.EraseAllAnchors();
        }
    }
}
