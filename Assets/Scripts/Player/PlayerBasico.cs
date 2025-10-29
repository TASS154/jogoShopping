// PlayerBasico.cs
using UnityEngine;

public class PlayerBasico : MonoBehaviour
{
    public float velocidade = 5f;
    public float forcaPulo = 8f;

    private Rigidbody2D rb;
    private bool estaNoChao;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float movimentoX = 0f;
        float movimentoY = 0f;

        if (Input.GetKey(KeyCode.A)) movimentoX = -1f;
        if (Input.GetKey(KeyCode.D)) movimentoX = 1f;
        if (Input.GetKey(KeyCode.W)) movimentoY = 1f;
        if (Input.GetKey(KeyCode.S)) movimentoY = -1f;

        rb.linearVelocity = new Vector2(movimentoX * velocidade, movimentoY * velocidade);

        if (Input.GetKeyDown(KeyCode.Space) && estaNoChao)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
        }

        if (movimentoX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movimentoX), 1f, 1f);
        }
    }
}