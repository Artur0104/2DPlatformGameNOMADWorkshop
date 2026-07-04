using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Configuração de spawn para cada tipo de entidade (inimigos, meteoros, boss).
    /// </summary>
    [System.Serializable]
    public struct SpawnConfigRunner
    {
        [Tooltip("Offset Y relativo ao jogador onde a entidade nasce.")]
        public float alturaSpawn;

        [Tooltip("Largura total da faixa de spawn no eixo X. 0 = ponto fixo.")]
        public float larguraSpawn;
    }

    /// <summary>
    /// Gerenciador de spawn do Infinite Runner.
    /// Spawna inimigos e meteoros continuamente do topo da tela.
    /// Usa pool ponderada por pesoSpawn. Dificuldade aumenta com o tempo/distância.
    /// Spawna o Boss quando a distância atinge distanciaMinimaBoss.
    /// </summary>
    public class GerenciadorSpawnRunner : MonoBehaviour
    {
        [Header("Inimigos")]
        [SerializeField, Tooltip("Configurações dos inimigos disponíveis para spawn.")]
        private ConfiguracaoInimigoRunner[] inimigosDisponiveis;

        [SerializeField, Tooltip("Prefab base do inimigo.")]
        private GameObject prefabInimigoBase;

        [Header("Meteoros")]
        [SerializeField, Tooltip("Configurações dos meteoros disponíveis para spawn.")]
        private ConfiguracaoMeteoro[] meteorosDisponiveis;

        [SerializeField, Tooltip("Prefab base do meteoro.")]
        private GameObject prefabMeteoroBase;

        [Header("Boss")]
        [SerializeField, Tooltip("Prefab do Boss.")]
        private GameObject prefabBoss;

        [SerializeField, Tooltip("Configuração do Boss.")]
        private ConfiguracaoBossRunner configBoss;

        [SerializeField, Tooltip("Prefab do projétil do boss.")]
        private GameObject prefabProjetilBoss;

        [Header("Drops")]
        [SerializeField, Tooltip("Prefab da gota de XP.")]
        private GameObject prefabDropXp;

        [SerializeField, Tooltip("Prefab da gota de Vida.")]
        private GameObject prefabDropVida;

        [SerializeField, Tooltip("Prefab do texto flutuante de dano.")]
        private GameObject prefabTextoFlutuante;

        [SerializeField, Tooltip("Prefab do projétil inimigo.")]
        private GameObject prefabProjetilInimigo;

        [Header("Spawn — Inimigos")]
        [SerializeField, Tooltip("Ponto e área de spawn dos inimigos.")]
        private SpawnConfigRunner spawnInimigos = new SpawnConfigRunner { alturaSpawn = 6f, larguraSpawn = 8f };

        [Header("Spawn — Meteoros")]
        [SerializeField, Tooltip("Ponto e área de spawn dos meteoros.")]
        private SpawnConfigRunner spawnMeteoros = new SpawnConfigRunner { alturaSpawn = 7f, larguraSpawn = 6f };

        [Header("Spawn — Boss")]
        [SerializeField, Tooltip("Ponto e área de spawn do boss.")]
        private SpawnConfigRunner spawnBoss = new SpawnConfigRunner { alturaSpawn = 5f, larguraSpawn = 0f };

        [Header("Parâmetros de Spawn")]
        [SerializeField, Tooltip("Intervalo base entre spawns (segundos).")]
        private float intervaloBaseSpawn = 1.5f;

        [SerializeField, Tooltip("Intervalo mínimo de spawn (dificuldade máxima).")]
        private float intervaloMinimo = 0.2f;

        [SerializeField, Tooltip("Multiplicador de dificuldade: quanto o fator de aceleração impacta.")]
        private float multiplicadorDificuldade = 1f;

        [SerializeField, Tooltip("Distância mínima para spawnar o Boss.")]
        private float distanciaMinimaBoss = 500f;

        [SerializeField, Tooltip("Chance base de spawnar meteoro (0 a 1).")]
        [Range(0f, 1f)]
        private float chanceMeteoroBase = 0.3f;

        [Header("Multiplicadores Globais (para overlay)")]
        [SerializeField, Tooltip("Multiplicador de velocidade dos inimigos.")]
        private float multiplicadorVelocidadeInimigos = 1f;

        [SerializeField, Tooltip("Multiplicador de vida dos inimigos.")]
        private float multiplicadorVidaInimigos = 1f;

        private Transform _jogador;
        private NaveJogadorRunner _naveJogador;
        private float _timerSpawn;
        private float _tempoJogo;
        private bool _bossSpawnado;

        public float IntervaloBaseSpawn
        {
            get { return intervaloBaseSpawn; }
            set { intervaloBaseSpawn = value; }
        }

        public float MultiplicadorDificuldade
        {
            get { return multiplicadorDificuldade; }
            set { multiplicadorDificuldade = value; }
        }

        public float DistanciaMinimaBoss
        {
            get { return distanciaMinimaBoss; }
            set { distanciaMinimaBoss = value; }
        }

        public float ChanceMeteoroBase
        {
            get { return chanceMeteoroBase; }
            set { chanceMeteoroBase = Mathf.Clamp01(value); }
        }

        public float MultiplicadorVelocidadeInimigos
        {
            get { return multiplicadorVelocidadeInimigos; }
            set { multiplicadorVelocidadeInimigos = value; }
        }

        public float MultiplicadorVidaInimigos
        {
            get { return multiplicadorVidaInimigos; }
            set { multiplicadorVidaInimigos = value; }
        }

        public float VelocidadeMeteoroOverride { get; set; }

        public float VidaMeteoroOverride { get; set; }

        private void Start()
        {
            GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
            if (jogadorGo != null)
            {
                _jogador = jogadorGo.transform;
                _naveJogador = jogadorGo.GetComponent<NaveJogadorRunner>();
            }

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ProjetilInimigo"), LayerMask.NameToLayer("Inimigos"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ProjetilInimigo"), LayerMask.NameToLayer("ProjetilJogador"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ProjetilInimigo"), LayerMask.NameToLayer("Bordas"), true);

            _bossSpawnado = false;
            _tempoJogo = 0f;
            _timerSpawn = intervaloBaseSpawn;
        }

        private void Update()
        {
            if (_jogador == null || _naveJogador == null)
            {
                GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
                if (jogadorGo != null)
                {
                    _jogador = jogadorGo.transform;
                    _naveJogador = jogadorGo.GetComponent<NaveJogadorRunner>();
                }

                if (_jogador == null) return;
            }

            _tempoJogo += Time.deltaTime;

            if (!_bossSpawnado && prefabBoss != null)
            {
                float distancia = _naveJogador.DistanciaPercorrida;
                if (distancia >= distanciaMinimaBoss)
                {
                    SpawnarBoss();
                    return;
                }
            }

            float fatorAceleracao = _naveJogador.FatorAceleracao;
            float intervaloAtual = intervaloBaseSpawn / (1f + fatorAceleracao * multiplicadorDificuldade);
            intervaloAtual = Mathf.Max(intervaloAtual, intervaloMinimo);

            _timerSpawn -= Time.deltaTime;
            if (_timerSpawn <= 0f)
            {
                _timerSpawn = intervaloAtual;
                Spawnar();
            }
        }

        private void Spawnar()
        {
            bool spawnarMeteoro = Random.value < chanceMeteoroBase && meteorosDisponiveis.Length > 0;

            if (spawnarMeteoro)
                SpawnarMeteoro();
            else if (inimigosDisponiveis.Length > 0)
                SpawnarInimigo();
        }

        private void SpawnarInimigo()
        {
            ConfiguracaoInimigoRunner config = EscolherPorPeso(inimigosDisponiveis, _tempoJogo);
            if (config == null || prefabInimigoBase == null) return;

            Vector2 posSpawn = CalcularPosicaoSpawn(spawnInimigos);

            GameObject inimigoGo = Object.Instantiate(prefabInimigoBase, posSpawn, Quaternion.identity);
            inimigoGo.layer = LayerMask.NameToLayer("Inimigos");
            inimigoGo.tag = "Inimigo";
            InimigoRunner inimigo = inimigoGo.GetComponent<InimigoRunner>();
            if (inimigo != null)
            {
                inimigo.ConfigSource = config;
                inimigo.PrefabDropXp = prefabDropXp;
                inimigo.PrefabDropVida = prefabDropVida;
                inimigo.PrefabTextoFlutuante = prefabTextoFlutuante;
                inimigo.PrefabProjetil = prefabProjetilInimigo;
                inimigo.VelocidadeAtual = config.Velocidade * multiplicadorVelocidadeInimigos;
            }

            ComponenteVida vidaComp = inimigoGo.GetComponent<ComponenteVida>();
            if (vidaComp != null && config != null)
            {
                vidaComp.DefinirVidaMaxima(config.Vida * multiplicadorVidaInimigos);
            }
        }

        private void SpawnarMeteoro()
        {
            ConfiguracaoMeteoro config = EscolherMeteoroPorPeso();
            if (config == null || prefabMeteoroBase == null) return;

            Vector2 posSpawn = CalcularPosicaoSpawn(spawnMeteoros);

            GameObject meteoroGo = Object.Instantiate(prefabMeteoroBase, posSpawn, Quaternion.identity);
            meteoroGo.layer = LayerMask.NameToLayer("Inimigos");
            meteoroGo.tag = "Inimigo";
            MeteoroRunner meteoro = meteoroGo.GetComponent<MeteoroRunner>();
            if (meteoro != null)
            {
                meteoro.ConfigSource = config;
                float vel = VelocidadeMeteoroOverride > 0f ? VelocidadeMeteoroOverride : config.Velocidade;
                meteoro.VelocidadeAtual = vel;
            }

            ComponenteVida vidaComp = meteoroGo.GetComponent<ComponenteVida>();
            if (vidaComp != null)
            {
                float vida = VidaMeteoroOverride > 0f ? VidaMeteoroOverride : config.Vida;
                vidaComp.DefinirVidaMaxima(vida);
            }
        }

        /// <summary>
        /// Spawna o Boss imediatamente na posição relativa ao jogador.
        /// </summary>
        public void SpawnarBoss()
        {
            if (_bossSpawnado || prefabBoss == null || _jogador == null) return;

            _bossSpawnado = true;

            Vector2 posBoss = CalcularPosicaoSpawn(spawnBoss);

            GameObject bossGo = Object.Instantiate(prefabBoss, posBoss, Quaternion.identity) as GameObject;
            if (bossGo != null)
            {
                bossGo.layer = LayerMask.NameToLayer("Inimigos");
                bossGo.tag = "Inimigo";
                BossRunner boss = bossGo.GetComponent<BossRunner>();
                if (boss != null && configBoss != null)
                {
                    boss.ConfigSource = configBoss;
                    boss.PrefabProjetil = prefabProjetilBoss;
                }
            }
        }

        private Vector2 CalcularPosicaoSpawn(SpawnConfigRunner cfg)
        {
            float x = transform.position.x + (cfg.larguraSpawn > 0f
                ? Random.Range(-cfg.larguraSpawn * 0.5f, cfg.larguraSpawn * 0.5f)
                : 0f);
            float y = transform.position.y + cfg.alturaSpawn;
            return new Vector2(x, y);
        }

        private ConfiguracaoInimigoRunner EscolherPorPeso(ConfiguracaoInimigoRunner[] opcoes, float tempoJogo)
        {
            if (opcoes == null || opcoes.Length == 0) return null;

            float pesoTotal = 0f;
            for (int i = 0; i < opcoes.Length; i++)
            {
                if (opcoes[i] != null && tempoJogo >= opcoes[i].TempoMinimoSpawn)
                    pesoTotal += opcoes[i].PesoSpawn;
            }

            if (pesoTotal <= 0f) return null;

            float sorteio = Random.Range(0f, pesoTotal);
            float acumulado = 0f;

            for (int i = 0; i < opcoes.Length; i++)
            {
                if (opcoes[i] == null) continue;
                if (tempoJogo < opcoes[i].TempoMinimoSpawn) continue;

                acumulado += opcoes[i].PesoSpawn;
                if (sorteio <= acumulado)
                    return opcoes[i];
            }

            return opcoes[opcoes.Length - 1];
        }

        private ConfiguracaoMeteoro EscolherMeteoroPorPeso()
        {
            if (meteorosDisponiveis == null || meteorosDisponiveis.Length == 0) return null;

            float pesoTotal = 0f;
            for (int i = 0; i < meteorosDisponiveis.Length; i++)
            {
                if (meteorosDisponiveis[i] != null)
                    pesoTotal += meteorosDisponiveis[i].PesoSpawn;
            }

            if (pesoTotal <= 0f) return null;

            float sorteio = Random.Range(0f, pesoTotal);
            float acumulado = 0f;

            for (int i = 0; i < meteorosDisponiveis.Length; i++)
            {
                if (meteorosDisponiveis[i] == null) continue;
                acumulado += meteorosDisponiveis[i].PesoSpawn;
                if (sorteio <= acumulado)
                    return meteorosDisponiveis[i];
            }

            return meteorosDisponiveis[meteorosDisponiveis.Length - 1];
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origem = transform.position;

            DesenharFaixaSpawn(spawnInimigos, origem, Color.cyan);
            DesenharFaixaSpawn(spawnMeteoros, origem, Color.yellow);
            DesenharFaixaSpawn(spawnBoss, origem, Color.red);
        }

        private void DesenharFaixaSpawn(SpawnConfigRunner cfg, Vector3 origem, Color cor)
        {
            Gizmos.color = cor;

            float y = origem.y + cfg.alturaSpawn;
            float meiaLargura = cfg.larguraSpawn * 0.5f;

            if (cfg.larguraSpawn <= 0f)
            {
                Gizmos.DrawSphere(new Vector3(origem.x, y, origem.z), 0.3f);
            }
            else
            {
                Vector3 esquerda = new Vector3(origem.x - meiaLargura, y, origem.z);
                Vector3 direita = new Vector3(origem.x + meiaLargura, y, origem.z);
                Gizmos.DrawLine(esquerda, direita);
                Gizmos.DrawSphere(esquerda, 0.15f);
                Gizmos.DrawSphere(direita, 0.15f);
            }
        }
    }
}
