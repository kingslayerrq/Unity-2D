using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    [Header("Cursor Textures")]
    public Texture2D defaultCursor;
    public Texture2D clickedCursor;

    [Header("Cursor Settings")]
    public Vector2 hotSpot = Vector2.zero; 
    public CursorMode cursorMode = CursorMode.Auto;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(clickedCursor, hotSpot, cursorMode);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
        }
    }
}