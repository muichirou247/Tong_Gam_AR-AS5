using UnityEngine;

public class BambooMapmaker : MonoBehaviour
{
    [Header("Mini Map")]
    public RectTransform mapParent;
    public GameObject redDotPrefab;
    private GameObject lastRedDot;

    public void PlaceRedDotOnMap(Vector3 worldPosition)
    {
        if (mapParent && redDotPrefab)
        {
            GameObject markerGO = Instantiate(redDotPrefab, mapParent);
            RectTransform markerRect = markerGO.GetComponent<RectTransform>();
            Vector2 mapPosition = WorldToMapPosition(worldPosition);
            markerRect.anchoredPosition = mapPosition;

            lastRedDot = markerGO;
        }
    }
    public GameObject GetLastRedDot()
    {
        return lastRedDot;
    }

    private Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        float mapWidth = mapParent.rect.width;
        float mapHeight = mapParent.rect.height;

        float mapX = Mathf.InverseLerp(-5f, 5f, worldPosition.x) * mapWidth;
        float mapY = Mathf.InverseLerp(-5f, 5f, worldPosition.z) * mapHeight;

        return new Vector2(mapX, mapY);
    }
}