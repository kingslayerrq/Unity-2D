using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Draws a run's map on screen: where each node sits, the node views and the
/// lines between them. It also holds the map's size, which is authored here
/// in the scene.
///
/// The map itself (paths, node types, node contents) is built by
/// <see cref="MapBuilder"/>. This component only lays it out and shows it.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mapContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject linePrefab;

    [Header("Settings")]
    [SerializeField] private NodeConfigLibrary nodeConfigLibrary;
    [SerializeField] private int horizontalOffset;
    [SerializeField] private int verticalOffset;
    [SerializeField] private int nodesPerFloor;
    [SerializeField] private int floorPerAct;
    [SerializeField] private float minVerticalGap = 100f;

    [Header("Debug")]
    [SerializeField] private int runSeed;
    [SerializeField] private float mapContainerHeightMult;
    [SerializeField] private bool generateOnPlay = false;

    private RectTransform _mapContainerRect;
    private float _mapContainerWidth;
    private float _mapContainerHeight;
    private float _nodeIconWidth;
    private float _nodeIconHeight;

    // The map currently drawn.
    private RunMap map;
    private List<NodeView> _cacheNodeViews = new List<NodeView>();

    /// <summary>The size of map this scene is set up to show.</summary>
    public MapSettings Settings => new MapSettings(nodesPerFloor, floorPerAct);

    public NodeConfigLibrary NodeConfigLibrary => nodeConfigLibrary;

    private void Awake()
    {
        RunManager.Instance.RegisterMapGenerator(this);
        RunManager.Instance.OnRunStateChange += OnEnterMapState;
    }

    private void OnEnterMapState(RunState runState)
    {
        if (runState == RunState.MapState)
        {
            for (int i = 0; i < _cacheNodeViews.Count; i++)
            {
                var nv =  _cacheNodeViews[i];
                if (nv != null) nv.RefreshNodeState();
            }
        }
    }

    #region Standalone generation (editor buttons and test scenes)

    public bool GenerateNewMapWithRandomSeed()
    {
        runSeed = Guid.NewGuid().GetHashCode();
        return GenerateMap();
    }

    public bool GenerateNewMapWithSeed(int seed)
    {
        runSeed = seed;
        return GenerateMap();
    }

    /// <summary>
    /// Builds a map from this component's own seed and settings and draws it.
    /// During a run the map comes from the run instead; see <see cref="ShowMap"/>.
    /// </summary>
    public bool GenerateMap()
    {
        return ShowMap(MapBuilder.Build(runSeed, Settings, nodeConfigLibrary));
    }

    #endregion

    /// <summary>
    /// Draws a map, replacing whatever was drawn before.
    /// </summary>
    /// <returns>
    /// False when it could not be drawn, for example because the panel has
    /// not been laid out yet.
    /// </returns>
    public bool ShowMap(RunMap newMap)
    {
        // clear before drawing
        ClearGeneratedMap();

        if (newMap == null)
        {
            Debug.LogError("MapGenerator was given no map to draw.", this);
            return false;
        }

        if (!PrepareLayout())
            return false;

        map = newMap;
        runSeed = newMap.Seed;

        LayoutNodes();
        DrawVisualMap();
        return true;
    }

    private bool PrepareLayout()
    {
        if (mapContainer == null || nodePrefab == null || linePrefab == null)
        {
            Debug.LogError("MapGenerator is missing one or more references.");
            return false;
        }

        _mapContainerRect = mapContainer.GetComponent<RectTransform>();
        if (_mapContainerRect == null)
        {
            Debug.LogError("mapContainer does not have a RectTransform.");
            return false;
        }

        _mapContainerWidth = _mapContainerRect.rect.width;
        if (_mapContainerWidth <= 0f)
        {
            Debug.LogError(
                "Cannot draw the map before its UI layout has a " +
                "positive width.",
                this);
            return false;
        }

        _mapContainerHeight = Screen.height * mapContainerHeightMult;
        _mapContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mapContainerHeight);

        RectTransform nodeRect = nodePrefab.GetComponent<RectTransform>();
        if (nodeRect == null)
        {
            Debug.LogError("nodePrefab does not have a RectTransform.");
            return false;
        }

        _nodeIconWidth = nodeRect.rect.width;
        _nodeIconHeight = nodeRect.rect.height;
        return true;
    }

    public void ClearGeneratedMap()
    {
        _cacheNodeViews.Clear();
        map = null;

        if (mapContainer == null)
            return;

        List<GameObject> childrenToDelete = new List<GameObject>();

        foreach (Transform child in mapContainer.transform)
        {
            childrenToDelete.Add(child.gameObject);
        }

        for (int i = 0; i < childrenToDelete.Count; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(childrenToDelete[i]);
            }
            else
            {
                DestroyImmediate(childrenToDelete[i]);
            }
        }
    }

    /// <summary>
    /// Works out where each slot of the map sits inside the container and
    /// stores it on the node, where the node views read it.
    /// </summary>
    private void LayoutNodes()
    {
        int floorCount = map.FloorCount;
        int slotsPerFloor = map.NodesPerFloor;

        float horizontalGap = (_mapContainerWidth - (slotsPerFloor * _nodeIconWidth) - (2 * horizontalOffset)) / (slotsPerFloor + 1);

        float calcVerticalGap = (_mapContainerHeight - (floorCount * _nodeIconHeight) - (2 * verticalOffset)) / (floorCount + 1);
        float verticalGap = Mathf.Max(calcVerticalGap, minVerticalGap);

        float stepX = horizontalGap + _nodeIconWidth;
        float stepY = verticalGap + _nodeIconHeight;

        float requiredHeight = (floorCount * _nodeIconHeight) + (verticalGap * (floorCount + 1)) + (2 * verticalOffset);
        _mapContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);

        float startX = (-_mapContainerWidth / 2) + horizontalOffset + (_nodeIconWidth / 2) + horizontalGap;
        float startY = (-requiredHeight / 2) + verticalOffset + (_nodeIconHeight / 2) + verticalGap;

        float currentX = startX;
        float currentY = startY;

        for (int floor = 0; floor < floorCount; floor++)
        {
            for (int index = 0; index < slotsPerFloor; index++)
            {
                map.Grid[floor][index].Position = new Vector2(currentX, currentY);
                currentX += stepX;
            }
            currentX = startX;
            currentY += stepY;
        }
    }

    private void DrawVisualMap()
    {
        for (int floor = 0; floor < map.FloorCount; floor++)
        {
            for (int index = 0; index < map.NodesPerFloor; index++)
            {
                MapNodeData mapNode = map.Grid[floor][index];

                // Only instantiate views for nodes that are part of the path
                if (!mapNode.IsReachable)
                    continue;

                GameObject nodeObj = Instantiate(nodePrefab, mapContainer.transform);
                NodeView view = nodeObj.GetComponent<NodeView>();

                if (view != null)
                {
                    view.Initialize(mapNode);
                    // Add to cache
                    _cacheNodeViews.Add(view);
                }
            }
        }

        // Draw lines after nodes are created
        DrawVisualLines();
    }

    private void DrawVisualLines()
    {
        for (int floor = 0; floor < map.FloorCount - 1; floor++)
        {
            for (int index = 0; index < map.NodesPerFloor; index++)
            {
                MapNodeData node = map.Grid[floor][index];

                if (!node.IsReachable) continue;

                foreach (MapNodeData child in node.Children)
                {
                    CreateUINodeConnection(node.Position, child.Position);
                }
            }
        }
    }

    private void CreateUINodeConnection(Vector2 startPos, Vector2 endPos)
    {
        // 1. Instantiate the line prefab inside the map container
        GameObject lineObj = Instantiate(linePrefab, mapContainer.transform);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 2. Push lines behind the nodes (so the icons render on top of the lines)
        lineObj.transform.SetAsFirstSibling();

        // 3. Set the starting position
        lineRect.anchoredPosition = startPos;

        // 4. Calculate the distance and direction
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        // 5. Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 6. Apply the math
        // width = distance to stretch to the target, height = line thickness (e.g., 5 pixels)
        lineRect.sizeDelta = new Vector2(distance, 5f);

        // Rotate the RectTransform to point at the target
        lineRect.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void OnDestroy()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.OnRunStateChange -= OnEnterMapState;
        }
    }
}
