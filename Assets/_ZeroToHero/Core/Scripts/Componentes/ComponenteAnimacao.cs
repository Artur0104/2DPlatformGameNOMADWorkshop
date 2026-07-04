using UnityEngine;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de animação simplificado.
    /// Fornece métodos para tocar animações e definir parâmetros do Animator.
    /// Se não houver Animator no GameObject, os métodos são ignorados silenciosamente
    /// (não gera erro, apenas loga um aviso).
    /// </summary>
    public class ComponenteAnimacao : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Animator usado pelo componente. Se vazio, usa o do GameObject.")]
        private Animator animador;

        [Header("Parâmetros Padrão")]
        [SerializeField, Tooltip("Nome do parâmetro de velocidade horizontal no Animator.")]
        private string parametroVelocidade = "Velocidade";

        [SerializeField, Tooltip("Nome do parâmetro 'NoChão' no Animator.")]
        private string parametroNoChao = "NoChao";

        [SerializeField, Tooltip("Nome do parâmetro 'Atacando' no Animator.")]
        private string parametroAtacando = "Atacando";

        [SerializeField, Tooltip("Nome do parâmetro 'Pulando' no Animator.")]
        private string parametroPulando = "Pulando";

        private void Awake()
        {
            if (animador == null)
            {
                animador = GetComponent<Animator>();
            }

            if (animador == null)
            {
                Debug.LogWarning($"[ComponenteAnimacao] Nenhum Animator encontrado em {gameObject.name}. Animações serão ignoradas.");
            }
        }

        /// <summary>
        /// Toca uma animação pelo nome do estado no Animator.
        /// Se não houver Animator, nada acontece.
        /// </summary>
        /// <param name="nomeAnimacao">Nome do estado da animação.</param>
        public void Tocar(string nomeAnimacao)
        {
            if (animador == null) return;

            animador.Play(nomeAnimacao);
        }

        /// <summary>
        /// Faz uma transição suave para uma animação.
        /// Se não houver Animator, nada acontece.
        /// </summary>
        /// <param name="nomeAnimacao">Nome do estado da animação.</param>
        /// <param name="tempoTransicao">Tempo da transição em segundos.</param>
        public void Transicao(string nomeAnimacao, float tempoTransicao = 0.1f)
        {
            if (animador == null) return;

            animador.CrossFade(nomeAnimacao, tempoTransicao);
        }

        /// <summary>
        /// Define um parâmetro float no Animator.
        /// </summary>
        public void DefinirFloat(string nomeParametro, float valor)
        {
            if (animador == null) return;
            animador.SetFloat(nomeParametro, valor);
        }

        /// <summary>
        /// Define um parâmetro bool no Animator.
        /// </summary>
        public void DefinirBool(string nomeParametro, bool valor)
        {
            if (animador == null) return;
            animador.SetBool(nomeParametro, valor);
        }

        /// <summary>
        /// Dispara um trigger no Animator.
        /// </summary>
        public void Disparar(string nomeTrigger)
        {
            if (animador == null) return;
            animador.SetTrigger(nomeTrigger);
        }

        /// <summary>
        /// Atualiza os parâmetros padrão do Animator baseado no estado do movimento.
        /// Normalmente chamado a partir do EntidadeBase ou JogadorPlataforma.
        /// </summary>
        /// <param name="velocidade">Velocidade horizontal.</param>
        /// <param name="noChao">Se a entidade está no chão.</param>
        /// <param name="atacando">Se está atacando.</param>
        /// <param name="pulando">Se está pulando.</param>
        public void AtualizarParametros(float velocidade, bool noChao, bool atacando, bool pulando)
        {
            if (animador == null) return;

            animador.SetFloat(parametroVelocidade, Mathf.Abs(velocidade));
            animador.SetBool(parametroNoChao, noChao);
            animador.SetBool(parametroAtacando, atacando);
            animador.SetBool(parametroPulando, pulando);
        }
    }
}
