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

        // Create anchor at right hand position on A button press (Right Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.LogWarning("AnchorUIActions: Button A Pressed - Creating Anchor");
            anchorManager.CreateSpatialAnchor("AnchorAgent", OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        }

        // Save all anchors on B button press (Right Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.LogWarning("AnchorUIActions: Button B Pressed - Saving Anchors");
            anchorManager.SaveAnchors();
        }

        // Load all anchors on X button press (Left Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            Debug.LogWarning("AnchorUIActions: Button X Pressed - Loading Anchors");
            anchorManager.LoadAllAnchors();
        }

        // Erase all anchors on Y button press (Left Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            Debug.LogWarning("AnchorUIActions: Button Y Pressed - Erasing Anchors");
            anchorManager.EraseAllAnchors();
        }
    }
}
