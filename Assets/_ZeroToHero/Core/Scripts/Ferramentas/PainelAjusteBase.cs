using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Core.Ferramentas
{
    /// <summary>
    /// Classe base abstrata para painéis de ajuste em tempo real (overlay).
    /// A hierarquia de UI é criada no Editor; o script apenas referencia,
    /// liga/desliga e faz bind dos callbacks.
    /// Hotkey: Backtick (`) abre/fecha o painel e pausa/retoma o jogo.
    /// </summary>
    public abstract class PainelAjusteBase : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Tecla para abrir/fechar o painel.")]
        private KeyCode hotkey = KeyCode.BackQuote;

        [SerializeField, Tooltip("GameObject raiz do painel (Panel). Arraste aqui.")]
        protected GameObject painelRaiz;

        [SerializeField, Tooltip("Se true, pausa o jogo ao abrir o painel.")]
        private bool pausarAoAbrir = true;

        protected Dictionary<string, Slider> sliders = new Dictionary<string, Slider>();
        protected Dictionary<string, float> valoresPadrao = new Dictionary<string, float>();

        /// <summary>
        /// Flag estática que indica se a pausa atual foi iniciada pelo overlay (Backtick)
        /// ou pelo menu de pause (Esc). Usado para hierarquia de inputs.
        /// </summary>
        public static bool PausadoPeloOverlay { get; set; }

        /// <summary>
        /// True se o painel de overlay está visível no momento.
        /// </summary>
        public bool OverlayAberto
        {
            get { return painelRaiz != null && painelRaiz.activeSelf; }
        }

        /// <summary>
        /// Cada template implementa: busca referências na hierarquia e faz bind dos callbacks.
        /// Chamado no Start, após o painel estar pronto.
        /// </summary>
        protected abstract void ConstruirPainel();

        /// <summary>
        /// Cada template implementa: restaura todos os sliders aos valores padrão.
        /// </summary>
        protected abstract void AplicarPadroes();

        /// <summary>
        /// Chamado toda vez que o painel é aberto (backtick).
        /// Cada template pode sobrescrever para sincronizar sliders com o estado atual.
        /// </summary>
        protected virtual void SincronizarValoresAoAbrir() { }

        protected virtual void Start()
        {
            ConstruirPainel();
            if (painelRaiz != null)
            {
                painelRaiz.SetActive(false);
            }
        }

        /// <summary>
        /// Detecta a hotkey (Backtick) e alterna a visibilidade do painel.
        /// Respeita a hierarquia de inputs: se o jogo está pausado via Esc,
        /// o Backtick é ignorado.
        /// </summary>
        protected virtual void Update()
        {
            if (!Input.GetKeyDown(hotkey) || painelRaiz == null) return;

            // Se o jogo está pausado mas NÃO foi o overlay que pausou (foi Esc),
            // ignora completamente o Backtick
            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.Pausado &&
                !PausadoPeloOverlay)
            {
                return;
            }

            if (painelRaiz.activeSelf)
            {
                FecharOverlay();
            }
            else
            {
                painelRaiz.SetActive(true);
                SincronizarValoresAoAbrir();
                PausadoPeloOverlay = true;
                if (pausarAoAbrir && GerenciadorJogo.Instancia != null)
                    GerenciadorJogo.Instancia.Pausar();
            }
        }

        /// <summary>
        /// Fecha o overlay e, se a pausa foi iniciada por ele, retoma o jogo.
        /// </summary>
        public void FecharOverlay()
        {
            if (painelRaiz == null) return;

            painelRaiz.SetActive(false);

            if (PausadoPeloOverlay && GerenciadorJogo.Instancia != null)
            {
                GerenciadorJogo.Instancia.Retomar();
                PausadoPeloOverlay = false;
            }
        }

        /// <summary>
        /// Busca um Transform filho por nome recursivamente em toda a subárvore.
        /// </summary>
        protected Transform FindRecursive(Transform raiz, string nome)
        {
            if (raiz == null) return null;

            if (raiz.name == nome) return raiz;

            for (int i = 0; i < raiz.childCount; i++)
            {
                var resultado = FindRecursive(raiz.GetChild(i), nome);
                if (resultado != null) return resultado;
            }

            return null;
        }

        /// <summary>
        /// Registra um slider no dicionário interno para tracking e reset.
        /// </summary>
        protected void RegistrarSlider(string nome, Slider slider, float valorPadrao)
        {
            if (slider == null) return;
            sliders[nome] = slider;
            valoresPadrao[nome] = valorPadrao;
        }

        /// <summary>
        /// Restaura todos os sliders registrados para seus valores padrão.
        /// </summary>
        protected void ResetarTodosSliders()
        {
            foreach (var kvp in valoresPadrao)
            {
                if (sliders.TryGetValue(kvp.Key, out Slider slider))
                {
                    slider.value = kvp.Value;
                }
            }
        }
    }
}
