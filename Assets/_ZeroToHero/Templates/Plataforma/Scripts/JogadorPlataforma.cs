using System.Collections;
using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Ferramentas;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Controlador do jogador para o template de Plataforma.
    /// Processa input, gerencia movimento, pulo, dash e interage com checkpoints.
    /// Herda de EntidadeBase e utiliza os componentes Core plugados automaticamente.
    /// </summary>
    public class JogadorPlataforma : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject com a configuração de movimento.")]
        private ConfiguracaoMovimento configMovimento;

        [SerializeField, Tooltip("Tempo de invencibilidade após respawn (segundos).")]
        private float tempoInvencibilidadeRespawn = 1.5f;

        [Header("Game Feel")]
        [SerializeField, Tooltip("Dead zone para detecção de direção.")]
        private float deadZoneDirecao = 0.01f;

        [SerializeField, Tooltip("Escala X do squash no ar (estica).")]
        private float squashArX = 1.2f;
        [SerializeField, Tooltip("Escala Y do squash no ar (comprime).")]
        private float squashArY = 0.8f;
        [SerializeField, Tooltip("Escala X do squash no pouso (comprime).")]
        private float squashPousoX = 0.8f;
        [SerializeField, Tooltip("Escala Y do squash no pouso (estica).")]
        private float squashPousoY = 1.2f;
        [SerializeField, Tooltip("Velocidade de interpolação do squash/stretch.")]
        private float velocidadeSquash = 15f;

        [SerializeField, Tooltip("Offset Y da partícula de poeira ao pousar.")]
        private float offsetPoeira = 0.5f;

        [Header("Morte")]
        [SerializeField, Tooltip("Duração do fade out ao morrer (segundos).")]
        private float duracaoFadeMorte = 0.5f;
        [SerializeField, Tooltip("Tempo total até decisão de respawn/GameOver (segundos).")]
        private float duracaoTotalMorte = 1f;

        [Header("Dano")]
        [SerializeField, Tooltip("Duração do piscar ao receber dano (segundos).")]
        private float duracaoPiscarDano = 0.5f;
        [SerializeField, Tooltip("Intervalo entre piscadas (segundos).")]
        private float intervaloPiscarDano = 0.08f;

        private Vector3 _posicaoCheckpoint;
        private bool _temCheckpoint = false;

        // Estado de morte (timer-based, substitui coroutine que era interrompida por Destroy)
        private bool _estaMorto = false;
        private float _timerMorte = 0f;
        private Color _corOriginalSprite;
        private bool _morteProcessada = false;

        // Edge detection do dash (um dash por pressionamento)
        private bool _dashDisparado = false;

        // Game feel
        private TrailRenderer _rastro;
        private bool _estavaNoChao;
        private int _direcaoVisual = 1;

        // Camera
        private CameraSuave _cameraSuave;

        /// <inheritdoc/>
        protected override void AoDespertar()
        {
            base.AoDespertar();

            if (vida == null)
            {
                vida = gameObject.AddComponent<ComponenteVida>();
            }

            if (movimento == null)
            {
                movimento = gameObject.AddComponent<ComponenteMovimento>();
            }

            _posicaoCheckpoint = transform.position;

            _rastro = GetComponent<TrailRenderer>();
            if (_rastro != null)
                _rastro.enabled = false;

            _cameraSuave = Camera.main != null ? Camera.main.GetComponent<CameraSuave>() : null;
        }

        /// <inheritdoc/>
        protected override void AoIniciar()
        {
            base.AoIniciar();

            if (vida != null)
            {
                vida.onMorte.AddListener(AoMorrer);
                vida.onDano.AddListener(AoReceberDano);

                // Impedir que ComponenteVida destrua o GameObject ao morrer
                // O JogadorPlataforma gerencia a morte/respawn via Update
                vida.DestruirAoMorrer = false;
                _corOriginalSprite = sr != null ? sr.color : Color.white;
            }

            if (movimento != null && configMovimento != null)
            {
                movimento.AplicarConfiguracao(configMovimento);
            }

            if (GerenciadorJogo.Instancia != null)
            {
                GerenciadorJogo.Instancia.IniciarJogo();
            }

            if (InputReader.Instancia != null)
            {
                InputReader.Instancia.HabilitarJogador();
            }

            if (GerenciadorHUD.Instancia != null && vida != null)
            {
                GerenciadorHUD.Instancia.AtualizarVida(vida.VidaAtual, vida.VidaMaxima);
            }
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            ProcessarMorte();

            if (_estaMorto) return;

            if (InputReader.Instancia != null)
                InputReader.Instancia.AtualizarInput();

            // Pausa / Retoma (Esc) — chave mestra, sobrepoe o overlay
            if (InputReader.Instancia != null && InputReader.Instancia.PausaPressionada())
            {
                if (GerenciadorJogo.Instancia != null &&
                    GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.Pausado)
                {
                    // Se overlay estava aberto atras do pause, fecha-o
                    var painel = FindFirstObjectByType<PainelAjusteBase>();
                    if (painel != null && painel.OverlayAberto)
                        painel.FecharOverlay();

                    GerenciadorJogo.Instancia.Retomar();
                }
                else if (GerenciadorJogo.Instancia != null &&
                         GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.Jogando)
                {
                    PainelAjusteBase.PausadoPeloOverlay = false;
                    GerenciadorJogo.Instancia.Pausar();
                }
            }

            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando)
            {
                return;
            }

            ProcessarInput();
            AtualizarGameFeel();
        }

        /// <summary>
        /// Lê o input do InputReader e aplica no ComponenteMovimento.
        /// Sobrescreva para adicionar inputs customizados.
        /// </summary>
        private void ProcessarInput()
        {
            if (InputReader.Instancia == null) return;

            float direcao = InputReader.Instancia.LerMovimento().x;
            movimento.Mover(new Vector2(direcao, 0f));

            if (InputReader.Instancia.Pular)
            {
                movimento.Pular();
            }

            bool correrSegurado = InputReader.Instancia.Correr;
            if (correrSegurado && !_dashDisparado)
            {
                movimento.Dash();
                _dashDisparado = true;
            }
            else if (!correrSegurado)
            {
                _dashDisparado = false;
            }

            if (InputReader.Instancia.Atacar && ataque != null)
            {
                ataque.Atacar();
            }

            // Direção visual é aplicada no AtualizarGameFeel (junto com squash)
            if (direcao > deadZoneDirecao) _direcaoVisual = 1;
            else if (direcao < -deadZoneDirecao) _direcaoVisual = -1;
        }

        private void AtualizarGameFeel()
        {
            if (movimento == null) return;

            bool estaNoChao = movimento.EstaNoChao;
            bool acabouDePousar = estaNoChao && !_estavaNoChao;

            // ── Squash & Stretch ──
            Vector3 escalaAlvo = Vector3.one;

            if (!estaNoChao)
                escalaAlvo = new Vector3(squashArX, squashArY, 1f);
            else if (acabouDePousar)
                escalaAlvo = new Vector3(squashPousoX, squashPousoY, 1f);
            // else: normal (1,1,1)

            // X: sinal muda instantaneamente (flip), magnitude tem Lerp suave
            float novaEscalaX = _direcaoVisual * Mathf.Lerp(
                Mathf.Abs(transform.localScale.x), escalaAlvo.x, Time.deltaTime * velocidadeSquash);

            float novaEscalaY = Mathf.Lerp(
                transform.localScale.y, escalaAlvo.y, Time.deltaTime * velocidadeSquash);

            transform.localScale = new Vector3(novaEscalaX, novaEscalaY, 1f);

            // ── Poeira ao pousar ──
            if (acabouDePousar && efeito != null)
            {
                efeito.EmitirParticula(
                    new Vector2(transform.position.x, transform.position.y - offsetPoeira));
            }

            // ── Trail do dash ──
            if (_rastro != null)
            {
                _rastro.enabled = movimento.EstaNoDash;
            }

            _estavaNoChao = estaNoChao;
        }

        private void ProcessarMorte()
        {
            if (!_estaMorto) return;

            _timerMorte += Time.unscaledDeltaTime;

            // Fade out (0s → 0.5s)
            if (_timerMorte <= duracaoFadeMorte && sr != null)
            {
                float a = 1f - (_timerMorte / duracaoFadeMorte);
                sr.color = new Color(_corOriginalSprite.r, _corOriginalSprite.g, _corOriginalSprite.b, a);
            }

            // Decisão final (após 1s total)
            if (_timerMorte >= duracaoTotalMorte && !_morteProcessada)
            {
                _morteProcessada = true;

                if (_temCheckpoint)
                {
                    Respawnar();
                    _estaMorto = false;
                    _morteProcessada = false;
                    _timerMorte = 0f;
                    // Fade in imediato
                    if (sr != null)
                        sr.color = new Color(_corOriginalSprite.r, _corOriginalSprite.g, _corOriginalSprite.b, 1f);
                }
                else
                {
                    if (GerenciadorJogo.Instancia != null)
                        GerenciadorJogo.Instancia.FinalizarJogo();

                    if (GerenciadorHUD.Instancia != null)
                        GerenciadorHUD.Instancia.MostrarTelaGameOver();
                }
            }
        }

        /// <summary>
        /// Chamado quando o jogador morre (via evento OnMorte do ComponenteVida).
        /// </summary>
        private void AoMorrer()
        {
            if (_estaMorto) return;
            _estaMorto = true;
            _timerMorte = 0f;
            _morteProcessada = false;

            // Zerar input para evitar movimento residual
            if (movimento != null)
                movimento.Parar();

            if (sr != null)
                _corOriginalSprite = sr.color;

            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (col != null)
                col.enabled = false;

            if (_cameraSuave != null)
                _cameraSuave.ResetarSuavidade();
        }

        /// <summary>
        /// Executa o respawn no último checkpoint.
        /// </summary>
        private void Respawnar()
        {
            transform.position = _posicaoCheckpoint;

            if (vida != null)
            {
                vida.Reviver(vida.VidaMaxima);
                vida.Invencivel = true;
                Invoke(nameof(RemoverInvencibilidade), tempoInvencibilidadeRespawn);
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            if (col != null)
            {
                col.enabled = true;
            }

            if (_cameraSuave != null)
                _cameraSuave.ResetarSuavidade();

            if (GerenciadorHUD.Instancia != null && vida != null)
            {
                GerenciadorHUD.Instancia.AtualizarVida(vida.VidaAtual, vida.VidaMaxima);
                GerenciadorHUD.Instancia.MostrarMensagem("Checkpoint!");
            }
        }

        private void RemoverInvencibilidade()
        {
            if (vida != null)
            {
                vida.Invencivel = false;
            }
        }

        /// <summary>
        /// Chamado quando o jogador recebe dano (via evento OnDano do ComponenteVida).
        /// </summary>
        private void AoReceberDano(float dano)
        {
            if (efeito != null)
            {
                efeito.TocarSomDano();
                efeito.VibrarCamera();
            }

            if (GerenciadorHUD.Instancia != null && vida != null)
            {
                GerenciadorHUD.Instancia.AtualizarVida(vida.VidaAtual, vida.VidaMaxima);
            }

            StartCoroutine(CorrotinaPiscarDano());
        }

        private IEnumerator CorrotinaPiscarDano()
        {
            float timer = 0f;

            while (timer < duracaoPiscarDano)
            {
                if (sr != null)
                    sr.enabled = !sr.enabled;

                yield return new WaitForSecondsRealtime(intervaloPiscarDano);
                timer += intervaloPiscarDano;
            }

            if (sr != null)
                sr.enabled = true;
        }

        /// <summary>
        /// Salva a posição de um checkpoint.
        /// Chamado pelo script Checkpoint quando o jogador o toca.
        /// </summary>
        public void SalvarCheckpoint(Vector3 posicao)
        {
            _posicaoCheckpoint = posicao;
            _temCheckpoint = true;

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.MostrarMensagem("Checkpoint salvo!");
            }
        }

        #region Unity Messages

        /// <inheritdoc/>
        public override void Pausar()
        {
            base.Pausar();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        /// <inheritdoc/>
        public override void Retomar()
        {
            base.Retomar();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        #endregion
    }
}
