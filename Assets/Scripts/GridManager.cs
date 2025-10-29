using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configurações da Grade")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 4f;

    [Header("Posição da Grid no Mundo")]
    public Vector2 gridOrigin = new Vector2(-40f, -8.5f); // Canto inferior esquerdo

    [Header("Prefabs dos Itens")]
    public GameObject[] itemPrefabs;
    public GameObject cursorPrefab;
    public GameObject cellOutlinePrefab;

    private bool[,] occupiedCells;
    private GameObject[,] grid;
    private int cursorX = 0, cursorY = 0;
    private GameObject cursorInstance;

    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    void Start()
    {
        occupiedCells = new bool[gridWidth, gridHeight];
        grid = new GameObject[gridWidth, gridHeight];

        // === CURSOR ===
        if (cursorPrefab != null)
        {
            cursorInstance = Instantiate(cursorPrefab, GetWorldPosition(0, 0), Quaternion.identity);
            cursorInstance.transform.localScale = Vector3.one * cellSize;
        }

        // === CRIA OUTLINES ===
        CreateOutlines();

        Debug.Log($"[GMGR] Grid iniciada: {gridWidth}x{gridHeight}, cellSize={cellSize}, origem={gridOrigin}");
    }

    void Update()
    {
        // === MOVIMENTO DO CURSOR ===
        if (Input.GetKeyDown(KeyCode.UpArrow)) cursorY = Mathf.Clamp(cursorY + 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) cursorY = Mathf.Clamp(cursorY - 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) cursorX = Mathf.Clamp(cursorX - 1, 0, gridWidth - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) cursorX = Mathf.Clamp(cursorX + 1, 0, gridWidth - 1);

        if (cursorInstance != null)
            cursorInstance.transform.position = GetWorldPosition(cursorX, cursorY);

        // === COLOCAÇÃO ===
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int sel = GameManager.Instance?.selectedItemIndex ?? -1;
            if (sel >= 0) GameManager.Instance.TryPlaceItem(cursorX, cursorY, sel);
        }

        // === REMOÇÃO ===
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            RemoveItem(cursorX, cursorY);

        // === MOVER GRID INTEIRA (Shift + WASD) ===
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float moveAmount = cellSize;
            bool moved = false;

            if (Input.GetKeyDown(KeyCode.W)) { gridOrigin.y += moveAmount; moved = true; }
            if (Input.GetKeyDown(KeyCode.S)) { gridOrigin.y -= moveAmount; moved = true; }
            if (Input.GetKeyDown(KeyCode.A)) { gridOrigin.x -= moveAmount; moved = true; }
            if (Input.GetKeyDown(KeyCode.D)) { gridOrigin.x += moveAmount; moved = true; }

            if (moved)
            {
                RefreshOutlines();
                UpdateCursorPosition();
                Debug.Log($"[GMGR] Grid movida para origem: {gridOrigin}");
            }
        }
    }

    // ================================================================
    // === CONVERSÃO COM ORIGEM ===
    // ================================================================

    public Vector3 GetWorldPosition(int gridX, int gridY)
    {
        float worldX = gridOrigin.x + gridX * cellSize;
        float worldY = gridOrigin.y + gridY * cellSize;
        return new Vector3(worldX, worldY, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y, bool centerX = false)
    {
        Vector3 pos = GetWorldPosition(x, y);
        if (centerX) pos.x += cellSize * 0.5f;
        return pos;
    }

    // ================================================================
    // === OUTLINES ===
    // ================================================================

    private void CreateOutlines()
    {
        // Remove antigos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Outline") || child == cursorInstance?.transform)
                continue;
            Destroy(child.gameObject);
        }

        if (cellOutlinePrefab == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 centerPos = new Vector3(
                    gridOrigin.x + x * cellSize + cellSize * 0.5f,
                    gridOrigin.y + y * cellSize + cellSize * 0.5f,
                    0
                );

                var outline = Instantiate(cellOutlinePrefab, centerPos, Quaternion.identity, transform);
                outline.transform.localScale = Vector3.one * cellSize;
                outline.name = $"Outline_{x}_{y}";

                var rend = outline.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.sortingLayerName = "Grid";
                    rend.sortingOrder = 0;
                }
            }
        }
    }

    private void RefreshOutlines()
    {
        CreateOutlines(); // Recria com nova origem
    }

    private void UpdateCursorPosition()
    {
        if (cursorInstance != null)
            cursorInstance.transform.position = GetWorldPosition(cursorX, cursorY);
    }

    // ================================================================
    // === COLOCAÇÃO E REMOÇÃO (2 CÉLULAS) ===
    // ================================================================

    public bool IsCellOccupied(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && occupiedCells[x, y];
    }

    public void PlaceItem(int x, int y, int itemIndex)
    {
        if (x < 0 || x + 1 >= gridWidth || y < 0 || y >= gridHeight)
        {
            PreviewManager.Instance?.ShowWarning("Sem espaço!");
            return;
        }

        if (IsCellOccupied(x, y) || IsCellOccupied(x + 1, y))
        {
            PreviewManager.Instance?.ShowWarning("Célula ocupada!");
            return;
        }

        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
            return;

        Vector3 pos = GridToWorld(x, y, centerX: true);
        GameObject obj = Instantiate(itemPrefabs[itemIndex], pos, Quaternion.identity);

        float scale = PreviewManager.Instance?.GetStoreScale() ?? 1f;
        obj.transform.localScale = Vector3.one * scale;

        grid[x, y] = obj;
        grid[x + 1, y] = obj;
        occupiedCells[x, y] = occupiedCells[x + 1, y] = true;

        foreach (var script in obj.GetComponentsInChildren<MonoBehaviour>())
            if (script.GetType() != typeof(Transform)) script.enabled = true;
        foreach (var col in obj.GetComponentsInChildren<Collider2D>())
            col.enabled = true;
        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            if (rend is SpriteRenderer sr)
            {
                Color c = sr.color; c.a = 1f; sr.color = c;
            }
            rend.sortingLayerName = "Items";
            rend.sortingOrder = 10;
        }

        Debug.Log($"[GMGR] Loja colocada em ({x},{y}) e ({x + 1},{y}) | escala: {scale}");
    }

    public void RemoveItem(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        if (!occupiedCells[x, y]) return;

        GameObject obj = grid[x, y];
        if (obj != null)
        {
            if (x + 1 < gridWidth && grid[x + 1, y] == obj)
            {
                grid[x + 1, y] = null;
                occupiedCells[x + 1, y] = false;
            }
            Destroy(obj);
        }
        grid[x, y] = null;
        occupiedCells[x, y] = false;
    }

    // ================================================================
    // === GIZMOS (DEBUG NO SCENE VIEW) ===
    // ================================================================

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Vector3 origin = new Vector3(gridOrigin.x, gridOrigin.y, 0);

        // Linhas verticais
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(x * cellSize, gridHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        // Linhas horizontais
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = origin + new Vector3(0, y * cellSize, 0);
            Vector3 end = origin + new Vector3(gridWidth * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
#endif
}