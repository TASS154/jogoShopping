using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    public enum MouseMode { Place, Drag, Delete }

    [Header("Configurações")]
    public float dragSpeed = 0.1f;

    [Header("Visual do Cursor no Grid")]
    public Transform gridCursor;
    public bool clampToGridBounds = true;
    public float gridCursorZ = 0f;
    public SpriteRenderer gridCursorRenderer;
    public Color freeCellColor = new Color(0f, 1f, 0f, 0.35f);
    public Color occupiedCellColor = new Color(1f, 0f, 0f, 0.35f);
    public bool matchCellSize = true;
    public string gridCursorSortingLayer = "Cursor";
    public int gridCursorSortingOrder = 30;

    private Vector2Int lastFreeCell;
    private bool hasLastFreeCell = false;
    private MouseMode currentMode = MouseMode.Place;
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // === MUDANÇA DE MODO ===
        if (Input.GetKeyDown(KeyCode.Space)) SetMode(MouseMode.Drag);
        if (Input.GetKeyDown(KeyCode.V)) SetMode(MouseMode.Place);
        if (Input.GetKeyDown(KeyCode.Delete)) SetMode(MouseMode.Delete);

        // === POSIÇÃO DO MOUSE NO GRID ===
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
        int x = gridPos.x;
        int y = gridPos.y;

        // === CURSOR VISUAL (destaca 1ª célula da loja) ===
        if (gridCursor != null && GridManager.Instance != null)
        {
            bool inside = x >= 0 && x < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight;
            int gx = Mathf.Clamp(x, 0, GridManager.Instance.gridWidth - 1);
            int gy = Mathf.Clamp(y, 0, GridManager.Instance.gridHeight - 1);

            bool occupied = inside && (GridManager.Instance.IsCellOccupied(gx, gy) ||
                                     (gx + 1 < GridManager.Instance.gridWidth && GridManager.Instance.IsCellOccupied(gx + 1, gy)));

            bool valid = inside && (currentMode == MouseMode.Delete ? occupied : !occupied);

            Vector2Int cell = valid ? new Vector2Int(gx, gy) : (hasLastFreeCell ? lastFreeCell : new Vector2Int(gx, gy));
            if (valid) { lastFreeCell = cell; hasLastFreeCell = true; }

            Vector3 pos = GridManager.Instance.GetWorldPosition(cell.x, cell.y);
            pos.z = gridCursorZ;
            gridCursor.position = pos;

            if (matchCellSize)
                gridCursor.localScale = Vector3.one * GridManager.Instance.cellSize;

            if (gridCursorRenderer != null)
            {
                gridCursorRenderer.color = valid ? freeCellColor : occupiedCellColor;
                gridCursorRenderer.sortingLayerName = gridCursorSortingLayer;
                gridCursorRenderer.sortingOrder = gridCursorSortingOrder;
            }

            gridCursor.gameObject.SetActive(inside);
        }

        // === DRAG DA CÂMERA ===
        if (currentMode == MouseMode.Drag)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CursorManager.Instance.SetOnMoveCursor();
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Camera.main.transform.position -= new Vector3(delta.x * dragSpeed, delta.y * dragSpeed, 0);
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                CursorManager.Instance.SetMoveCursor();
                isDragging = false;
            }
        }
        // === CLIQUE PARA COLOCAR/REMOVER ===
        else if (Input.GetMouseButtonDown(0))
        {
            if (x >= 0 && x + 1 < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight)
            {
                if (currentMode == MouseMode.Place)
                {
                    int sel = GameManager.Instance.selectedItemIndex;
                    if (sel >= 0)
                    {
                        if (GridManager.Instance.IsCellOccupied(x, y) || GridManager.Instance.IsCellOccupied(x + 1, y))
                            PreviewManager.Instance.ShowWarning("Célula ocupada!");
                        else
                            GameManager.Instance.TryPlaceItem(x, y, sel);
                    }
                }
                else if (currentMode == MouseMode.Delete && GridManager.Instance.IsCellOccupied(x, y))
                {
                    GridManager.Instance.RemoveItem(x, y);
                }
            }
        }
    }

    public void SetMode(MouseMode mode)
    {
        currentMode = mode;
        CursorManager.Instance.SetNormalCursor();
        if (mode == MouseMode.Drag) CursorManager.Instance.SetMoveCursor();
        else if (mode == MouseMode.Delete) CursorManager.Instance.SetTrashCursor();
        isDragging = false;
    }
}