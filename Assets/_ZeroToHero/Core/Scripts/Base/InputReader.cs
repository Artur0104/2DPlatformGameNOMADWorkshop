using UnityEngine;
using UnityEngine.InputSystem;

namespace ZeroToHero.Core.Base
{
    /// <summary>
    /// Leitor de entrada (Singleton). Encapsula o InputSystem_Actions e
    /// expõe métodos para leitura de input (polling ou eventos).
    /// Adicione este script a um GameObject na cena (ex: GerenciadorJogo)
    /// e referencie o asset InputSystem_Actions no campo.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [Header("Asset de Input")]
        [SerializeField, Tooltip("Arraste o asset InputSystem_Actions aqui.")]
        private InputActionAsset inputAsset;

        // Mapas
        private InputActionMap _mapaJogador;
        private InputActionMap _mapaGlobal;
        private InputActionMap _mapaUI;

        // Ações do jogador
        private InputAction _acaoMover;
        private InputAction _acaoAtacar;
        private InputAction _acaoInteragir;
        private InputAction _acaoPular;
        private InputAction _acaoCorrer;
        private InputAction _acaoHabilidade1;
        private InputAction _acaoHabilidade2;
        private InputAction _acaoHabilidade3;
        private InputAction _acaoHabilidade4;
        private InputAction _acaoFoco;

        // Ação global
        private InputAction _acaoPausar;

        // Estado
        public Vector2 Movimento { get; private set; }
        public bool Atacar { get; private set; }
        public bool Interagir { get; private set; }
        public bool Pular { get; private set; }
        public bool Correr { get; private set; }
        public bool Habilidade1 { get; private set; }
        public bool Habilidade2 { get; private set; }
        public bool Habilidade3 { get; private set; }
        public bool Habilidade4 { get; private set; }
        public bool Foco { get; private set; }
        public bool Pausar { get; private set; }

        private static InputReader _instancia;
        public static InputReader Instancia => _instancia;

        private void Awake()
        {
            if (_instancia != null)
            {
                Destroy(gameObject);
                return;
            }
            _instancia = this;
            DontDestroyOnLoad(gameObject);

            if (inputAsset == null)
            {
                Debug.LogError("[InputReader] Nenhum InputActionAsset atribuído. Arraste o InputSystem_Actions para o campo inputAsset.");
                return;
            }

            _mapaJogador = inputAsset.FindActionMap("Player");
            _mapaGlobal = inputAsset.FindActionMap("Global");
            _mapaUI = inputAsset.FindActionMap("UI");

            if (_mapaJogador == null)
            {
                Debug.LogError("[InputReader] ActionMap 'Player' não encontrado no asset.");
                return;
            }

            VincularAcoes();
        }

        private void VincularAcoes()
        {
            _acaoMover = _mapaJogador.FindAction("Move");
            _acaoAtacar = _mapaJogador.FindAction("Attack");
            _acaoInteragir = _mapaJogador.FindAction("Interact");
            _acaoPular = _mapaJogador.FindAction("Jump");
            _acaoCorrer = _mapaJogador.FindAction("Correr");
            _acaoHabilidade1 = _mapaJogador.FindAction("Habilidade1");
            _acaoHabilidade2 = _mapaJogador.FindAction("Habilidade2");
            _acaoHabilidade3 = _mapaJogador.FindAction("Habilidade3");
            _acaoHabilidade4 = _mapaJogador.FindAction("Habilidade4");
            _acaoFoco = _mapaJogador.FindAction("Foco");

            if (_mapaGlobal != null)
            {
                _acaoPausar = _mapaGlobal.FindAction("Pausar");
            }
        }

        #region Habilitar/Desabilitar Mapas

        private void OnEnable()
        {
            _mapaJogador?.Enable();
            _mapaGlobal?.Enable();
            // UI começa desabilitado; habilitar quando abrir menus
        }

        private void OnDisable()
        {
            _mapaJogador?.Disable();
            _mapaGlobal?.Disable();
            _mapaUI?.Disable();
        }

        /// <summary>
        /// Habilita o mapa de UI (para navegação em menus).
        /// Desabilita os mapas Player e Global.
        /// </summary>
        public void HabilitarUI()
        {
            _mapaJogador?.Disable();
            _mapaGlobal?.Disable();
            _mapaUI?.Enable();
        }

        /// <summary>
        /// Habilita o mapa do jogador e desabilita UI.
        /// </summary>
        public void HabilitarJogador()
        {
            _mapaUI?.Disable();
            _mapaJogador?.Enable();
            _mapaGlobal?.Enable();
        }

        #endregion

        #region Leitura de Input

        /// <summary>
        /// Atualiza todos os valores de input. Chame a cada Update (ou FixedUpdate).
        /// Use este método se preferir polling (checar valores a cada frame).
        /// </summary>
        public void AtualizarInput()
        {
            if (_acaoMover != null)
                Movimento = _acaoMover.ReadValue<Vector2>();

            if (_acaoAtacar != null)
                Atacar = _acaoAtacar.WasPressedThisFrame();

            if (_acaoInteragir != null)
                Interagir = _acaoInteragir.WasPressedThisFrame();

            if (_acaoPular != null)
                Pular = _acaoPular.WasPressedThisFrame();

            if (_acaoCorrer != null)
                Correr = _acaoCorrer.IsPressed();

            if (_acaoHabilidade1 != null)
                Habilidade1 = _acaoHabilidade1.WasPressedThisFrame();

            if (_acaoHabilidade2 != null)
                Habilidade2 = _acaoHabilidade2.WasPressedThisFrame();

            if (_acaoHabilidade3 != null)
                Habilidade3 = _acaoHabilidade3.WasPressedThisFrame();

            if (_acaoHabilidade4 != null)
                Habilidade4 = _acaoHabilidade4.WasPressedThisFrame();

            if (_acaoFoco != null)
                Foco = _acaoFoco.IsPressed();

            if (_acaoPausar != null)
                Pausar = _acaoPausar.WasPressedThisFrame();
        }

        /// <summary>
        /// Limpa os estados de botão após o processamento do frame.
        /// Chame no final do Update se estiver consumindo os inputs.
        /// </summary>
        public void LimparEstadosDeBotao()
        {
            Atacar = false;
            Interagir = false;
            Pular = false;
            Correr = false;
            Habilidade1 = false;
            Habilidade2 = false;
            Habilidade3 = false;
            Habilidade4 = false;
            Pausar = false;
        }

        #endregion

        #region Leitura Direta (sem polling)

        /// <summary>
        /// Retorna o vetor de movimento atual (para leitura direta sem polling).
        /// </summary>
        public Vector2 LerMovimento()
        {
            return _acaoMover?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        /// <summary>
        /// Verifica se a ação de pulo foi pressionada neste frame.
        /// </summary>
        public bool PuloPressionado()
        {
            return _acaoPular?.WasPressedThisFrame() ?? false;
        }

        /// <summary>
        /// Verifica se a ação de ataque foi pressionada neste frame.
        /// </summary>
        public bool AtaquePressionado()
        {
            return _acaoAtacar?.WasPressedThisFrame() ?? false;
        }

        /// <summary>
        /// Verifica se a ação de interagir foi pressionada neste frame.
        /// </summary>
        public bool InteracaoPressionada()
        {
            return _acaoInteragir?.WasPressedThisFrame() ?? false;
        }

        /// <summary>
        /// Verifica se o dash/correr está sendo segurado.
        /// </summary>
        public bool CorrerSegurado()
        {
            return _acaoCorrer?.IsPressed() ?? false;
        }

        /// <summary>
        /// Verifica se uma habilidade específica foi pressionada (índice 0-3).
        /// </summary>
        public bool HabilidadePressionada(int indice)
        {
            return indice switch
            {
                0 => _acaoHabilidade1?.WasPressedThisFrame() ?? false,
                1 => _acaoHabilidade2?.WasPressedThisFrame() ?? false,
                2 => _acaoHabilidade3?.WasPressedThisFrame() ?? false,
                3 => _acaoHabilidade4?.WasPressedThisFrame() ?? false,
                _ => false
            };
        }

        /// <summary>
        /// Verifica se o foco está ativo (segurado).
        /// </summary>
        public bool FocoAtivo()
        {
            return _acaoFoco?.IsPressed() ?? false;
        }

        /// <summary>
        /// Verifica se pausa foi pressionada neste frame.
        /// </summary>
        public bool PausaPressionada()
        {
            return _acaoPausar?.WasPressedThisFrame() ?? false;
        }

        #endregion
    }
}
