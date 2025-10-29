// HudManager.cs
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Botões do HUD")]
    public Button cursorButton;
    public Button moveButton;
    public Button trashButton;

    [Header("Barra de Construção")]
    public RectTransform barra;
    public Canvas parentCanvas;

    private bool isBarVisible = false;
    private CanvasGroup canvasGroup;

    private readonly Vector3 escalaFixa = new Vector3(603.328f, 130.2784f, 1f);

    void Start()
    {
        if (barra == null)
        {
            Debug.LogError("Barra não atribuída no HudManager!");
            return;
        }

        barra.localScale = escalaFixa;

        if (parentCanvas != null)
        {
            barra.SetParent(parentCanvas.transform, false);
        }

        canvasGroup = barra.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = barra.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (cursorButton)
            cursorButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Place));
        if (moveButton)
            moveButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Drag));
        if (trashButton)
            trashButton.onClick.AddListener(() => MouseManager.Instance.SetMode(MouseManager.MouseMode.Delete));
    }

    void Update()
    {
        if (barra.localScale != escalaFixa)
            barra.localScale = escalaFixa;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isBarVisible = !isBarVisible;
            canvasGroup.alpha = isBarVisible ? 1f : 0f;
            canvasGroup.blocksRaycasts = isBarVisible;
            canvasGroup.interactable = isBarVisible;
        }
    }
}