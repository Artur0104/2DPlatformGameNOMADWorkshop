using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Meteoro do Infinite Runner.
    /// Obstáculo destrutível que desce do topo em linha reta.
    /// Tem vida e pode ser destruído a tiros do jogador.
    /// </summary>
    public class MeteoroRunner : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject com parâmetros do meteoro.")]
        private ConfiguracaoMeteoro config;

        private float _vidaAtual;
        private float _velocidadeAtual;

        public float VelocidadeAtual
        {
            get { return _velocidadeAtual; }
            set { _velocidadeAtual = value; }
        }

        public ConfiguracaoMeteoro ConfigSource
        {
            set { config = value; }
        }

        /// <inheritdoc/>
        protected override void AoIniciar()
        {
            if (config == null)
            {
                Destroy(gameObject);
                return;
            }

            _vidaAtual = config.Vida;
            _velocidadeAtual = config.Velocidade;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            if (vida != null)
            {
                vida.DefinirVidaMaxima(_vidaAtual);
                vida.onMorte.AddListener(AoMorrer);
            }
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            if (config == null) return;

            transform.Translate(Vector3.down * _velocidadeAtual * Time.deltaTime, Space.World);

            if (Mathf.Abs(transform.position.y) > 10f)
                Destroy(gameObject);
        }

        private void AoMorrer()
        {
            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Jogador"))
            {
                NaveJogadorRunner jogador = other.GetComponent<NaveJogadorRunner>();
                if (jogador != null)
                    jogador.ReceberDano();

                Destroy(gameObject);
            }
        }
    }
}
