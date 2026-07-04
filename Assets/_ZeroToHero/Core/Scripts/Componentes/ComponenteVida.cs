using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Interfaces;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de vida para qualquer entidade do jogo.
    /// Gerencia vida atual, dano, cura e morte.
    /// Arraste este componente para qualquer GameObject que precise de vida.
    /// </summary>
    public class ComponenteVida : MonoBehaviour, IDanificavel
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Vida máxima da entidade.")]
        private float vidaMaxima = 100f;

        [SerializeField, Tooltip("Vida atual da entidade.")]
        private float vidaAtual = 100f;

        [SerializeField, Tooltip("Se ativado, a entidade não recebe dano.")]
        private bool invencivel = false;

        [SerializeField, Tooltip("Tempo de invencibilidade após receber dano (em segundos). 0 = sem invencibilidade.")]
        private float tempoInvencibilidade = 0f;

        [SerializeField, Tooltip("Se true, destrói o GameObject ao morrer. Desmarque para controlar externamente.")]
        private bool destruirAoMorrer = true;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando a entidade recebe dano. Parâmetro: dano recebido.")]
        public UnityEvent<float> onDano;

        [SerializeField, Tooltip("Disparado quando a entidade é curada. Parâmetro: quantidade curada.")]
        public UnityEvent<float> onCura;

        [SerializeField, Tooltip("Disparado quando a entidade morre.")]
        public UnityEvent onMorte;

        private float _timerInvencibilidade = 0f;
        private bool _estaViva = true;

        public float VidaAtual => vidaAtual;
        public float VidaMaxima => vidaMaxima;
        public bool EstaViva => _estaViva;
        public bool Invencivel { get => invencivel; set => invencivel = value; }
        public bool DestruirAoMorrer { get => destruirAoMorrer; set => destruirAoMorrer = value; }

        private void Awake()
        {
            if (vidaAtual <= 0f || vidaAtual > vidaMaxima)
            {
                vidaAtual = vidaMaxima;
            }
        }

        private void Update()
        {
            if (_timerInvencibilidade > 0f)
            {
                _timerInvencibilidade -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Aplica dano à entidade. O valor é subtraído da vida atual.
        /// Se a vida chegar a 0, Morrer() é chamado automaticamente.
        /// Sobrescreva via evento OnDano para adicionar comportamentos extras
        /// (ex: piscar sprite, knockback, etc.).
        /// </summary>
        /// <param name="dano">Quantidade de dano a ser aplicada.</param>
        public virtual void ReceberDano(float dano)
        {
            if (!_estaViva || invencivel || _timerInvencibilidade > 0f)
            {
                return;
            }

            vidaAtual -= dano;
            onDano?.Invoke(dano);

            if (tempoInvencibilidade > 0f)
            {
                _timerInvencibilidade = tempoInvencibilidade;
            }

            if (vidaAtual <= 0f)
            {
                vidaAtual = 0f;
                Morrer();
            }
        }

        /// <summary>
        /// Recupera vida da entidade. Não ultrapassa a vida máxima.
        /// </summary>
        /// <param name="quantidade">Quantidade de vida a ser recuperada.</param>
        public virtual void Curar(float quantidade)
        {
            if (!_estaViva)
            {
                return;
            }

            float vidaAnterior = vidaAtual;
            vidaAtual = Mathf.Min(vidaAtual + quantidade, vidaMaxima);
            float curado = vidaAtual - vidaAnterior;

            if (curado > 0f)
            {
                onCura?.Invoke(curado);
            }
        }

        /// <summary>
        /// Chamado quando a vida chega a 0.
        /// Padrão: destrói o GameObject.
        /// Conecte o evento OnMorte para adicionar comportamentos
        /// (ex: tocar animação de morte, spawnar partículas, dropar itens).
        /// </summary>
        public virtual void Morrer()
        {
            if (!_estaViva)
            {
                return;
            }

            _estaViva = false;
            onMorte?.Invoke();

            if (destruirAoMorrer)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Revive a entidade com a quantidade de vida especificada.
        /// Útil para sistemas de respawn.
        /// </summary>
        /// <param name="quantidadeVida">Vida com a qual a entidade reviverá.</param>
        public virtual void Reviver(float quantidadeVida)
        {
            _estaViva = true;
            vidaAtual = Mathf.Clamp(quantidadeVida, 1f, vidaMaxima);
            _timerInvencibilidade = 0f;
        }

        /// <summary>
        /// Define a vida máxima e ajusta a vida atual proporcionalmente.
        /// </summary>
        public void DefinirVidaMaxima(float novoMaximo)
        {
            float proporcao = vidaAtual / vidaMaxima;
            vidaMaxima = novoMaximo;
            vidaAtual = vidaMaxima * proporcao;
        }
    }
}
