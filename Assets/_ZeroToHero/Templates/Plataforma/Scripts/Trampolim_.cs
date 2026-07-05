using UnityEngine;

namespace ZeroToHero.Cenas.Plataforma
{
    public class Trampolim : MonoBehaviour
    {
        [SerializeField] private float forcaDoPulo = 15f;
        [SerializeField] private string tagJogador = "Jogador";

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(tagJogador))
            {
                bool jogadorAcima = collision.transform.position.y > transform.position.y;

                if (jogadorAcima)
                {
                    Rigidbody2D rbJogador = collision.gameObject.GetComponent<Rigidbody2D>();

                    if (rbJogador != null)
                    {
                        rbJogador.linearVelocity = new Vector2(rbJogador.linearVelocity.x, forcaDoPulo);
                    }
                }
            }
        }
    }
}