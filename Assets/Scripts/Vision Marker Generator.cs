using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionMarkerGenerator : MonoBehaviour
{
    public List<GameObject> markerPrefabs;
    public List<Transform> markerPositions;
    private List<GameObject> currentMarkerInstances = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        InitializeMarkers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void InitializeMarkers()
    {
        ClearAllMarkers();
        
        foreach (Transform position in markerPositions)
        {
            if(markerPrefabs.Count>0)
            {
                float randomYAngle = Random.Range(-110f, 45f);
                Quaternion randomWorldRotation = Quaternion.Euler(0f, randomYAngle, 0f);
                int randomIndex = Random.Range(0, markerPrefabs.Count);
                GameObject newMarker = Instantiate(markerPrefabs[randomIndex], position.position, randomWorldRotation);
                currentMarkerInstances.Add(newMarker);
            }
            else
            {
                Debug.Log("마커프리팹이 할당되지 않음!");
                return;
            }
        }
    }

    public void ChangeAllMarkers()
    {
        ClearAllMarkers();
        
        foreach (Transform position in markerPositions)
        {
            if (markerPrefabs.Count > 0)
            {
                float randomYAngle = Random.Range(-110f, 45f);
                Quaternion randomWorldRotation = Quaternion.Euler(0f, randomYAngle, 0f);
                int randomIndex = Random.Range(0, markerPrefabs.Count);
                GameObject newMarker = Instantiate(markerPrefabs[randomIndex], position.position, randomWorldRotation);
                currentMarkerInstances.Add(newMarker);
            }
            else
            {
                Debug.Log("마커프리팹이 할당되지 않음!");
                return;
            }
        }
    }

    void ClearAllMarkers()
    {
        foreach (GameObject instance in currentMarkerInstances)
        {
            if(instance !=null)
            {
                Destroy(instance);
            }
        }
        currentMarkerInstances.Clear();
    }
}
