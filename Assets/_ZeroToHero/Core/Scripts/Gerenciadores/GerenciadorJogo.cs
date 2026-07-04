using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Core.Gerenciadores
{
    /// <summary>
    /// Estados possíveis do jogo.
    /// </summary>
    public enum EstadoJogo
    {
        Menu,
        Jogando,
        Pausado,
        GameOver,
        Vitoria
    }

    /// <summary>
    /// Gerenciador global de estado do jogo (Singleton).
    /// Controla o fluxo: Menu → Jogando ⇄ Pausado → GameOver / Vitória.
    /// Acesse de qualquer lugar: GerenciadorJogo.Instancia
    /// </summary>
    public class GerenciadorJogo : GerenciadorBase<GerenciadorJogo>
    {
        [Header("Estado Atual")]
        [SerializeField, Tooltip("Estado atual do jogo.")]
        private EstadoJogo estadoAtual = EstadoJogo.Menu;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o estado do jogo muda. Parâmetro: novo estado.")]
        private UnityEvent<EstadoJogo> onEstadoMudou;

        [SerializeField, Tooltip("Disparado quando o jogo é pausado.")]
        public UnityEvent onJogoPausado;

        [SerializeField, Tooltip("Disparado quando o jogo é retomado da pausa.")]
        public UnityEvent onJogoRetomado;

        [SerializeField, Tooltip("Disparado quando o jogo termina (Game Over).")]
        public UnityEvent onGameOver;

        [SerializeField, Tooltip("Disparado quando o jogador vence.")]
        public UnityEvent onVitoria;

        public EstadoJogo EstadoAtual
        {
            get { return estadoAtual; }
        }

        /// <summary>
        /// Muda o estado do jogo. Dispara os eventos correspondentes.
        /// </summary>
        /// <param name="novoEstado">Novo estado do jogo.</param>
        public void MudarEstado(EstadoJogo novoEstado)
        {
            if (estadoAtual == novoEstado) return;

            EstadoJogo estadoAnterior = estadoAtual;
            estadoAtual = novoEstado;

            onEstadoMudou.Invoke(novoEstado);

            switch (novoEstado)
            {
                case EstadoJogo.Pausado:
                    onJogoPausado.Invoke();
                    Time.timeScale = 0f;
                    break;

                case EstadoJogo.GameOver:
                    Time.timeScale = 0f;
                    onGameOver.Invoke();
                    break;

                case EstadoJogo.Vitoria:
                    Time.timeScale = 0f;
                    onVitoria.Invoke();
                    break;

                case EstadoJogo.Jogando:
                    if (estadoAnterior == EstadoJogo.Pausado)
                    {
                        onJogoRetomado.Invoke();
                        Time.timeScale = 1f;
                    }
                    break;
            }
        }

        /// <summary>
        /// Pausa ou retoma o jogo. Alterna entre Jogando e Pausado.
        /// </summary>
        public void Pausar()
        {
            if (estadoAtual == EstadoJogo.Jogando)
            {
                MudarEstado(EstadoJogo.Pausado);
            }
        }

        /// <summary>
        /// Retoma o jogo da pausa.
        /// </summary>
        public void Retomar()
        {
            if (estadoAtual == EstadoJogo.Pausado)
            {
                MudarEstado(EstadoJogo.Jogando);
            }
        }

        /// <summary>
        /// Finaliza o jogo como Game Over.
        /// </summary>
        public void FinalizarJogo()
        {
            MudarEstado(EstadoJogo.GameOver);
        }

        /// <summary>
        /// Finaliza o jogo como Vitória.
        /// </summary>
        public void VitoriaJogo()
        {
            MudarEstado(EstadoJogo.Vitoria);
        }

        /// <summary>
        /// Reinicia o estado para Jogando (util ao recarregar cena).
        /// </summary>
        public void IniciarJogo()
        {
            Time.timeScale = 1f;
            MudarEstado(EstadoJogo.Jogando);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (estadoAtual == EstadoJogo.GameOver || estadoAtual == EstadoJogo.Vitoria)
                    return;

                if (estadoAtual == EstadoJogo.Jogando)
                {
                    Pausar();
                }
                else if (estadoAtual == EstadoJogo.Pausado)
                {
                    Retomar();
                }
            }
        }
}
}
