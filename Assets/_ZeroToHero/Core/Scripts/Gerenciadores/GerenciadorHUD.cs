using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Core.Gerenciadores
{
    /// <summary>
    /// Gerenciador de HUD (Singleton).
    /// Controla a exibição de vida, pontuação, moedas, ondas e mensagens na tela.
    /// Acesse de qualquer lugar: GerenciadorHUD.Instancia
    /// </summary>
    public class GerenciadorHUD : GerenciadorBase<GerenciadorHUD>
    {
        [Header("Vida")]
        [SerializeField, Tooltip("Texto que exibe a vida atual (ex: 'HP: 100/100').")]
        private TMP_Text textoVida;

        [SerializeField, Tooltip("Slider/barra de vida.")]
        private Slider barraVida;

        [Header("Pontuação")]
        [SerializeField, Tooltip("Texto que exibe a pontuação.")]
        private TMP_Text textoPontuacao;

        [SerializeField, Tooltip("Texto que exibe as moedas.")]
        private TMP_Text textoMoedas;

        [Header("Ondas (Tower Defense)")]
        [SerializeField, Tooltip("Texto que exibe a onda atual.")]
        private TMP_Text textoOnda;

        [SerializeField, Tooltip("Texto que exibe os inimigos restantes na onda.")]
        private TMP_Text textoInimigosRestantes;

        [SerializeField, Tooltip("Texto que exibe o timer para a próxima onda.")]
        private TMP_Text textoTempoOnda;

        [Header("Telas")]
        [SerializeField, Tooltip("Painel de vitória (ativado ao completar a fase).")]
        private GameObject painelVitoria;

        [SerializeField, Tooltip("Painel de Game Over (ativado ao morrer sem checkpoint).")]
        private GameObject painelGameOver;

        [Header("Mensagens")]
        [SerializeField, Tooltip("Texto de mensagem temporária.")]
        private TMP_Text textoMensagem;

        [SerializeField, Tooltip("Duração da mensagem temporária em segundos.")]
        private float duracaoMensagem = 2f;

        [Header("Timer")]
        [SerializeField, Tooltip("Texto que exibe o tempo decorrido.")]
        private TMP_Text textoTempo;

        private int _pontuacao = 0;
        private int _moedas = 0;
        private float _timerMensagem = 0f;
        
        private System.Type _tipoOndas;
        private Component _ondasRef;
private float _tempoDecorrido = 0f;

        private void Start()
        {
            _tipoOndas = System.Type.GetType("ZeroToHero.Cenas.TowerDefense.GerenciadorOndas, Assembly-CSharp");
            if (_tipoOndas != null)
                _ondasRef = FindAnyObjectByType(_tipoOndas) as Component;

            if (GerenciadorJogo.Instancia != null)
            {
                GerenciadorJogo.Instancia.onGameOver.AddListener(MostrarTelaGameOver);
                GerenciadorJogo.Instancia.onVitoria.AddListener(MostrarTelaVitoria);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            BuscarReferenciasUI();
        }

        private void BuscarReferenciasUI()
        {
            var hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas == null) return;

            var vidaTransform = hudCanvas.transform.Find("TextoVida");
            if (vidaTransform != null) textoVida = vidaTransform.GetComponent<TMP_Text>();

            var mdTransform = hudCanvas.transform.Find("TextoMoedas");
            if (mdTransform != null) textoMoedas = mdTransform.GetComponent<TMP_Text>();

            var msgTransform = hudCanvas.transform.Find("TextoMensagem");
            if (msgTransform != null) textoMensagem = msgTransform.GetComponent<TMP_Text>();

            var tempoTransform = hudCanvas.transform.Find("TextoTempo");
            if (tempoTransform != null) textoTempo = tempoTransform.GetComponent<TMP_Text>();

            var inimigosTransform = hudCanvas.transform.Find("TextoInimigosRestantes");
            if (inimigosTransform != null) textoInimigosRestantes = inimigosTransform.GetComponent<TMP_Text>();

            var tempoOndaTransform = hudCanvas.transform.Find("TextoTempoOnda");
            if (tempoOndaTransform != null) textoTempoOnda = tempoOndaTransform.GetComponent<TMP_Text>();
        }

        public void AtualizarVida(float atual, float maxima)
        {
            if (textoVida != null)
            {
                textoVida.text = string.Format("HP: {0:F0} / {1:F0}", atual, maxima);
            }

            if (barraVida != null && maxima > 0f)
            {
                barraVida.value = atual / maxima;
            }
        }

        public void AtualizarPontuacao(int valor)
        {
            _pontuacao = valor;
            if (textoPontuacao != null)
            {
                textoPontuacao.text = string.Format("Pontos: {0}", _pontuacao);
            }
        }

        public void AdicionarMoedas(int quantidade)
        {
            _moedas += quantidade;
            if (textoMoedas != null)
            {
                textoMoedas.text = string.Format("Moedas: {0}", _moedas);
            }
        }

        public bool GastarMoedas(int quantidade)
        {
            if (_moedas < quantidade) return false;
            _moedas -= quantidade;
            if (textoMoedas != null)
            {
                textoMoedas.text = string.Format("Moedas: {0}", _moedas);
            }
            return true;
        }

        public int GetMoedas()
        {
            return _moedas;
        }

        public void AtualizarOnda(int ondaAtual, int totalOndas)
        {
            if (textoOnda != null)
            {
                textoOnda.text = string.Format("Onda: {0} / {1}", ondaAtual, totalOndas);
            }
        }

        public void MostrarMensagem(string mensagem)
        {
            if (textoMensagem != null)
            {
                textoMensagem.text = mensagem;
                textoMensagem.enabled = true;
                _timerMensagem = duracaoMensagem;
            }
            else
            {
                Debug.Log(string.Format("[HUD] {0}", mensagem));
            }
        }

        public void MostrarMensagem(string mensagem, float duracao)
        {
            duracaoMensagem = duracao;
            MostrarMensagem(mensagem);
        }

        private void Update()
        {
            if (_timerMensagem > 0f)
            {
                _timerMensagem -= Time.deltaTime;
                if (_timerMensagem <= 0f && textoMensagem != null)
                {
                    textoMensagem.enabled = false;
                }
            }

            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.Jogando)
            {
                _tempoDecorrido += Time.deltaTime;
                if (textoTempo != null)
                {
                    int minutos = (int)(_tempoDecorrido / 60f);
                    textoTempo.text = string.Format("{0:00}:{1:00}", minutos, (int)(_tempoDecorrido % 60f));
                }

                AtualizarTextosOnda();
            }
        }

        private void AtualizarTextosOnda()
        {
            if (_ondasRef == null || _tipoOndas == null) return;

            var propInimigos = _tipoOndas.GetProperty("InimigosRestantes");
            if (textoInimigosRestantes != null && propInimigos != null)
            {
                int inimigos = (int)propInimigos.GetValue(_ondasRef, null);
                textoInimigosRestantes.text = string.Format("Inimigos: {0}", inimigos);
            }

            var propTimer = _tipoOndas.GetProperty("TimerProximaOnda");
            if (textoTempoOnda != null && propTimer != null)
            {
                float tempo = (float)propTimer.GetValue(_ondasRef, null);
                if (tempo > 0f)
                {
                    textoTempoOnda.text = string.Format("Proxima onda: {0:F0}s", tempo);
                    textoTempoOnda.enabled = true;
                }
                else
                {
                    textoTempoOnda.enabled = false;
                }
            }
        }

        public void MostrarTelaVitoria()
        {
            if (painelVitoria == null)
                painelVitoria = BuscarNaCena("HUD_Vitoria");

            if (painelVitoria != null)
            {
                VincularBotoesFimDeJogo(painelVitoria);
                painelVitoria.SetActive(true);
            }
        }

        public void MostrarTelaGameOver()
        {
            if (painelGameOver == null)
                painelGameOver = BuscarNaCena("HUD_GameOver");

            if (painelGameOver != null)
            {
                VincularBotoesFimDeJogo(painelGameOver);
                painelGameOver.SetActive(true);
            }
        }

        private void VincularBotoesFimDeJogo(GameObject painel)
        {
            if (painel == null || GerenciadorCena.Instancia == null) return;

            Transform btnReiniciar = painel.transform.Find("Btn_Reiniciar");
            if (btnReiniciar != null)
            {
                Button btn = btnReiniciar.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(GerenciadorCena.Instancia.RecarregarCena);
                }
            }

            Transform btnMenu = painel.transform.Find("Btn_Menu");
            if (btnMenu != null)
            {
                Button btn = btnMenu.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(delegate { GerenciadorCena.Instancia.CarregarCena("MenuPrincipal"); });
                }
            }
        }

        private GameObject BuscarNaCena(string nome)
        {
            var raizes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < raizes.Length; i++)
            {
                Transform t = BuscarRecursivo(raizes[i].transform, nome);
                if (t != null) return t.gameObject;
            }
            return null;
        }

        private Transform BuscarRecursivo(Transform pai, string nome)
        {
            if (pai.name == nome) return pai;
            for (int i = 0; i < pai.childCount; i++)
            {
                Transform t = BuscarRecursivo(pai.GetChild(i), nome);
                if (t != null) return t;
            }
            return null;
        }
    }
}
