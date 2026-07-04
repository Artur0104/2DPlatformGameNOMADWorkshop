using UnityEngine;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Plataforma que se move entre dois pontos (A e B).
    /// Opcionalmente ativada apenas quando o jogador está sobre ela.
    /// Carrega o jogador junto ao se mover.
    /// </summary>
    public class PlataformaMovel : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField, Tooltip("Ponto A da plataforma.")]
        private Transform pontoA;

        [SerializeField, Tooltip("Ponto B da plataforma.")]
        private Transform pontoB;

        [SerializeField, Tooltip("Velocidade de movimento.")]
        private float velocidade = 2f;

        [SerializeField, Tooltip("Tempo de espera ao chegar em cada ponto (segundos).")]
        private float tempoEspera = 0.5f;

        [SerializeField, Tooltip("Se ativado, a plataforma só se move quando o jogador está sobre ela.")]
        private bool ativarComJogador = false;

        [SerializeField, Tooltip("Distância mínima para considerar que chegou ao destino.")]
        private float distanciaChegada = 0.05f;

        [Header("Referências")]
        [SerializeField, Tooltip("Tag do jogador.")]
        private string tagJogador = "Jogador";

        private Vector3 _posicaoInicial;
        private bool _indoParaB = true;
        private float _timerEspera = 0f;
        private bool _estaEsperando = false;
        private Transform _jogadorSobrePlataforma;

        private void Start()
        {
            _posicaoInicial = transform.position;
        }

        private void Update()
        {
            if (pontoA == null || pontoB == null) return;

            if (ativarComJogador && _jogadorSobrePlataforma == null) return;

            if (_estaEsperando)
            {
                _timerEspera -= Time.deltaTime;
                if (_timerEspera <= 0f)
                {
                    _estaEsperando = false;
                }
                return;
            }

            Vector3 destino = _indoParaB ? pontoB.position : pontoA.position;
            Vector3 posicaoAnterior = transform.position;

            transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);

            Vector3 deslocamento = transform.position - posicaoAnterior;

            if (_jogadorSobrePlataforma != null)
            {
                var rbJogador = _jogadorSobrePlataforma.GetComponent<Rigidbody2D>();
                if (rbJogador != null)
                    rbJogador.MovePosition(rbJogador.position + (Vector2)deslocamento);
                else
                    _jogadorSobrePlataforma.position += deslocamento;
            }

            if (Vector3.Distance(transform.position, destino) < distanciaChegada)
            {
                _indoParaB = !_indoParaB;
                _estaEsperando = true;
                _timerEspera = tempoEspera;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(tagJogador))
            {
                _jogadorSobrePlataforma = collision.transform;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(tagJogador))
            {
                _jogadorSobrePlataforma = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (pontoA != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(pontoA.position, 0.2f);
            }

            if (pontoB != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(pontoB.position, 0.2f);
            }

            if (pontoA != null && pontoB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pontoA.position, pontoB.position);
            }
        }
    }
}
