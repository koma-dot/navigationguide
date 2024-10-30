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

    // Load all saved anchors
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
        if (unboundAnchor.TryGetPose(out Pose pose))
        {
            string strAnchorType = anchorTypes[unboundAnchor.Uuid.ToString()];
            GameObject go = null;

            if (strAnchorType == "AnchorAgent")
            {
                go = Instantiate(anchorPrefabAgent, pose.position, pose.rotation);
            }
            else if (strAnchorType == "AnchorDeviceSound")
            {
                go = Instantiate(anchorPrefabDeviceSound, pose.position, pose.rotation);
            }
            else if (strAnchorType == "AnchorDeviceLight")
            {
                go = Instantiate(anchorPrefabDeviceLight, pose.position, pose.rotation);
            }
            else if (strAnchorType == "AnchorWallBlocker")
            {
                go = Instantiate(anchorWallBlocker, pose.position, pose.rotation);
            }

            if (go != null)
            {
                OVRSpatialAnchor anchor = go.AddComponent<OVRSpatialAnchor>();
                unboundAnchor.BindTo(anchor);
                anchorInstances.Add(anchor);

                string anchorUuid = anchor.Uuid.ToString();
                go.name = go.tag + "_" + anchorUuid.Substring(0, 4);
                go.SetActive(true);

                statusText.text = "Anchor localized: " + go.name;
            }
        }
        else
        {
            Debug.LogError("Cannot retrieve pose for the localized anchor.");
        }
    }
}

