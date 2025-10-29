using UnityEngine;

public class Background : MonoBehaviour
{
    // Caso queira arrastar outro objeto como background
    public GameObject backgroundObject;

    void Start()
    {
        // Se não arrastou nada, usa o próprio GameObject
        if (backgroundObject == null)
            backgroundObject = gameObject;

        // Posiciona atrás de todos os elementos (Z maior)
        backgroundObject.transform.position = new Vector3(13.8f, 13.51f, 10f);
    }
}
