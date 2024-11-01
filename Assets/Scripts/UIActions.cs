using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActions : MonoBehaviour
{
    //rivate AnchorManager anchorManager;

    private void Start()
    {
        //anchorManager = AnchorManager.Instance;
        //if (anchorManager == null)
        //{
        //    Debug.LogError("AnchorManager instance not found in the scene.");
        //}
    }

    // Button 1: Save Anchors
    public void Btn01()
    {
        Debug.Log("UIActions::Btn01 - Save Anchors");

        AnchorManager.Instance.SaveAnchors();

    }

    // Button 2: Load Anchors
    public void Btn02()
    {
        Debug.LogWarning("UIActions::Btn02 - Load Anchors");

        AnchorManager.Instance.LoadAllAnchors();

    }

    // Button 3: Erase Anchors
    public void Btn03()
    {
        Debug.Log("UIActions::Btn03 - Erase Anchors");

        AnchorManager.Instance.EraseAllAnchors();

    }

    // Button 4: Placeholder or any additional functionality
    public void Btn04()
    {
        Debug.Log("UIActions::Btn04");
    }
}
