using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Torre de defesa para Tower Defense.
    /// Ataca automaticamente o inimigo mais próximo dentro do alcance.
    /// Suporta melhoria (upgrade), venda e efeitos de status.
    /// </summary>
    public class Torre : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject de configuração da torre.")]
        private ConfiguracaoTorre configTorre;

        [Header("Atributos da Torre")]
        [SerializeField, Tooltip("Alcance de ataque da torre.")]
        private float alcance = 3f;

        [SerializeField, Tooltip("Dano por ataque.")]
        private float dano = 10f;

        [SerializeField, Tooltip("Velocidade de ataque (ataques por segundo).")]
        private float velocidadeAtaque = 1f;

        [SerializeField, Tooltip("Custo de melhoria (moedas).")]
        private int custoMelhoria = 75;

        [Header("Visual")]
        [SerializeField, Tooltip("SpriteRenderer do círculo de alcance (opcional).")]
        private SpriteRenderer circuloAlcance;

        [SerializeField, Tooltip("Material da linha de ataque.")]
        private Material materialLinhaAtaque;

        [Header("Alvos")]
        [SerializeField, Tooltip("Tag dos inimigos.")]
        private string tagInimigo = "Inimigo";

        [SerializeField, Tooltip("LayerMask dos alvos para dano em área.")]
        private LayerMask camadaInimigos = -1;

        [Header("Eventos")]
        public static System.Action<Torre> OnTorreSelecionada;
        public static System.Action<int> OnTorreMelhorada;
        public static System.Action OnTorreDestruida;

        private float _timerAtaque = 0f;
        private Transform _alvoAtual;
        private Vector2Int _celulaOcupada = new Vector2Int(-1, -1);
        private int _nivel = 1;
        private LineRenderer _linhaAtaque;

        protected override void Awake()
        {
            base.Awake();
            ConfigurarLineRenderer();
        }

        private void ConfigurarLineRenderer()
        {
            _linhaAtaque = GetComponent<LineRenderer>();
            if (_linhaAtaque == null)
            {
                _linhaAtaque = gameObject.AddComponent<LineRenderer>();
            }

            _linhaAtaque.positionCount = 2;
            _linhaAtaque.startWidth = 0.1f;
            _linhaAtaque.endWidth = 0.05f;
            _linhaAtaque.enabled = false;

            if (materialLinhaAtaque != null)
                _linhaAtaque.material = materialLinhaAtaque;
            else
                _linhaAtaque.material = new Material(Shader.Find("Sprites/Default"));

            _linhaAtaque.startColor = Color.yellow;
            _linhaAtaque.endColor = Color.yellow;
        }

        protected override void AoIniciar()
        {
            base.AoIniciar();

            if (configTorre != null)
            {
                dano = configTorre.dano;
                alcance = configTorre.alcance;
                velocidadeAtaque = configTorre.velocidadeAtaque;
                custoMelhoria = configTorre.custoMelhoria;

                if (_linhaAtaque != null)
                {
                    _linhaAtaque.startColor = configTorre.corProjetil;
                    _linhaAtaque.endColor = configTorre.corProjetil;
                }
            }

            if (circuloAlcance != null)
            {
                float diametro = alcance * 2f;
                circuloAlcance.transform.localScale = new Vector3(diametro, diametro, 1f);
            }
        }

        protected override void AoAtualizar()
        {
            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando)
            {
                return;
            }

            _timerAtaque -= Time.deltaTime;

            EncontrarAlvo();

            if (_alvoAtual != null && _timerAtaque <= 0f)
            {
                AtacarAlvo();
                _timerAtaque = 1f / velocidadeAtaque;
            }
        }

        /// <summary>Encontra o inimigo mais próximo dentro do alcance.</summary>
        private void EncontrarAlvo()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alcance, camadaInimigos);

            float menorDistancia = alcance;
            Transform melhorAlvo = null;

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag(tagInimigo)) continue;
                float distancia = Vector2.Distance(transform.position, hit.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    melhorAlvo = hit.transform;
                }
            }

            _alvoAtual = melhorAlvo;
        }

        /// <inheritdoc/>
        protected virtual void AtacarAlvo()
        {
            if (_alvoAtual == null) return;

            ComponenteVida vidaInimigo = _alvoAtual.GetComponent<ComponenteVida>();
            if (vidaInimigo != null)
            {
                vidaInimigo.ReceberDano(dano);
            }

            AplicarStatus(_alvoAtual.gameObject);

            if (efeito != null)
            {
                efeito.TocarSomAtaque();
            }

            if (sr != null)
            {
                StartCoroutine(PiscarTorre());
            }

            if (_linhaAtaque != null)
            {
                _linhaAtaque.SetPosition(0, transform.position);
                _linhaAtaque.SetPosition(1, _alvoAtual.position);
                _linhaAtaque.enabled = true;
                StartCoroutine(DesligarLinha());
            }
        }

        private System.Collections.IEnumerator DesligarLinha()
        {
            yield return new WaitForSeconds(0.1f);
            if (_linhaAtaque != null)
                _linhaAtaque.enabled = false;
        }

        private void AplicarStatus(GameObject alvo)
        {
            if (configTorre == null) return;

            switch (configTorre.tipo)
            {
                case TipoTorre.Gelo:
                    StartCoroutine(AplicarSlow(alvo, configTorre.intensidadeStatus, configTorre.duracaoStatus));
                    break;
                case TipoTorre.Veneno:
                case TipoTorre.Fogo:
                    StartCoroutine(AplicarDoT(alvo, configTorre.intensidadeStatus, configTorre.duracaoStatus));
                    break;
                case TipoTorre.Explosivo:
                    AplicarDanoEmArea();
                    break;
            }
        }

        private System.Collections.IEnumerator AplicarSlow(GameObject alvo, float intensidade, float duracao)
        {
            InimigoCaminho inimigo = alvo.GetComponent<InimigoCaminho>();
            if (inimigo == null) yield break;

            inimigo.AplicarSlow(intensidade);

            yield return new WaitForSeconds(duracao);

            if (inimigo != null)
                inimigo.RestaurarVelocidade();
        }

        private System.Collections.IEnumerator AplicarDoT(GameObject alvo, float danoPorSegundo, float duracao)
        {
            float timer = 0f;
            while (timer < duracao && alvo != null)
            {
                InimigoCaminho inimigo = alvo.GetComponent<InimigoCaminho>();
                if (inimigo != null)
                    inimigo.AplicarDanoDOT(danoPorSegundo * Time.deltaTime);
                else
                {
                    ComponenteVida vida = alvo.GetComponent<ComponenteVida>();
                    if (vida != null)
                        vida.ReceberDano(danoPorSegundo * Time.deltaTime);
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void AplicarDanoEmArea()
        {
            if (_alvoAtual == null || configTorre == null || configTorre.raioExplosao <= 0f) return;

            Vector2 posicao = _alvoAtual.position;
            Collider2D[] alvos = Physics2D.OverlapCircleAll(posicao, configTorre.raioExplosao, camadaInimigos);

            foreach (Collider2D colisor in alvos)
            {
                if (colisor.gameObject == _alvoAtual.gameObject) continue;

                ComponenteVida vida = colisor.GetComponent<ComponenteVida>();
                if (vida != null)
                {
                    vida.ReceberDano(dano * 0.5f);
                }
            }
        }

        private System.Collections.IEnumerator PiscarTorre()
        {
            if (sr == null) yield break;
            Color corOriginal = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = corOriginal;
        }

        /// <summary>Melhora a torre, aumentando dano, alcance e velocidade.</summary>
        public void Melhorar()
        {
            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando) return;

            if (configTorre != null && _nivel >= configTorre.nivelMaximo)
            {
                if (GerenciadorHUD.Instancia != null)
                    GerenciadorHUD.Instancia.MostrarMensagem("Torre no nível máximo!");
                return;
            }

            if (GerenciadorHUD.Instancia != null)
            {
                if (!GerenciadorHUD.Instancia.GastarMoedas(custoMelhoria))
                {
                    GerenciadorHUD.Instancia.MostrarMensagem("Moedas insuficientes para melhoria!");
                    return;
                }
            }

            _nivel++;

            if (configTorre != null)
            {
                dano *= configTorre.multiplicadorDanoPorNivel;
                velocidadeAtaque *= configTorre.multiplicadorVelocidadePorNivel;
                alcance *= configTorre.multiplicadorAlcancePorNivel;
            }
            else
            {
                dano *= 1.5f;
                velocidadeAtaque *= 1.2f;
                alcance *= 1.1f;
            }

            if (circuloAlcance != null)
            {
                float diametro = alcance * 2f;
                circuloAlcance.transform.localScale = new Vector3(diametro, diametro, 1f);
            }

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.MostrarMensagem(string.Format("Torre nível {0}!", _nivel));
            }

            if (OnTorreMelhorada != null)
                OnTorreMelhorada(_nivel);
        }

        /// <summary>Vende a torre, concedendo reembolso de 50% do custo.</summary>
        public void Vender()
        {
            int reembolso = configTorre != null ? configTorre.custo / 2 : custoMelhoria / 2;

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.AdicionarMoedas(reembolso);
                GerenciadorHUD.Instancia.MostrarMensagem(string.Format("Torre vendida! +{0} moedas", reembolso));
            }

            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            Destroy(gameObject);
        }

        /// <summary>Retorna o nível atual da torre.</summary>
        public int ObterNivel() { return _nivel; }

        /// <summary>Retorna o dano atual da torre.</summary>
        public float ObterDano() { return dano; }

        /// <summary>Retorna o alcance atual da torre.</summary>
        public float ObterAlcance() { return alcance; }

        /// <summary>Retorna a velocidade de ataque atual.</summary>
        public float ObterVelocidadeAtaque() { return velocidadeAtaque; }

        /// <summary>Retorna o custo de melhoria atual.</summary>
        public int ObterCustoMelhoria() { return custoMelhoria; }

        /// <summary>Retorna a ConfiguracaoTorre associada.</summary>
        public ConfiguracaoTorre ObterConfig() { return configTorre; }

        /// <summary>Inicializa a torre com uma configuração e posição na grade.</summary>
        public void DefinirConfig(ConfiguracaoTorre config, int cx, int cy)
        {
            _celulaOcupada = new Vector2Int(cx, cy);
            configTorre = config;
            if (config != null)
            {
                dano = config.dano;
                alcance = config.alcance;
                velocidadeAtaque = config.velocidadeAtaque;
                custoMelhoria = config.custoMelhoria;

                if (_linhaAtaque != null)
                {
                    _linhaAtaque.startColor = config.corProjetil;
                    _linhaAtaque.endColor = config.corProjetil;
                }
            }

            if (circuloAlcance != null)
            {
                float diametro = alcance * 2f;
                circuloAlcance.transform.localScale = new Vector3(diametro, diametro, 1f);
            }
        }

        /// <summary>Calcula os stats que a torre terá no próximo nível.</summary>
        public void ObterStatsProximoNivel(out float danoProx, out float alcanceProx, out float velProx)
        {
            if (configTorre != null)
            {
                danoProx = dano * configTorre.multiplicadorDanoPorNivel;
                alcanceProx = alcance * configTorre.multiplicadorAlcancePorNivel;
                velProx = velocidadeAtaque * configTorre.multiplicadorVelocidadePorNivel;
            }
            else
            {
                danoProx = dano * 1.5f;
                alcanceProx = alcance * 1.1f;
                velProx = velocidadeAtaque * 1.2f;
            }
        }

        protected virtual void AoDestruir()
        {
            if (_celulaOcupada.x >= 0)
            {
                GradePosicionamento grade = FindFirstObjectByType<GradePosicionamento>();
                if (grade != null)
                    grade.LiberarCelula(_celulaOcupada.x, _celulaOcupada.y);
            }

            if (OnTorreDestruida != null)
                OnTorreDestruida();
        }

        private void OnDestroy()
        {
            AoDestruir();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, alcance);

            if (configTorre != null && configTorre.raioExplosao > 0f && _alvoAtual != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                Gizmos.DrawWireSphere(_alvoAtual.position, configTorre.raioExplosao);
            }
        }
    }
}
