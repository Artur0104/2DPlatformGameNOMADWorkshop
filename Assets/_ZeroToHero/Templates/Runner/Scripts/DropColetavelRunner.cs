using UnityEngine;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Tipos de drop largados por inimigos ao morrer.
    /// </summary>
    public enum TipoDrop
    {
        XP,
        Vida
    }

    /// <summary>
    /// Gota de XP ou vida largada por inimigos ao morrer.
    /// É atraída magneticamente ao jogador quando dentro do raio de coleta.
    /// </summary>
    public class DropColetavelRunner : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Tipo de drop (XP ou Vida).")]
        private TipoDrop tipo = TipoDrop.XP;

        [SerializeField, Tooltip("Quantidade de XP ou vidas concedidas.")]
        private int quantidade = 5;

        [SerializeField, Tooltip("Velocidade de movimento.")]
        private float velocidade = 3f;

        [SerializeField, Tooltip("Raio de atração magnética ao jogador.")]
        private float raioColeta = 3f;

        [SerializeField, Tooltip("Tempo de vida máximo da gota (segundos).")]
        private float tempoVida = 10f;

        [SerializeField, Tooltip("Velocidade de descida quando fora do raio magnético.")]
        private float velocidadeDescida = 0.5f;

        private Transform _jogador;
        private float _timer;

        public TipoDrop Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }

        public int Quantidade
        {
            get { return quantidade; }
            set { quantidade = value; }
        }

        private void Start()
        {
            GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
            if (jogadorGo != null)
                _jogador = jogadorGo.transform;

            _timer = tempoVida;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (_jogador == null)
            {
                GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
                if (jogadorGo != null)
                    _jogador = jogadorGo.transform;
            }

            if (_jogador != null)
            {
                float distancia = Vector2.Distance(transform.position, _jogador.position);

                if (distancia <= raioColeta)
                {
                    Vector2 direcao = (_jogador.position - transform.position).normalized;
                    transform.Translate(direcao * velocidade * Time.deltaTime);
                }
                else
                {
                    transform.Translate(Vector2.down * velocidadeDescida * Time.deltaTime);
                }
            }
            else
            {
                transform.Translate(Vector2.down * velocidadeDescida * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Jogador")) return;

            if (tipo == TipoDrop.XP)
            {
                GerenciadorLevelUpRunner levelUp =
                    FindFirstObjectByType<GerenciadorLevelUpRunner>();
                if (levelUp != null)
                    levelUp.AdicionarXP(quantidade);
            }
            else if (tipo == TipoDrop.Vida)
            {
                NaveJogadorRunner jogador =
                    FindFirstObjectByType<NaveJogadorRunner>();
                if (jogador != null)
                    jogador.Curar(1);
            }

            Destroy(gameObject);
        }
    }
}
