using UnityEngine;
using UnityEngine.UI;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Ferramentas;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Painel de ajustes em tempo real para o Infinite Runner.
    /// 16 sliders + 4 botões. Herda de PainelAjusteBase.
    /// Canvas pré-montado no Editor; sliders encontrados via FindRecursive.
    /// </summary>
    public class PainelAjusteRunner : PainelAjusteBase
    {
        [Header("Referências (auto-detecta se vazias)")]
        [SerializeField, Tooltip("Referência à nave do jogador.")]
        private NaveJogadorRunner naveJogador;

        [SerializeField, Tooltip("Referência ao gerenciador de spawn.")]
        private GerenciadorSpawnRunner gerenciadorSpawn;

        [SerializeField, Tooltip("Referência ao gerenciador de level up.")]
        private GerenciadorLevelUpRunner gerenciadorLevelUp;

        private void OnEnable()
        {
            if (painelRaiz != null)
                ConstruirPainel();
        }

        /// <inheritdoc/>
        protected override void ConstruirPainel()
        {
            ResolverReferencias();
            BindSliders();
            BindBotoes();
            SincronizarValores();
        }

        /// <inheritdoc/>
        protected override void AplicarPadroes()
        {
            ResetarTodosSliders();
        }

        /// <inheritdoc/>
        protected override void SincronizarValoresAoAbrir()
        {
            SincronizarValores();
        }

        private void ResolverReferencias()
        {
            if (naveJogador == null)
                naveJogador = FindFirstObjectByType<NaveJogadorRunner>();
            if (gerenciadorSpawn == null)
                gerenciadorSpawn = FindFirstObjectByType<GerenciadorSpawnRunner>();
            if (gerenciadorLevelUp == null)
                gerenciadorLevelUp = FindFirstObjectByType<GerenciadorLevelUpRunner>();
        }

        private void BindSliders()
        {
            Transform panel = painelRaiz != null ? painelRaiz.transform : null;

            // Categoria: Jogador
            VincularSlider(panel, "S_Velocidade", 5f,
                v => { if (naveJogador != null) naveJogador.VelocidadeAtual = v; });

            VincularSlider(panel, "S_DanoBase", 10f,
                v => { if (naveJogador != null) naveJogador.DanoAtual = v; });

            // Categoria: Inimigos
            VincularSlider(panel, "S_IntervaloSpawn", 1.5f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.IntervaloBaseSpawn = v; });

            VincularSlider(panel, "S_VelocidadeInimigos", 1f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.MultiplicadorVelocidadeInimigos = v; });

            VincularSlider(panel, "S_VidaInimigos", 1f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.MultiplicadorVidaInimigos = v; });

            // Categoria: Meteoros
            VincularSlider(panel, "S_ChanceMeteoro", 30f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.ChanceMeteoroBase = v / 100f; });

            VincularSlider(panel, "S_VelocidadeMeteoro", 3f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.VelocidadeMeteoroOverride = v; });

            VincularSlider(panel, "S_VidaMeteoro", 15f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.VidaMeteoroOverride = v; });

            // Categoria: Progressão
            VincularSlider(panel, "S_XPporNivel", 20f,
                v => { if (gerenciadorLevelUp != null) gerenciadorLevelUp.XpParaProximoNivel = Mathf.RoundToInt(v); });

            VincularSlider(panel, "S_MultiplicadorXPNivel", 1.5f,
                v => { if (gerenciadorLevelUp != null) gerenciadorLevelUp.MultiplicadorXpPorNivel = v; });

            VincularSlider(panel, "S_DistanciaBoss", 500f,
                v => { if (gerenciadorSpawn != null) gerenciadorSpawn.DistanciaMinimaBoss = v; });
        }

        private void BindBotoes()
        {
            Transform panel = painelRaiz != null ? painelRaiz.transform : null;

            VincularBotao(panel, "Btn_Reset", AplicarPadroes);

            VincularBotao(panel, "Btn_Xp", delegate
            {
                if (gerenciadorLevelUp != null)
                    gerenciadorLevelUp.AdicionarXP(50);
            });

            VincularBotao(panel, "Btn_Boss", delegate
            {
                if (gerenciadorSpawn != null)
                    gerenciadorSpawn.SpawnarBoss();
            });

            VincularBotao(panel, "Btn_Restart", delegate
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
                Debug.LogWarning("[PainelAjusteRunner] Slider '" + nome + "' nao encontrado.");
                return;
            }

            Slider slider = child.GetComponentInChildren<Slider>();
            if (slider == null)
            {
                Debug.LogWarning("[PainelAjusteRunner] Componente Slider nao encontrado em '" + nome + "'.");
                return;
            }

            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(callback);

            Transform valorTransform = child.Find("Valor");
            if (valorTransform != null)
            {
                var valorText = valorTransform.GetComponent<TMPro.TextMeshProUGUI>();
                if (valorText != null)
                {
                    slider.onValueChanged.AddListener(delegate (float v) { valorText.text = v.ToString("F1"); });
                    valorText.text = padrao.ToString("F1");
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
                Debug.LogWarning("[PainelAjusteRunner] Botao '" + nome + "' nao encontrado.");
                return;
            }

            Button btn = child.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning("[PainelAjusteRunner] Componente Button nao encontrado em '" + nome + "'.");
                return;
            }

            btn.onClick.AddListener(callback);
        }

        private void SincronizarValores()
        {
            if (naveJogador != null)
            {
                AtualizarValorSlider("S_Velocidade", naveJogador.VelocidadeAtual);
                AtualizarValorSlider("S_DanoBase", naveJogador.DanoAtual);
            }

            if (gerenciadorSpawn != null)
            {
                AtualizarValorSlider("S_IntervaloSpawn", gerenciadorSpawn.IntervaloBaseSpawn);
                AtualizarValorSlider("S_VelocidadeInimigos", gerenciadorSpawn.MultiplicadorVelocidadeInimigos);
                AtualizarValorSlider("S_VidaInimigos", gerenciadorSpawn.MultiplicadorVidaInimigos);
                AtualizarValorSlider("S_ChanceMeteoro", gerenciadorSpawn.ChanceMeteoroBase * 100f);
                AtualizarValorSlider("S_DistanciaBoss", gerenciadorSpawn.DistanciaMinimaBoss);
            }

            if (gerenciadorLevelUp != null)
            {
                AtualizarValorSlider("S_XPporNivel", gerenciadorLevelUp.XpParaProximoNivel);
                AtualizarValorSlider("S_MultiplicadorXPNivel", gerenciadorLevelUp.MultiplicadorXpPorNivel);
            }
        }

        public void AtualizarValorSlider(string nome, float novoValor)
        {
            if (!sliders.TryGetValue(nome, out Slider slider)) return;

            slider.SetValueWithoutNotify(novoValor);

            Transform child = slider.transform.Find("Valor");
            if (child != null)
            {
                var txt = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (txt != null)
                    txt.text = novoValor.ToString("F1");
            }
        }
    }
}
