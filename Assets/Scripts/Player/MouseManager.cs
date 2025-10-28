using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("Configurações de Arrasto")]
    public float dragSpeed = 0.1f; // Velocidade do arrasto da câmera (ajuste no Inspector)
    public float dragThreshold = 5f; // Distância mínima para considerar arrasto (evita mudanças acidentais)

    private bool dragMode = false; // Modo de arrasto ativado?
    private bool isDragging = false; // Está arrastando agora?
    private Vector3 lastMousePosition;

    void Update()
    {
        // Toggle modo de arrasto com Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dragMode = true;
            CursorManager.Instance.SetMoveCursor();
            Debug.Log("Modo de arrasto ativado (cursor 'move').");
        }

        // Sai do modo de arrasto com V
        if (Input.GetKeyDown(KeyCode.V))
        {
            dragMode = false;
            CursorManager.Instance.SetNormalCursor();
            Debug.Log("Modo de arrasto desativado (cursor normal).");
        }

        // Obtém posição do mouse
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        int x = Mathf.FloorToInt(worldPos.x / GridManager.Instance.cellSize);
        int y = Mathf.FloorToInt(worldPos.y / GridManager.Instance.cellSize);

        // Lógica baseada no modo
        if (dragMode)
        {
            // Modo de arrasto: Não coloca itens, apenas arrasta câmera
            if (Input.GetMouseButtonDown(0))
            {
                CursorManager.Instance.SetMoveCursor();
                isDragging = true;
                lastMousePosition = mousePos;
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 delta = mousePos - lastMousePosition;
                if (delta.magnitude > dragThreshold)
                {
                    CursorManager.Instance.SetOnMoveCursor();
                    // Arraste a câmera na direção oposta ao movimento do mouse
                    Camera.main.transform.position -= new Vector3(delta.x * dragSpeed, delta.y * dragSpeed, 0);
                }
                lastMousePosition = mousePos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                CursorManager.Instance.SetMoveCursor();
                isDragging = false;
            }
        }
        else
        {
            // Modo normal: Coloca itens na grade
            if (Input.GetMouseButtonDown(0))
            {
                // Verifica limites da grade
                if (x >= 0 && x < GridManager.Instance.gridWidth && y >= 0 && y < GridManager.Instance.gridHeight)
                {
                    int selected = GameManager.Instance.selectedItemIndex;
                    if (selected >= 0)
                    {
                        GameManager.Instance.TryPlaceItem(x, y, selected);
                    }
                    else
                    {
                        Debug.Log("Nenhum item selecionado! Configure via UI ou Inspector.");
                    }
                }
            }
        }
    }
}