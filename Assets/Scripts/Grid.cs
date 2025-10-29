using UnityEngine;

public class GridBuilder2D : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    public GameObject[] objetos; // Prefabs 1–9 (sprites)
    public GameObject cursorPrefab; // Prefab do cursor visual
    public GameObject cellOutlinePrefab;

    private int selectedObject = 0;
    private int cursorX = 0;
    private int cursorY = 0;

    private GameObject[,] grid;
    private GameObject cursorInstance;

    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];

        // Cria o cursor visual
        cursorInstance = Instantiate(cursorPrefab, Vector3.zero, Quaternion.identity);
        cursorInstance.transform.localScale = Vector3.one * cellSize;
    }

    void Update()
    {
        // Seleciona objeto (1–9)
        for (int i = 0; i < objetos.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
                selectedObject = i;
        }


        // Movimenta cursor
        if (Input.GetKeyDown(KeyCode.UpArrow)) cursorY = Mathf.Clamp(cursorY + 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) cursorY = Mathf.Clamp(cursorY - 1, 0, gridHeight - 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) cursorX = Mathf.Clamp(cursorX - 1, 0, gridWidth - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) cursorX = Mathf.Clamp(cursorX + 1, 0, gridWidth - 1);

        // Atualiza posição do cursor visual (2D)
        cursorInstance.transform.position = new Vector3(cursorX * cellSize, cursorY * cellSize, 0);

        // Coloca objeto na célula atual
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = new Vector3(cursorX * cellSize, cursorY * cellSize, 0);

            if (grid[cursorX, cursorY] != null)
                Destroy(grid[cursorX, cursorY]);

            grid[cursorX, cursorY] = Instantiate(objetos[selectedObject], pos, Quaternion.identity);
        }

        // Remove objeto
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (grid[cursorX, cursorY] != null)
            {
                Destroy(grid[cursorX, cursorY]);
                grid[cursorX, cursorY] = null;
            }
        }
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 1); // Z=1 para não cobrir sprites
                Instantiate(cellOutlinePrefab, pos, Quaternion.identity, transform);
            }
        }
    }
}

