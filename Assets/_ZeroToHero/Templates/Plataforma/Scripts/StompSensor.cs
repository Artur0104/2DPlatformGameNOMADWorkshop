using UnityEngine;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Sensor de stomp: BoxCollider2D trigger posicionado no topo do inimigo.
    /// Detecta quando o jogador pisa no inimigo e notifica o InimigoPatrulha pai.
    /// </summary>
    public class StompSensor : MonoBehaviour
    {
        [SerializeField, Tooltip("Tag do jogador.")]
        private string tagJogador = "Jogador";

        private InimigoPatrulha _inimigoPai;

        private void Awake()
        {
            _inimigoPai = GetComponentInParent<InimigoPatrulha>();
            if (_inimigoPai == null)
            {
                Debug.LogError("[StompSensor] InimigoPatrulha nao encontrado no parent.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(tagJogador)) return;

            var jogadorRb = other.GetComponent<Rigidbody2D>();
            if (jogadorRb == null) return;

            _inimigoPai?.AplicarStomp(jogadorRb);
        }
    }
}
