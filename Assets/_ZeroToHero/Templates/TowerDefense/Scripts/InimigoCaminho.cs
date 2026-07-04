using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Inimigo que segue um caminho de waypoints para Tower Defense.
    /// Ao chegar ao fim do caminho, causa dano à base do jogador e é destruído.
    /// Concede recompensa em moedas ao ser derrotado.
    /// </summary>
    public class InimigoCaminho : EntidadeBase
    {
        [Header("Caminho")]
        [SerializeField, Tooltip("Array de waypoints que definem o caminho (ordem sequencial).")]
        private Transform[] waypoints;

        [SerializeField, Tooltip("Velocidade de movimento.")]
        private float velocidade = 2f;

        [Header("Atributos")]
        [SerializeField, Tooltip("Dano causado à base se chegar ao fim.")]
        private float danoBase = 10f;

        [SerializeField, Tooltip("Recompensa em moedas ao ser derrotado.")]
        private int recompensa = 25;

        private int _indiceWaypoint = 0;
        private float _velocidadeOriginal;
        private float _velocidadeAtual;
        private GameObject _barraVidaGo;
        private SpriteRenderer _srBarraVida;

        /// <summary>Velocidade efetiva atual do inimigo (afetada por slow).</summary>
        public float VelocidadeAtual
        {
            get { return _velocidadeAtual; }
            set { _velocidadeAtual = value; }
        }

        protected override void AoIniciar()
        {
            base.AoIniciar();

            if (vida == null)
            {
                vida = gameObject.AddComponent<ComponenteVida>();
            }

            _velocidadeOriginal = velocidade;
            _velocidadeAtual = velocidade;

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning(string.Format("[InimigoCaminho] Nenhum waypoint configurado em {0}.", gameObject.name));
            }

            if (vida != null)
            {
                vida.onMorte.AddListener(AoMorrerInimigo);
                vida.onDano.AddListener(MostrarDanoFlutuante);
            }

            CriarBarraVida();
        }

        protected override void AoAtualizar()
        {
            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando)
            {
                return;
            }

            if (waypoints == null || waypoints.Length == 0 || _indiceWaypoint >= waypoints.Length) return;

            AtualizarBarraVida();
            SeguirCaminho();
        }

        /// <summary>Move o inimigo em direção ao próximo waypoint.</summary>
        private void SeguirCaminho()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Transform alvo = waypoints[_indiceWaypoint];
            Vector2 direcao = (alvo.position - transform.position).normalized;
            transform.Translate(direcao * _velocidadeAtual * Time.deltaTime);

            if (Vector2.Distance(transform.position, alvo.position) < 0.1f)
            {
                _indiceWaypoint++;

                if (_indiceWaypoint >= waypoints.Length)
                {
                    ChegarAoFim();
                }
            }
        }

        /// <summary>Causa dano à base e destrói o inimigo.</summary>
        private void ChegarAoFim()
        {
            Base baseJogador = FindFirstObjectByType<Base>();
            if (baseJogador != null)
            {
                baseJogador.ReceberDanoBase(danoBase);
            }

            if (vida != null)
            {
                vida.ReceberDano(vida.VidaMaxima);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Concede recompensa em moedas ao morrer.</summary>
        private void AoMorrerInimigo()
        {
            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.AdicionarMoedas(recompensa);
            }

            MostrarMoedaFlutuante();

            if (efeito != null)
            {
                efeito.EmitirParticula(transform.position);
            }
        }

        private void MostrarDanoFlutuante(float dano)
        {
            TextoDanoFlutuante tdf = ObterComponenteDanoFlutuante();
            if (tdf != null)
                tdf.MostrarDano(dano);
        }

        /// <summary>Aplica dano ao longo do tempo e exibe texto DOT flutuante.</summary>
        public void AplicarDanoDOT(float dano)
        {
            ComponenteVida vidaComp = GetComponent<ComponenteVida>();
            if (vidaComp != null)
                vidaComp.ReceberDano(dano);

            TextoDanoFlutuante tdf = ObterComponenteDanoFlutuante();
            if (tdf != null)
                tdf.MostrarDOT(dano);
        }

        /// <summary>Exibe texto de debuff flutuante.</summary>
        public void MostrarDebuffTexto(string texto)
        {
            TextoDanoFlutuante tdf = ObterComponenteDanoFlutuante();
            if (tdf != null)
                tdf.MostrarDebuff(texto);
        }

        /// <summary>Remove o texto de debuff flutuante.</summary>
        public void RestaurarDebuffTexto()
        {
            TextoDanoFlutuante tdf = ObterComponenteDanoFlutuante();
            if (tdf != null)
                tdf.MostrarDebuff("");
        }

        private void MostrarMoedaFlutuante()
        {
            TextoDanoFlutuante tdf = ObterComponenteDanoFlutuante();
            if (tdf != null)
            {
                tdf.MostrarMoeda(recompensa);
            }

            Transform canvasTr = transform.Find("Canvas_Dano");
            if (canvasTr != null)
            {
                canvasTr.SetParent(null);
                UnityEngine.Object.DontDestroyOnLoad(canvasTr.gameObject);
                Destroy(canvasTr.gameObject, 2f);
            }
        }

        private TextoDanoFlutuante ObterComponenteDanoFlutuante()
        {
            Transform canvasTr = transform.Find("Canvas_Dano");
            if (canvasTr == null) return null;
            return canvasTr.GetComponent<TextoDanoFlutuante>();
        }

        /// <summary>Aplica lentidão ao inimigo, reduzindo sua velocidade.</summary>
        public void AplicarSlow(float intensidade)
        {
            _velocidadeAtual = _velocidadeOriginal * (1f - intensidade);
            MostrarDebuffTexto(string.Format("-{0:F0}% MV", intensidade * 100f));
        }

        /// <summary>Restaura a velocidade original do inimigo.</summary>
        public void RestaurarVelocidade()
        {
            _velocidadeAtual = _velocidadeOriginal;
        }

        /// <summary>Define os waypoints que o inimigo deve seguir.</summary>
        public void DefinirWaypoints(Transform[] novosWaypoints)
        {
            waypoints = novosWaypoints;
            _indiceWaypoint = 0;
        }

        private void CriarBarraVida()
        {
            _barraVidaGo = BuscarFilho("BarraVida");
            if (_barraVidaGo == null) return;

            Transform fundoTr = _barraVidaGo.transform.Find("Fundo");

            Transform barraTr = _barraVidaGo.transform.Find("Barra");
            if (barraTr != null)
                _srBarraVida = barraTr.GetComponent<SpriteRenderer>();
        }

        private GameObject BuscarFilho(string nome)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == nome)
                    return transform.GetChild(i).gameObject;
            }
            return null;
        }

        private void AtualizarBarraVida()
        {
            if (_barraVidaGo == null || vida == null || vida.VidaMaxima <= 0f) return;

            float pct = vida.VidaAtual / vida.VidaMaxima;
            if (pct < 0f) pct = 0f;
            if (pct > 1f) pct = 1f;

            Transform barraTr = _barraVidaGo.transform.Find("Barra");
            if (barraTr == null) return;

            Vector3 escalaAtual = barraTr.localScale;
            barraTr.localScale = new Vector3(pct, escalaAtual.y, escalaAtual.z);

            if (_srBarraVida != null)
            {
                if (pct > 0.5f)
                    _srBarraVida.color = Color.green;
                else if (pct > 0.25f)
                    _srBarraVida.color = Color.yellow;
                else
                    _srBarraVida.color = Color.red;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.red;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
    }
}
