using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static OVRSpatialAnchor;

public class AnchorManager : MonoBehaviour
{
    public static AnchorManager Instance = null;

    public GameObject anchorPrefabAgent;
    public GameObject anchorPrefabDeviceLight;
    public GameObject anchorPrefabDeviceSound;
    public GameObject anchorWallBlocker;

    public TextMeshProUGUI statusText;  // Text display for anchor status

    private List<GameObject> lstAnchorGOs = new List<GameObject>();
    private List<OVRSpatialAnchor> anchorInstances = new List<OVRSpatialAnchor>();
    private HashSet<Guid> anchorUuids = new HashSet<Guid>();
    private Dictionary<string, string> anchorTypes = new Dictionary<string, string>();

    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;
    public const string keyNumSavedAnchors = "numUuids";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _onLocalized = OnLocalized;
        }
        else
        {
            Destroy(this);
        }
    }

    // Create anchor by selecting the prefab based on type
    public void CreateSpatialAnchor(string strAnchorType, Vector3 position, Quaternion orientation)
    {
        if (strAnchorType == "AnchorAgent")
        {
            CreateSpatialAnchor(anchorPrefabAgent, position, orientation);
        }
        else if (strAnchorType == "AnchorDeviceSound")
        {
            CreateSpatialAnchor(anchorPrefabDeviceSound, position, orientation);
        }
        else if (strAnchorType == "AnchorDeviceLight")
        {
            CreateSpatialAnchor(anchorPrefabDeviceLight, position, orientation);
        }
        else if (strAnchorType == "AnchorWallBlocker")
        {
            CreateSpatialAnchor(anchorWallBlocker, position, orientation);
        }
    }

    // Method to get anchor positions for navigation
    public List<Vector3> GetAnchorPositions()
    {
        List<Vector3> anchorPositions = new List<Vector3>();
        foreach (var anchorGO in lstAnchorGOs)
        {
            anchorPositions.Add(anchorGO.transform.position);
        }
        return anchorPositions;
    }

    // Instantiate the prefab and position the anchor
    private void CreateSpatialAnchor(GameObject anchorPrefab, Vector3 position, Quaternion rotation)
    {
        Debug.Log("AnchorManager::CreateSpatialAnchor");
        GameObject go = Instantiate(anchorPrefab, position, rotation);
        lstAnchorGOs.Add(go);
        go.name = go.tag + " (not saved)";
        go.SetActive(true);
    }

    // Save created anchors
    public async void SaveAnchors()
    {
        Debug.Log("AnchorManager::SaveAnchors");

        foreach (GameObject go in lstAnchorGOs)
        {
            OVRSpatialAnchor anchor = go.AddComponent<OVRSpatialAnchor>();

            if (!await anchor.WhenLocalizedAsync())
            {
                Debug.LogError("Unable to create anchor.");
                Destroy(anchor.gameObject);
                return;
            }

            var saveResult = await anchor.SaveAnchorAsync();
            if (saveResult.Success)
            {
                anchorUuids.Add(anchor.Uuid);

                if (!PlayerPrefs.HasKey(keyNumSavedAnchors))
                {
                    PlayerPrefs.SetInt(keyNumSavedAnchors, 0);
                }

                int playerNumUuids = PlayerPrefs.GetInt(keyNumSavedAnchors);
                PlayerPrefs.SetString("AnchorUuid" + playerNumUuids, anchor.Uuid.ToString());
                PlayerPrefs.SetString("AnchorUuid" + playerNumUuids + "Type", anchor.gameObject.tag);
                PlayerPrefs.SetInt(keyNumSavedAnchors, ++playerNumUuids);
                Debug.Log("Anchor saved");

                string anchorUuid = anchor.Uuid.ToString();
                go.name = go.tag + "_" + anchorUuid.Substring(0, 4);

                statusText.text = "Anchor saved: " + go.name;
            }
            else
            {
                Debug.LogError("Failed to save anchor.");
            }
        }
    }

    // Load all saved anchors and instantiate them as GameObjects in the scene
    public async void LoadAllAnchors()
    {
        Debug.Log("AnchorManager::LoadAllAnchors");

        int numAnchorUuid = PlayerPrefs.GetInt(keyNumSavedAnchors);
        if (numAnchorUuid > 0)
        {
            for (int ii = 0; ii < numAnchorUuid; ++ii)
            {
                string uuidKey = "AnchorUuid" + ii;
                string currentUuid = PlayerPrefs.GetString(uuidKey);
                anchorUuids.Add(new Guid(currentUuid));

                string strAnchorType = PlayerPrefs.GetString(uuidKey + "Type");
                anchorTypes.Add(currentUuid, strAnchorType);
            }

            var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(anchorUuids, unboundAnchors);

            if (result.Success)
            {
                foreach (var anchor in unboundAnchors)
                {
                    anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
                }
            }
            else
            {
                Debug.LogError($"Load anchors failed with {result.Status}");
            }
        }

        // Create GameObjects for each anchor after loading them
        foreach (var anchorId in anchorUuids)
        {
            string uuidString = anchorId.ToString();
            if (anchorTypes.ContainsKey(uuidString))
            {
                string anchorType = anchorTypes[uuidString];
                CreateAnchorGameObject(anchorType, uuidString);
            }
        }
    }

    // Helper function to instantiate an anchor in the scene
    private void CreateAnchorGameObject(string anchorType, string uuidString)
    {
        GameObject anchorPrefab = GetPrefabFromType(anchorType);

        if (anchorPrefab != null)
        {
            // Instantiate the prefab at origin
            GameObject go = Instantiate(anchorPrefab, Vector3.zero, Quaternion.identity);
            go.name = anchorType + "_" + uuidString.Substring(0, 4);

            // Look for the matching anchor instance to set position and rotation
            foreach (var anchor in anchorInstances)
            {
                if (anchor.Uuid.ToString() == uuidString)
                {
                    // Directly use the transform properties of the anchor
                    go.transform.position = anchor.transform.position;
                    go.transform.rotation = anchor.transform.rotation;
                    break;
                }
            }

            // Set active so that it's visible in the scene
            go.SetActive(true);
            lstAnchorGOs.Add(go);
        }
        else
        {
            Debug.LogError("Unknown anchor type: " + anchorType);
        }
    }

    // Erase all anchors
    public async void EraseAllAnchors()
    {
        Debug.Log("AnchorManager::EraseAllAnchors");

        foreach (OVRSpatialAnchor anchor in anchorInstances)
        {
            Destroy(anchor.gameObject);
        }

        anchorInstances.Clear();

        var result = await OVRSpatialAnchor.EraseAnchorsAsync(null, anchorUuids);
        if (result.Success)
        {
            anchorUuids.Clear();
            Debug.Log("Anchors erased.");
        }
        else
        {
            Debug.LogError($"Anchors NOT erased: {result.Status}");
        }

        foreach (GameObject go in lstAnchorGOs)
        {
            Destroy(go);
        }

        int numAnchorUuid = PlayerPrefs.GetInt(keyNumSavedAnchors);
        for (int ii = 0; ii < numAnchorUuid; ++ii)
        {
            string uuidKey = "AnchorUuid" + ii;
            PlayerPrefs.DeleteKey(uuidKey);
            PlayerPrefs.DeleteKey(uuidKey + "Type");
        }

        PlayerPrefs.DeleteKey(keyNumSavedAnchors);
        anchorTypes.Clear();

        statusText.text = "All anchors erased.";
    }

    // Callback when an anchor is localized
    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        if (success)
        {
            // Determine the anchor type based on its UUID
            string anchorUuid = unboundAnchor.Uuid.ToString();
            string strAnchorType = anchorTypes[anchorUuid];
            GameObject anchorPrefab = GetPrefabFromType(strAnchorType);

            if (anchorPrefab != null)
            {
                // Instantiate a new GameObject at the origin
                GameObject go = Instantiate(anchorPrefab, Vector3.zero, Quaternion.identity);
                go.name = strAnchorType + "_" + anchorUuid.Substring(0, 4);

                // Add an OVRSpatialAnchor to this GameObject and bind the unboundAnchor to it
                OVRSpatialAnchor anchorComponent = go.AddComponent<OVRSpatialAnchor>();
                unboundAnchor.BindTo(anchorComponent);

                // After binding, use the transform of the newly bound anchor component
                go.transform.position = anchorComponent.transform.position;
                go.transform.rotation = anchorComponent.transform.rotation;
                go.SetActive(true);

                // Add to the list of anchor instances
                anchorInstances.Add(anchorComponent);

                // Update the status text
                statusText.text = "Anchor localized: " + go.name;
            }
            else
            {
                Debug.LogError("Unknown anchor type: " + strAnchorType);
            }
        }
        else
        {
            Debug.LogError("Cannot localize anchor.");
        }
    }

    // Get prefab based on anchor type
    private GameObject GetPrefabFromType(string anchorType)
    {
        switch (anchorType)
        {
            case "AnchorAgent":
                return anchorPrefabAgent;
            case "AnchorDeviceSound":
                return anchorPrefabDeviceSound;
            case "AnchorDeviceLight":
                return anchorPrefabDeviceLight;
            case "AnchorWallBlocker":
                return anchorWallBlocker;
            default:
                return null;
        }
    }
}
