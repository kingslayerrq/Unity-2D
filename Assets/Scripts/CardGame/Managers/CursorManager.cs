using UnityEngine;
using UnityEngine.InputSystem;

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
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Cursor.SetCursor(clickedCursor, hotSpot, cursorMode);
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
        }
    }
}
