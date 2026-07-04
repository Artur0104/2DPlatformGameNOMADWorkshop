using UnityEngine;
using UnityEngine.Events;
using TMPro;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Nave do jogador no Infinite Runner.
    /// Movimento nas 4 direções via InputReader, tiro automático pra cima,
    /// mecânica risk/reward com limite superior que acelera spawn e distância.
    /// </summary>
    public class NaveJogadorRunner : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject com parâmetros da nave.")]
        private ConfiguracaoNaveJogador config;

        [Header("Limites")]
        [SerializeField, Tooltip("Posição Y máxima que o jogador pode atingir.")]
        private float limiteY = 4f;

        [SerializeField, Tooltip("Velocidade base do contador de distância.")]
        private float velocidadeDistancia = 3f;

        [Header("Risk/Reward")]
        [SerializeField, Tooltip("Quanto o fator de aceleração impacta o spawn.")]
        private float multiplicadorSpawnFator = 1f;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o número de vidas muda.")]
        public UnityEvent<int> onVidasAlteradas;

        [SerializeField, Tooltip("Disparado quando a distância percorrida muda.")]
        public UnityEvent<float> onDistanciaAlterada;

        [Header("HUD (opcional)")]
        [SerializeField, Tooltip("Texto de distância no HUD. Se atribuído, atualizado automaticamente.")]
        private TMP_Text textoDistanciaHUD;

        [SerializeField, Tooltip("Texto de vidas no HUD. Se atribuído, atualizado automaticamente.")]
        private TMP_Text textoVidasHUD;

        // Estado interno
        private float _distanciaPercorrida;
        private float _danoAtual;
        private float _velocidadeAtual;
        private bool _invencivelTemporario;

        public int Vidas
        {
            get { return vida != null ? Mathf.FloorToInt(vida.VidaAtual) : 0; }
        }

        public float DistanciaPercorrida
        {
            get { return _distanciaPercorrida; }
        }

        public float FatorAceleracao { get; private set; }

        public float DanoAtual
        {
            get { return _danoAtual; }
            set { _danoAtual = value; }
        }

        public float DanoBase
        {
            get { return config != null ? config.DanoBase : 10f; }
        }

        public float VelocidadeAtual
        {
            get { return _velocidadeAtual; }
            set { _velocidadeAtual = value; }
        }

        public float LimiteY
        {
            get { return limiteY; }
        }

        public float MultiplicadorSpawnFator
        {
            get { return multiplicadorSpawnFator; }
            set { multiplicadorSpawnFator = value; }
        }

        public bool InvencivelTemporario
        {
            get { return _invencivelTemporario; }
            set { _invencivelTemporario = value; }
        }

        public float FrequenciaTiroBoost { get; set; }

        /// <inheritdoc/>
        protected override void AoIniciar()
        {
            if (config == null)
            {
                Debug.LogError("[NaveJogadorRunner] ConfiguracaoNaveJogador não atribuída!");
                return;
            }

            _velocidadeAtual = config.Velocidade;
            _danoAtual = config.DanoBase;
            _distanciaPercorrida = 0f;
            FatorAceleracao = 0f;
            _invencivelTemporario = false;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            if (vida != null)
            {
                vida.DefinirVidaMaxima(config.VidasIniciais);
                vida.Invencivel = false;
                vida.onDano.AddListener(AoReceberDanoDoComponenteVida);
                vida.onMorte.AddListener(AoMorrerPelaVida);
            }

            if (InputReader.Instancia != null)
                InputReader.Instancia.HabilitarJogador();

            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.IniciarJogo();

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("Bordas"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ProjetilJogador"), LayerMask.NameToLayer("Default"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ProjetilJogador"), LayerMask.NameToLayer("Bordas"), true);

            if (onVidasAlteradas != null)
                onVidasAlteradas.Invoke(config.VidasIniciais);

            AtualizarHUDVidas();
        }

        private void AoMorrerPelaVida()
        {
            Morrer();
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            if (config == null) return;

            ProcessarMovimento();
            AtualizarDistancia();
            CalcularFatorAceleracao();
        }

        private void ProcessarMovimento()
        {
            Vector2 input = Vector2.zero;
            if (InputReader.Instancia != null)
                input = InputReader.Instancia.LerMovimento();

            Vector3 movimento = new Vector3(input.x, input.y, 0f) * _velocidadeAtual * Time.deltaTime;

            Vector3 novaPos = transform.position + movimento;
            novaPos.y = Mathf.Clamp(novaPos.y, -limiteY, limiteY);

            transform.position = novaPos;
        }

        private void AtualizarDistancia()
        {
            float incremento = velocidadeDistancia * (1f + FatorAceleracao) * Time.deltaTime;
            _distanciaPercorrida += incremento;

            if (textoDistanciaHUD != null)
                textoDistanciaHUD.text = "Distância: " + Mathf.FloorToInt(_distanciaPercorrida).ToString() + " m";

            if (onDistanciaAlterada != null)
                onDistanciaAlterada.Invoke(_distanciaPercorrida);
        }

        private void CalcularFatorAceleracao()
        {
            if (limiteY <= 0f)
            {
                FatorAceleracao = 0f;
                return;
            }

            FatorAceleracao = Mathf.Clamp01(transform.position.y / limiteY);
        }

        /// <summary>
        /// Aplica dano ao jogador via colisão direta. Delega ao ComponenteVida.
        /// </summary>
        public void ReceberDano()
        {
            if (_invencivelTemporario) return;

            if (vida != null)
                vida.ReceberDano(1f);
        }

        private void AoReceberDanoDoComponenteVida(float dano)
        {
            AtualizarHUDVidas();
        }

        /// <summary>
        /// Cura a vida do jogador via ComponenteVida.
        /// </summary>
        public void Curar(int quantidade)
        {
            if (vida != null)
                vida.Curar(quantidade);

            AtualizarHUDVidas();
        }

        private void AtualizarHUDVidas()
        {
            if (textoVidasHUD != null && vida != null)
                textoVidasHUD.text = "Vidas: " + Mathf.FloorToInt(vida.VidaAtual).ToString();
        }

        private void Morrer()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.FinalizarJogo();

            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            gameObject.SetActive(false);
        }
    }
}
