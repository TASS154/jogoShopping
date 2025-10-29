using UnityEngine;
using TMPro;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [Header("=== PREFABS DAS LOJAS (4) ===")]
    public GameObject[] storePrefabsArray = new GameObject[4];

    [Header("Configurações Visuais")]
    public float storeScale = 3.5f;           // Ajustado para cellSize = 4
    public float ghostAlpha = 0.3f;
    public Color validColor = new Color(1f, 1f, 1f, 0.3f);
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.3f);

    [Header("UI Feedback")]
    public TextMeshProUGUI storeNameText;
    public TextMeshProUGUI costWarningText;

    private GameObject previewInstance;
    private int currentPreviewIndex = -1;
    private float lastSelectionTime = 0f;
    private const float DOUBLE_TAP_THRESHOLD = 0.3f;

    private readonly string[] storeNames = { "Hamburgueria", "Padaria", "Abibas", "Arcade Alley" };
    private readonly int[] storeCosts = { 150, 50, 100, 300 };

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        HidePreview();
        UpdateStoreUI();
        Debug.Log("[PM] PreviewManager iniciado");
    }

    void Update()
    {
        HandleStoreSelection();
        HandleCancelInput();
        HandleAddMoney();
        UpdatePreviewPosition();
    }

    #region INPUT HANDLERS
    private void HandleStoreSelection()
    {
        int index = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;

        if (index != -1)
        {
            float timeSinceLast = Time.time - lastSelectionTime;
            if (currentPreviewIndex == index && timeSinceLast < DOUBLE_TAP_THRESHOLD)
            {
                CancelSelection();
            }
            else
            {
                SelectStore(index);
            }
            lastSelectionTime = Time.time;
        }
    }

    private void HandleCancelInput()
    {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.Space))
            CancelSelection();
    }

    private void HandleAddMoney()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            GameManager.Instance?.AddMoney(100);
    }
    #endregion

    public void SelectStore(int storeIndex)
    {
        if (storeIndex < 0 || storeIndex >= storePrefabsArray.Length || storePrefabsArray[storeIndex] == null)
        {
            ShowWarning("Prefab inválido!");
            return;
        }

        int cost = storeCosts[storeIndex];
        if (GameManager.Instance.GetCurrentMoney() < cost)
        {
            ShowWarning("Dinheiro insuficiente!");
            return;
        }

        currentPreviewIndex = storeIndex;
        GameManager.Instance?.SetSelectedItem(storeIndex);
        CreatePreview();
        UpdateStoreUI();
        HideWarning();
    }

    public void CancelSelection()
    {
        if (currentPreviewIndex == -1) return;
        GameManager.Instance?.SetSelectedItem(-1);
        HidePreview();
    }

    #region PREVIEW LOGIC
    private void CreatePreview()
    {
        if (previewInstance != null) Destroy(previewInstance);

        GameObject prefab = storePrefabsArray[currentPreviewIndex];
        previewInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        previewInstance.name = "StorePreview";
        previewInstance.transform.localScale = Vector3.one * storeScale;

        ApplyGhostEffect(previewInstance);

        // Força sorting layer
        foreach (var rend in previewInstance.GetComponentsInChildren<Renderer>())
        {
            rend.sortingLayerName = "Preview";
            rend.sortingOrder = 20;
        }
    }

    private void ApplyGhostEffect(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            if (r is SpriteRenderer sr)
            {
                Color c = sr.color;
                c.a = ghostAlpha;
                sr.color = c;
            }
        }
        foreach (var col in obj.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        foreach (var script in obj.GetComponentsInChildren<MonoBehaviour>())
            if (script.GetType() != typeof(Transform)) script.enabled = false;
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null || GridManager.Instance == null || Camera.main == null)
            return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
        int x = gridPos.x;
        int y = gridPos.y;

        // === VALIDAÇÃO: 2 CÉLULAS LIVRES ===
        if (x < 0 || x + 1 >= GridManager.Instance.gridWidth || y < 0 || y >= GridManager.Instance.gridHeight)
        {
            previewInstance.SetActive(false);
            return;
        }

        previewInstance.SetActive(true);
        previewInstance.transform.position = GridManager.Instance.GridToWorld(x, y, centerX: true);

        bool isValid = !GridManager.Instance.IsCellOccupied(x, y) && !GridManager.Instance.IsCellOccupied(x + 1, y);
        Color target = isValid ? validColor : invalidColor;

        foreach (Renderer r in previewInstance.GetComponentsInChildren<Renderer>())
        {
            if (r is SpriteRenderer sr)
                sr.color = Color.Lerp(sr.color, target, Time.deltaTime * 20f);
        }
    }
    #endregion

    #region UI
    private void UpdateStoreUI()
    {
        if (storeNameText != null)
        {
            storeNameText.text = currentPreviewIndex >= 0
                ? $"{storeNames[currentPreviewIndex]} - Custo: ${storeCosts[currentPreviewIndex]}"
                : "Selecione uma loja (1-4)";
        }
    }

    public void ShowWarning(string message)
    {
        if (costWarningText != null)
        {
            costWarningText.text = message;
            costWarningText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideWarning));
            Invoke(nameof(HideWarning), 2f);
        }
    }

    private void HideWarning()
    {
        if (costWarningText != null) costWarningText.gameObject.SetActive(false);
    }

    public void HidePreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
        currentPreviewIndex = -1;
        UpdateStoreUI();
    }
    #endregion

    public float GetStoreScale() => storeScale;

    void OnDestroy() => HidePreview();
}