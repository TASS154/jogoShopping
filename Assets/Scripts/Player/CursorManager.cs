// CursorManager.cs
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Configurações do Cursor")]
    public Texture2D normalCursorTexture;
    public Vector2 normalHotSpot = Vector2.zero;
    public Texture2D moveCursorTexture;
    public Vector2 moveHotSpot = new Vector2(16f, 16f);
    public Texture2D onMoveCursorTexture;
    public Vector2 onMoveHotSpot = new Vector2(16f, 16f);
    public Texture2D trashCursorTexture;
    public Vector2 trashHotSpot = new Vector2(16f, 16f);
    public CursorMode cursorMode = CursorMode.Auto;

    public static CursorManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetNormalCursor();
    }

    public void SetNormalCursor()
    {
        if (normalCursorTexture != null)
        {
            Cursor.SetCursor(normalCursorTexture, normalHotSpot, cursorMode);
            Debug.Log("Cursor normal aplicado.");
        }
        else
        {
            Debug.LogError("Textura normal do cursor não atribuída!");
        }
    }

    public void SetMoveCursor()
    {
        if (moveCursorTexture != null)
        {
            Cursor.SetCursor(moveCursorTexture, moveHotSpot, cursorMode);
            Debug.Log("Cursor 'move' aplicado.");
        }
        else
        {
            Debug.LogError("Textura 'move' do cursor não atribuída!");
        }
    }

    public void SetOnMoveCursor()
    {
        if (onMoveCursorTexture != null)
        {
            Cursor.SetCursor(onMoveCursorTexture, onMoveHotSpot, cursorMode);
            Debug.Log("Cursor 'onmove' aplicado.");
        }
        else
        {
            Debug.LogError("Textura 'onmove' do cursor não atribuída!");
        }
    }

    public void SetTrashCursor()
    {
        if (trashCursorTexture != null)
        {
            Cursor.SetCursor(trashCursorTexture, trashHotSpot, cursorMode);
            Debug.Log("Cursor 'trash' aplicado.");
        }
        else
        {
            Debug.LogError("Textura 'trash' do cursor não atribuída!");
        }
    }

    public void ResetToDefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ChangeCursorForItem(int itemIndex)
    {
        SetNormalCursor();
    }
}