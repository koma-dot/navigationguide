using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAnchorController : MonoBehaviour
{
    //--public Transform rightHand;
    //--public Transform leftHand;

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
            Debug.LogWarning("AnchorUIActions: Button A Pressed - Waypoint");
            anchorManager.CreateSpatialAnchor("Waypoint", OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), /*OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*/ new Quaternion());
        }

        // Create AnchorDeviceLight at right hand position on B button press (Right Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.LogWarning("AnchorUIActions: Button B Pressed - Creating TargetA");
            anchorManager.CreateSpatialAnchor("TargetA", OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), /*OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch)*/ new Quaternion());
        }

        // Create AnchorDeviceSound at left hand position on X button press (Left Controller)
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            Debug.LogWarning("AnchorUIActions: Button X Pressed - Creating TargetB");
            anchorManager.CreateSpatialAnchor("TargetB", OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch), /*OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch)*/ new Quaternion());
        }
    }
}
