using UnityEngine;
using ZeroToHero.Core.Componentes;

namespace ZeroToHero.Core.Base
{
    /// <summary>
    /// Classe base para toda entidade do jogo.
    /// Faz cache automático dos componentes comuns (Rigidbody2D, SpriteRenderer, etc.)
    /// e dos componentes plug-and-play do template.
    /// Sobrescreva os métodos virtuais para adicionar comportamentos específicos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EntidadeBase : MonoBehaviour
    {
        [Header("Referências (preenchidas automaticamente)")]
        [SerializeField, Tooltip("Rigidbody2D da entidade.")]
        protected Rigidbody2D rb;

        [SerializeField, Tooltip("SpriteRenderer da entidade.")]
        protected SpriteRenderer sr;

        [SerializeField, Tooltip("Collider2D da entidade.")]
        protected Collider2D col;

        [SerializeField, Tooltip("Animator da entidade (opcional).")]
        protected Animator anim;

        [SerializeField, Tooltip("AudioSource da entidade (opcional).")]
        protected AudioSource audioSrc;

        [Header("Componentes Plugados")]
        [SerializeField, Tooltip("Componente de vida.")]
        protected ComponenteVida vida;

        [SerializeField, Tooltip("Componente de movimento.")]
        protected ComponenteMovimento movimento;

        [SerializeField, Tooltip("Componente de animação.")]
        protected ComponenteAnimacao animacao;

        [SerializeField, Tooltip("Componente de ataque.")]
        protected ComponenteAtaque ataque;

        [SerializeField, Tooltip("Componente de efeitos (partículas, som).")]
        protected ComponenteEfeito efeito;

        [SerializeField, Tooltip("Componentes de habilidade.")]
        protected ComponenteHabilidade[] habilidades;

        protected virtual void AoDespertar()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            anim = GetComponent<Animator>();
            audioSrc = GetComponent<AudioSource>();

            vida = GetComponent<ComponenteVida>();
            movimento = GetComponent<ComponenteMovimento>();
            animacao = GetComponent<ComponenteAnimacao>();
            ataque = GetComponent<ComponenteAtaque>();
            efeito = GetComponent<ComponenteEfeito>();
            habilidades = GetComponents<ComponenteHabilidade>();
        }

        /// <summary>
        /// Chamado ao iniciar a entidade (Unity Start).
        /// Sobrescreva para lógica de inicialização.
        /// </summary>
        protected virtual void AoIniciar() { }

        /// <summary>
        /// Chamado a cada frame (Unity Update).
        /// Sobrescreva para lógica contínua.
        /// </summary>
        protected virtual void AoAtualizar() { }

        /// <summary>
        /// Chamado quando a entidade é habilitada.
        /// Sobrescreva para reconectar eventos, recarregar dados, etc.
        /// </summary>
        protected virtual void AoHabilitar() { }

        /// <summary>
        /// Chamado quando a entidade é desabilitada.
        /// Sobrescreva para desconectar eventos, salvar estado, etc.
        /// </summary>
        protected virtual void AoDesabilitar() { }

        /// <summary>
        /// Chamado quando o jogo é pausado.
        /// Sobrescreva para parar comportamentos que dependem de tempo.
        /// </summary>
        public virtual void Pausar() { }

        /// <summary>
        /// Chamado quando o jogo é retomado da pausa.
        /// Sobrescreva para retomar comportamentos pausados.
        /// </summary>
        public virtual void Retomar() { }

        #region Unity Messages

        protected virtual void Awake()
        {
            AoDespertar();
        }

        protected virtual void Start()
        {
            AoIniciar();
        }

        protected virtual void Update()
        {
            AoAtualizar();
        }

        protected virtual void OnEnable()
        {
            AoHabilitar();
        }

        protected virtual void OnDisable()
        {
            AoDesabilitar();
        }

        #endregion
    }
}
