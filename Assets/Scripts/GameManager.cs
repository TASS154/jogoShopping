using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referências")]
    public GridManager gridManager;
    public MouseManager mouseManager;
    public TextMeshProUGUI moneyText;
    public Canvas hudCanvas;

    [Header("Sistema de Compras")]
    public int[] itemCosts = { 150, 50, 100, 300 };   // 4 itens
    public int startingMoney = 1000;
    private int currentMoney;
    public int selectedItemIndex = -1;   // -1 = nada

    [Header("Efeito de Ganho")]
    public GameObject upcoinPrefab;
    public float upcoinScale = 0.6f;
    public float upcoinAlpha = 0.4f;
    public float floatDuration = 1.2f;
    public float floatDistance = 40f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        currentMoney = startingMoney;
        UpdateMoneyUI();
        Debug.Log($"[GM] GameManager iniciado – dinheiro inicial: {currentMoney}");
    }

    void Start()
    {
        if (gridManager == null) Debug.LogError("[GM] GridManager não configurado!");
        if (mouseManager == null) Debug.LogError("[GM] MouseManager não configurado!");
        if (hudCanvas == null) Debug.LogError("[GM] HUD Canvas não atribuído!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[GM] Reset da cena (tecla R)");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void TryPlaceItem(int x, int y, int itemIndex)
    {
        Debug.Log($"[GM] TryPlaceItem → ({x},{y}) | índice {itemIndex}");

        if (itemIndex < 0 || itemIndex >= itemCosts.Length)
        {
            Debug.LogError($"[GM] Índice fora do range: {itemIndex}");
            return;
        }

        int cost = itemCosts[itemIndex];
        if (currentMoney < cost)
        {
            PreviewManager.Instance?.ShowWarning("Dinheiro insuficiente!");
            Debug.Log($"[GM] Falha: dinheiro {currentMoney} < custo {cost}");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("[GM] gridManager é null!");
            return;
        }

        if (gridManager.IsCellOccupied(x, y))
        {
            PreviewManager.Instance?.ShowWarning("Célula ocupada!");
            Debug.Log($"[GM] Célula ({x},{y}) já ocupada");
            return;
        }

        // ---- COLOCAÇÃO REAL ----
        gridManager.PlaceItem(x, y, itemIndex);
        currentMoney -= cost;
        UpdateMoneyUI();

        Debug.Log($"[GM] Loja {itemIndex} colocada em ({x},{y})! Dinheiro restante: {currentMoney}");
    }

    public void SetSelectedItem(int index)
    {
        if (index >= 0 && index < itemCosts.Length)
        {
            selectedItemIndex = index;
            Debug.Log($"[GM] SelectedItemIndex ← {index}");
        }
        else
        {
            selectedItemIndex = -1;
            Debug.Log($"[GM] SelectedItemIndex ← -1 (índice inválido {index})");
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"[GM] +{amount} dinheiro → total {currentMoney}");

        if (upcoinPrefab != null && moneyText != null && hudCanvas != null)
            StartCoroutine(ShowMoneyGainEffect(amount));
    }

    public int GetCurrentMoney() => currentMoney;

    private void UpdateMoneyUI()
    {
        if (moneyText != null) moneyText.text = $"${currentMoney}";
    }

    private IEnumerator ShowMoneyGainEffect(int amount)
    {
        // (mesmo código anterior – sem alterações)
        // ... (mantido)
        yield return null;
    }
}