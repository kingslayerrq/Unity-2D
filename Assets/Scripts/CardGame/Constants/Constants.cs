using UnityEngine;

public class CONSTANTS
{
    public static int UNIT_TARGET_PRIORITY = 999;
    public static int PLAY_AREA_TARGET_PRIORITY = 1;

    // ----- Card sorting order layers -----
    // Each CardVisual prefab has its own Canvas with overrideSorting=true.
    // These values are written into that canvas's sortingOrder. Higher = front.
    //
    // Layout (front to back):
    //   ANIMATION_GHOST_START   (1500..) — fly-to-discard / fly-to-exhaust ghosts
    //   SELECTION_TRAY_CARD     (1100)   — cards picked into the selection tray
    //   HAND_DURING_SELECTION   (1050)   — every hand card while a selection prompt is open
    //   SELECTION_CANVAS        (1000)   — the selection canvas backdrop (set in scene)
    //   CARD_DRAG               (999)    — the card while the mouse is dragging it
    //   CARD_IN_FLIGHT          (800)    — played card after drop, before its animation captures it
    //   CARD_HOVER_BUMP         (+100 over base) — mouse-over lift
    //   CARD_BASE               (1)      — idle hand card
    public const int CARD_BASE_SORTING_ORDER = 1;
    public const int CARD_HOVER_BUMP = 100;
    public const int CARD_IN_FLIGHT_SORTING_ORDER = 800;
    public const int CARD_DRAG_SORTING_ORDER = 999;
    public const int CARD_SELECTION_CANVAS_SORTING_ORDER = 1000;
    public const int CARD_HAND_DURING_SELECTION = 1050;
    public const int CARD_SELECTION_TRAY_SORTING_ORDER = 1100;
    public const int CARD_ANIMATION_GHOST_START = 1500;
}
