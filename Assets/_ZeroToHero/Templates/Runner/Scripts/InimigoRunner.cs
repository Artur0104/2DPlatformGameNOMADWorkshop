using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Inimigo do Infinite Runner.
    /// Spawna no topo, desce com desvio lateral em direção ao jogador.
    /// Inimigos do tipo Grande disparam projéteis.
    /// Ao morrer, dropa XP (gota magnética) e chance de vida.
    /// </summary>
    public class InimigoRunner : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject com parâmetros do inimigo.")]
        private ConfiguracaoInimigoRunner config;

        [Header("Projétil (para inimigos que atiram)")]
        [SerializeField, Tooltip("Prefab do projétil disparado. Necessário se config.atiraProjeteis = true.")]
        private GameObject projetilPrefab;

        [SerializeField, Tooltip("Ponto de origem do disparo.")]
        private Transform pontoDisparo;

        [Header("Drops")]
        [SerializeField, Tooltip("Prefab da gota de XP.")]
        private GameObject prefabDropXp;

        [SerializeField, Tooltip("Prefab da gota de Vida.")]
        private GameObject prefabDropVida;

        [SerializeField, Tooltip("Prefab do texto flutuante de dano.")]
        private GameObject prefabTextoFlutuante;

        private Transform _jogador;
        private float _timerTiro;
        private float _vidaAtual;
        private float _velocidadeAtual;
        private float _danoAtual;

        public float VelocidadeAtual
        {
            get { return _velocidadeAtual; }
            set { _velocidadeAtual = value; }
        }

        public float VidaAtual
        {
            get { return _vidaAtual; }
        }

        public float DanoAtual
        {
            get { return _danoAtual; }
        }

        public ConfiguracaoInimigoRunner ConfigSource
        {
            set { config = value; }
        }

        public GameObject PrefabDropXp
        {
            set { prefabDropXp = value; }
        }

        public GameObject PrefabDropVida
        {
            set { prefabDropVida = value; }
        }

        public GameObject PrefabTextoFlutuante
        {
            set { prefabTextoFlutuante = value; }
        }

        public GameObject PrefabProjetil
        {
            set { projetilPrefab = value; }
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
            _danoAtual = config.DanoContato;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            if (vida != null)
            {
                vida.DefinirVidaMaxima(_vidaAtual);
                vida.onDano.AddListener(AoReceberDano);
                vida.onMorte.AddListener(AoMorrer);
            }

            GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
            if (jogadorGo != null)
                _jogador = jogadorGo.transform;

            if (sr != null && config.Sprite != null)
                sr.sprite = config.Sprite;

            _timerTiro = config.CooldownTiro * 0.5f;
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            if (config == null) return;

            Vector3 movimento = Vector3.down * _velocidadeAtual * Time.deltaTime;

            if (_jogador != null && config.DesvioX > 0f)
            {
                float diffX = _jogador.position.x - transform.position.x;
                float desvio = Mathf.Sign(diffX) * Mathf.Min(Mathf.Abs(diffX) * config.DesvioX * Time.deltaTime, config.DesvioX * Time.deltaTime);
                movimento.x = desvio;
            }

            transform.Translate(movimento, Space.World);

            if (config.AtiraProjeteis && _jogador != null)
            {
                _timerTiro -= Time.deltaTime;
                if (_timerTiro <= 0f)
                {
                    _timerTiro = config.CooldownTiro;
                    DispararProjetil();
                }
            }

            if (Mathf.Abs(transform.position.y) > 10f)
                Destroy(gameObject);
        }

        private void DispararProjetil()
        {
            if (projetilPrefab == null) return;

            Vector2 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
            Vector2 direcao = Vector2.down;

            GameObject projetilGo = Object.Instantiate(projetilPrefab, origem, Quaternion.identity);
            if (projetilGo == null) return;

            projetilGo.layer = LayerMask.NameToLayer("ProjetilInimigo");

            ComponenteProjetil compProjetil = projetilGo.GetComponent<ComponenteProjetil>();
            if (compProjetil != null)
            {
                compProjetil.Disparar(direcao);
                compProjetil.InicializarValores(config.DanoProjetil, config.VelocidadeProjetil, 5f, false, LayerMask.GetMask("Default"));
            }
        }

        private void AoReceberDano(float dano)
        {
            if (prefabTextoFlutuante != null)
            {
                Vector3 pos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, 0f);
                GameObject textoGo = Object.Instantiate(prefabTextoFlutuante, pos, Quaternion.identity);
                TextoFlutuanteRunner textoFlutuante = textoGo.GetComponent<TextoFlutuanteRunner>();
                if (textoFlutuante != null)
                {
                    textoFlutuante.Inicializar("-" + dano.ToString("F0"), Color.red);
                }
            }
        }

        private void AoMorrer()
        {
            SpawnarDropXP();
            SpawnarDropVida();

            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            Destroy(gameObject, 0.1f);
        }

        private void SpawnarDropXP()
        {
            if (prefabDropXp == null || config.XpDrop <= 0) return;
            if (Random.value > config.ChanceDropXp) return;

            GameObject dropGo = Object.Instantiate(prefabDropXp, transform.position, Quaternion.identity);
            DropColetavelRunner drop = dropGo.GetComponent<DropColetavelRunner>();
            if (drop != null)
            {
                drop.Tipo = TipoDrop.XP;
                drop.Quantidade = config.XpDrop;
            }
        }

        private void SpawnarDropVida()
        {
            if (prefabDropVida == null) return;

            float chance = config.ChanceDropVida;
            if (chance <= 0f) return;
            if (Random.value > chance) return;

            GameObject dropGo = Object.Instantiate(prefabDropVida, transform.position, Quaternion.identity);
            DropColetavelRunner drop = dropGo.GetComponent<DropColetavelRunner>();
            if (drop != null)
            {
                drop.Tipo = TipoDrop.Vida;
                drop.Quantidade = 1;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Jogador")) return;

            NaveJogadorRunner jogador = other.GetComponent<NaveJogadorRunner>();
            if (jogador != null)
                jogador.ReceberDano();

            Destroy(gameObject);
        }
    }
}
