using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configurações da Grade")]
    public int gridWidth = 20; // Largura da grade em células
    public int gridHeight = 20; // Altura da grade em células
    public float cellSize = 1f; // Tamanho de cada célula (ajuste para combinar com seus assets)

    [Header("Prefabs dos Itens")]
    public GameObject[] itemPrefabs; // Array de prefabs para itens 0-8 (atribua no Inspector)

    private bool[,] occupiedCells; // Array para rastrear células ocupadas
    public static GridManager Instance { get; private set; } // Singleton pattern

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        occupiedCells = new bool[gridWidth, gridHeight]; // Inicializa todas como falsas (livres)
    }

    // Removido Update: Lógica de input agora no MouseManager

    public bool IsCellOccupied(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return occupiedCells[x, y];
        }
        return true; // Fora dos limites = considerado ocupado
    }

    public void PlaceItem(int x, int y, int itemIndex, Vector3 pos)
    {
        if (itemIndex < 0 || itemIndex >= itemPrefabs.Length || itemPrefabs[itemIndex] == null)
        {
            Debug.LogWarning("Prefab de item inválido ou não atribuído!");
            return;
        }

        Instantiate(itemPrefabs[itemIndex], pos, Quaternion.identity);
        occupiedCells[x, y] = true;
    }
}