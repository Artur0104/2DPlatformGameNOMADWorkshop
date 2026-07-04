using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Ferramentas;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Painel de ajustes em tempo real para o template de Plataforma.
    /// A hierarquia de UI deve existir na cena; este script faz bind dos callbacks.
    /// </summary>
    public class PainelAjustePlataforma : PainelAjusteBase
    {
        [Header("Referências (auto-detecta se vazias)")]
        [SerializeField, Tooltip("Referência ao jogador. Se vazio, busca via FindObjectOfType.")]
        private JogadorPlataforma jogador;

        private ComponenteMovimento _movimento;
        private Rigidbody2D _rb;

        private void OnEnable()
        {
            // Re-bind listeners apos domain reload
            if (painelRaiz != null)
                ConstruirPainel();
        }

        /// <inheritdoc/>
        protected override void ConstruirPainel()
        {
            ResolverReferencias();
            BindSliders();
            BindBotoes();
        }

        /// <inheritdoc/>
        protected override void AplicarPadroes()
        {
            ResetarTodosSliders();
        }

        private void ResolverReferencias()
        {
            if (jogador == null)
            {
                jogador = FindFirstObjectByType<JogadorPlataforma>();
            }

            if (jogador != null)
            {
                _movimento = jogador.GetComponent<ComponenteMovimento>();
                _rb = jogador.GetComponent<Rigidbody2D>();
            }
        }

        private void BindSliders()
        {
            Transform panel = painelRaiz != null ? painelRaiz.transform : null;

            // ── Movimento ──
            VincularSlider(panel, "Slider_VelocidadeAndar", 5f,
                v => _movimento?.DefinirVelocidade(v));

            VincularSlider(panel, "Slider_Desaceleracao", 8f,
                v => DefinirPropriedadeMovimento("desaceleracao", v));

            // ── Pulo ──
            VincularSlider(panel, "Slider_ForcaPulo", 10f,
                v => DefinirPropriedadeMovimento("forcaPulo", v));

            VincularSlider(panel, "Slider_CoyoteTime", 0.1f,
                v => DefinirPropriedadeMovimento("tempoCoyote", v));

            VincularSlider(panel, "Slider_BufferPulo", 0.1f,
                v => DefinirPropriedadeMovimento("bufferPulo", v));

            VincularSlider(panel, "Slider_PulosMaximos", 1f,
                v => DefinirPropriedadeMovimento("pulosMaximos", v));

            VincularSlider(panel, "Slider_Gravidade", 0f,
                v => DefinirPropriedadeMovimento("gravidade", v));

            // ── Dash ──
            VincularSlider(panel, "Slider_VelocidadeDash", 15f,
                v => DefinirPropriedadeMovimento("velocidadeDash", v));

            VincularSlider(panel, "Slider_DuracaoDash", 0.2f,
                v => DefinirPropriedadeMovimento("tempoDash", v));

            VincularSlider(panel, "Slider_CooldownDash", 0.5f,
                v => DefinirPropriedadeMovimento("cooldownDash", v));

            // ── Tempo ──
            VincularSlider(panel, "Slider_EscalaTempo", 1f,
                v => Time.timeScale = v);
        }

        private void BindBotoes()
        {
            Transform panel = painelRaiz != null ? painelRaiz.transform : null;

            VincularBotao(panel, "Btn_Resetar", AplicarPadroes);
            VincularBotao(panel, "Btn_Reiniciar", () =>
            {
                if (GerenciadorCena.Instancia != null)
                    GerenciadorCena.Instancia.RecarregarCena();
            });
        }

        private void VincularSlider(Transform raiz, string nome, float padrao, UnityEngine.Events.UnityAction<float> callback)
        {
            if (raiz == null) return;

            Transform child = FindRecursive(raiz, nome);
            if (child == null)
            {
                Debug.LogWarning("[PainelAjuste] Slider '" + nome + "' nao encontrado na hierarquia.");
                return;
            }

            Slider slider = child.GetComponentInChildren<Slider>();
            if (slider == null)
            {
                Debug.LogWarning("[PainelAjuste] Componente Slider nao encontrado em '" + nome + "'.");
                return;
            }

            slider.onValueChanged.AddListener(callback);

            // Vincular texto do valor (exibe o numero ao lado do slider)
            Transform valorTransform = child.Find("Valor");
            if (valorTransform != null)
            {
                var valorText = valorTransform.GetComponent<TMPro.TextMeshProUGUI>();
                if (valorText != null)
                {
                    slider.onValueChanged.AddListener(v => valorText.text = v.ToString("F1"));
                }
            }

            RegistrarSlider(nome, slider, padrao);
        }

        private void VincularBotao(Transform raiz, string nome, UnityEngine.Events.UnityAction callback)
        {
            if (raiz == null) return;

            Transform child = FindRecursive(raiz, nome);
            if (child == null)
            {
                Debug.LogWarning("[PainelAjuste] Botao '" + nome + "' nao encontrado na hierarquia.");
                return;
            }

            Button btn = child.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning($"[PainelAjuste] Componente Button não encontrado em '{nome}'.");
                return;
            }

            btn.onClick.AddListener(callback);
        }

        /// <summary>
        /// Define uma propriedade privada serializada do ComponenteMovimento via SerializedObject.
        /// Funciona em Editor e em builds standalone.
        /// </summary>
        private void DefinirPropriedadeMovimento(string nomeCampo, float valor)
        {
            if (_movimento == null) return;

            SerializedObject so = new SerializedObject(_movimento);
            SerializedProperty prop = so.FindProperty(nomeCampo);
            if (prop != null)
            {
                if (prop.propertyType == SerializedPropertyType.Integer)
                    prop.intValue = Mathf.RoundToInt(valor);
                else
                    prop.floatValue = valor;
                so.ApplyModifiedProperties();
            }
        }
    }
}
