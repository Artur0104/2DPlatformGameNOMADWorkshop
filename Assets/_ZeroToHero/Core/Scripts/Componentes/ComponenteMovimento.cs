using UnityEngine;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de movimento genérico.
    /// Suporta movimento horizontal, pulo, dash e gravidade customizada.
    /// Configurável via ScriptableObject ConfiguracaoMovimento ou diretamente no Inspector.
    /// Funciona com Rigidbody2D (modo física) ou Transform (modo direto).
    /// </summary>
    public class ComponenteMovimento : MonoBehaviour
    {
        [Header("Configuração (via SO ou manual)")]
        [SerializeField, Tooltip("ScriptableObject com a configuração de movimento. Se definido, sobrescreve os campos abaixo.")]
        private ConfiguracaoMovimento config;

        [SerializeField, Tooltip("Velocidade de movimento horizontal.")]
        private float velocidadeAndar = 5f;

        [SerializeField, Tooltip("Força do pulo.")]
        private float forcaPulo = 10f;

        [SerializeField, Tooltip("Gravidade customizada. 0 = usa gravidade padrão da física.")]
        private float gravidade = 0f;

        [SerializeField, Tooltip("Tempo extra (em segundos) que o jogador pode pular após sair de uma plataforma (Coyote Time).")]
        private float tempoCoyote = 0.1f;

        [SerializeField, Tooltip("Tempo (em segundos) em que um pulo pressionado antes de tocar o chão é armazenado (Jump Buffer).")]
        private float bufferPulo = 0.1f;

        [SerializeField, Tooltip("Número máximo de pulos consecutivos (1 = pulo simples, 2 = double jump).")]
        private int pulosMaximos = 1;

        [SerializeField, Tooltip("Duração do dash em segundos.")]
        private float tempoDash = 0.2f;

        [SerializeField, Tooltip("Velocidade do dash.")]
        private float velocidadeDash = 15f;

        [SerializeField, Tooltip("Tempo de espera entre dashes consecutivos (segundos).")]
        private float cooldownDash = 0.5f;

        [SerializeField, Tooltip("Taxa de desaceleracao horizontal quando o jogador solta as teclas. 0=desliza infinito.")]
        private float desaceleracao = 8f;

        [SerializeField, Tooltip("LayerMask para verificar se a entidade está no chão.")]
        private LayerMask camadaChao;

        [SerializeField, Tooltip("Tamanho da caixa de verificação de chão.")]
        private Vector2 tamanhoVerificacaoChao = new Vector2(0.5f, 0.1f);

        [SerializeField, Tooltip("Modo de movimento: Fisica (Rigidbody2D) ou Direto (Transform).")]
        private ModoMovimento modo = ModoMovimento.Fisica;

        public enum ModoMovimento
        {
            Fisica,
            Direto
        }

        // Componentes
        private Rigidbody2D _rb;
        private Collider2D _col;

        // Estado
        private Vector2 _inputMovimento;
        private bool _desejaPular;
        private bool _desejaDash;
        private int _pulosRestantes;
        private float _timerCoyote;
        private float _timerBufferPulo;
        private float _timerDash;
        private float _timerCooldownDash;
        private bool _estaNoDash;
        private bool _estaNoChao;
        private float _direcaoDash;
        private float _velocidadeAtual;

        // Propriedades públicas
        public Vector2 InputMovimento { get => _inputMovimento; set => _inputMovimento = value; }
        public bool EstaNoChao => _estaNoChao;
        public bool EstaNoDash => _estaNoDash;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();

            if (config != null)
            {
                AplicarConfiguracao(config);
            }
        }

        private void Start()
        {
            _pulosRestantes = pulosMaximos;
            _velocidadeAtual = velocidadeAndar;
        }

        private void Update()
        {
            if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando)
            {
                return;
            }

            VerificarChao();
            ProcessarCoyoteTime();
            ProcessarBufferPulo();

            if (_timerCooldownDash > 0f)
                _timerCooldownDash -= Time.deltaTime;

            if (_desejaDash)
            {
                IniciarDash();
            }

            if (_desejaPular && PodePular())
            {
                ExecutarPulo();
            }
        }

        private void FixedUpdate()
        {
            if (_estaNoDash)
            {
                ProcessarDash();
            }
            else
            {
                AplicarMovimentoHorizontal();
                AplicarGravidade();
            }
        }

        #region Verificação de Chão

        private void VerificarChao()
        {
            Vector2 origem = (Vector2)transform.position + Vector2.down * (_col.bounds.extents.y);
            Vector2 tamanho = new Vector2(_col.bounds.size.x * 0.8f, tamanhoVerificacaoChao.y);

            RaycastHit2D hit = Physics2D.BoxCast(origem, tamanho, 0f, Vector2.down, tamanhoVerificacaoChao.y, camadaChao);
            bool estavaNoChao = _estaNoChao;
            _estaNoChao = hit.collider != null;

            if (_estaNoChao && !estavaNoChao)
            {
                _pulosRestantes = pulosMaximos;
            }

            if (_estaNoChao)
            {
                _timerCoyote = tempoCoyote;
            }
        }

        #endregion

        #region Coyote Time e Jump Buffer

        private void ProcessarCoyoteTime()
        {
            if (!_estaNoChao && _timerCoyote > 0f)
            {
                _timerCoyote -= Time.deltaTime;
            }
        }

        private void ProcessarBufferPulo()
        {
            if (_timerBufferPulo > 0f)
            {
                _timerBufferPulo -= Time.deltaTime;

                if (PodePular())
                {
                    ExecutarPulo();
                }
            }
        }

        private bool PodePular()
        {
            if (_estaNoDash) return false;

            if (_estaNoChao)
            {
                return true;
            }

            if (_timerCoyote > 0f && _pulosRestantes == pulosMaximos)
            {
                return true;
            }

            if (_pulosRestantes > 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Movimento

        /// <summary>
        /// Define a direção de movimento horizontal.
        /// Use Vector2.right * Input.GetAxis("Horizontal").
        /// </summary>
        public void Mover(Vector2 direcao)
        {
            _inputMovimento = direcao;
        }

        /// <summary>
        /// Solicita um pulo. O pulo será executado se as condições forem atendidas
        /// (no chão, coyote time ativo, ou pulos restantes).
        /// </summary>
        public void Pular()
        {
            _desejaPular = true;

            if (!PodePular())
            {
                _timerBufferPulo = bufferPulo;
            }
        }

        /// <summary>
        /// Solicita um dash na direção atual do movimento.
        /// </summary>
        public void Dash()
        {
            _desejaDash = true;
        }

        /// <summary>
        /// Para o movimento horizontal imediatamente.
        /// </summary>
        public void Parar()
        {
            _inputMovimento = Vector2.zero;
        }

        private void AplicarMovimentoHorizontal()
        {
            _direcaoDash = Mathf.Sign(_inputMovimento.x);

            if (modo == ModoMovimento.Fisica)
            {
                if (_inputMovimento.x != 0f)
                {
                    _rb.linearVelocity = new Vector2(_inputMovimento.x * _velocidadeAtual, _rb.linearVelocity.y);
                }
                else
                {
                    // Desaceleracao suave no eixo X (nao afeta Y = pulo/queda)
                    float vx = Mathf.Lerp(_rb.linearVelocity.x, 0f, Time.fixedDeltaTime * desaceleracao);
                    if (Mathf.Abs(vx) < 0.01f) vx = 0f;
                    _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);
                }
            }
            else
            {
                transform.Translate(_inputMovimento.x * _velocidadeAtual * Time.deltaTime, 0f, 0f);
            }
        }

        private void AplicarGravidade()
        {
            if (gravidade > 0f && modo == ModoMovimento.Fisica)
            {
                _rb.linearVelocity += Vector2.down * gravidade * Time.fixedDeltaTime;
            }
        }

        private void ExecutarPulo()
        {
            if (_estaNoDash) return;

            if (modo == ModoMovimento.Fisica)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, forcaPulo);
            }
            else
            {
                transform.Translate(0f, forcaPulo * Time.deltaTime, 0f);
            }

            _pulosRestantes--;
            _timerCoyote = 0f;
            _timerBufferPulo = 0f;
            _desejaPular = false;
        }

        #endregion

        #region Dash

        private void IniciarDash()
        {
            if (_estaNoDash) return;
            if (_timerCooldownDash > 0f) return;

            _estaNoDash = true;
            _timerDash = tempoDash;
            _desejaDash = false;

            if (_direcaoDash == 0f)
            {
                _direcaoDash = transform.localScale.x > 0f ? 1f : -1f;
            }
        }

        private void ProcessarDash()
        {
            _timerDash -= Time.fixedDeltaTime;

            if (modo == ModoMovimento.Fisica)
            {
                _rb.linearVelocity = new Vector2(_direcaoDash * velocidadeDash, 0f);
            }
            else
            {
                transform.Translate(_direcaoDash * velocidadeDash * Time.fixedDeltaTime, 0f, 0f);
            }

            if (_timerDash <= 0f)
            {
                _estaNoDash = false;
                _timerCooldownDash = cooldownDash;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            }
        }

        #endregion

        #region Configuração

        /// <summary>
        /// Aplica as configurações de um ScriptableObject ConfiguracaoMovimento.
        /// </summary>
        public void AplicarConfiguracao(ConfiguracaoMovimento novaConfig)
        {
            if (novaConfig == null) return;

            config = novaConfig;
            velocidadeAndar = novaConfig.velocidadeAndar;
            forcaPulo = novaConfig.forcaPulo;
            gravidade = novaConfig.gravidade;
            tempoCoyote = novaConfig.tempoCoyote;
            bufferPulo = novaConfig.bufferPulo;
            pulosMaximos = novaConfig.pulosMaximos;
            tempoDash = novaConfig.tempoDash;
            velocidadeDash = novaConfig.velocidadeDash;
            cooldownDash = novaConfig.cooldownDash;
        }

        /// <summary>
        /// Define a velocidade de movimento horizontal em tempo de execução.
        /// </summary>
        public void DefinirVelocidade(float novaVelocidade)
        {
            _velocidadeAtual = novaVelocidade;
        }

        /// <summary>
        /// Restaura a velocidade original definida na configuração.
        /// </summary>
        public void RestaurarVelocidade()
        {
            _velocidadeAtual = velocidadeAndar;
        }

        #endregion
    }
}
