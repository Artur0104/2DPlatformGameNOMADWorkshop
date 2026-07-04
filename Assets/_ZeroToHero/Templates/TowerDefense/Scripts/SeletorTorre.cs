using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Painel de seleção de torres (UI inferior).
    /// Cria botões dinamicamente a partir das ConfiguracaoTorre disponíveis.
    /// </summary>
    public class SeletorTorre : MonoBehaviour
    {
        [Header("Torres")]
        [SerializeField, Tooltip("Lista de configurações de torre disponíveis.")]
        private ConfiguracaoTorre[] torresDisponiveis;

        [Header("UI")]
        [SerializeField, Tooltip("Container onde os botões serão instanciados.")]
        private Transform containerBotoes;

        [SerializeField, Tooltip("Prefab do botão de torre (deve ter Button, Image, e TMP_Texts).")]
        private GameObject botaoTorrePrefab;

        [SerializeField, Tooltip("Cor de destaque para o botão selecionado.")]
        private Color corSelecionado = Color.green;

        [SerializeField, Tooltip("Cor normal dos botões.")]
        private Color corNormal = new Color(0.25f, 0.25f, 0.25f, 1f);

        private Button _botaoSelecionado;

        public ConfiguracaoTorre TorreSelecionada { get; private set; }
        public static SeletorTorre Instancia { get; private set; }

        private void Awake()
        {
            Instancia = this;
        }

        private void Start()
        {
            if (torresDisponiveis == null || torresDisponiveis.Length == 0)
            {
                Debug.LogWarning("[SeletorTorre] Nenhuma torre configurada.");
                return;
            }

            for (int i = 0; i < torresDisponiveis.Length; i++)
            {
                CriarBotaoTorre(torresDisponiveis[i]);
            }
        }

        /// <summary>Cria um botão de torre no container inferior.</summary>
        private void CriarBotaoTorre(ConfiguracaoTorre config)
        {
            if (botaoTorrePrefab == null || containerBotoes == null) return;

            GameObject btnGo = Instantiate(botaoTorrePrefab, containerBotoes);
            Button btn = btnGo.GetComponent<Button>();

            if (btn != null)
            {
                btn.transition = Selectable.Transition.None;
            }

            ConfiguracaoTorre configCapturada = config;
            bool eraSelecionada = (TorreSelecionada == config);

            if (btn != null)
            {
                btn.onClick.AddListener(delegate {
                    DestacarBotao(btn, configCapturada);
                    SelecionarTorre(configCapturada);
                });
            }

            Transform imgTransform = btnGo.transform.Find("Icone");
            if (imgTransform != null)
            {
                Image imgIcone = imgTransform.GetComponent<Image>();
                if (imgIcone != null && config.icone != null)
                {
                    imgIcone.sprite = config.icone;
                }
            }

            Transform nomeTransform = btnGo.transform.Find("Nome");
            if (nomeTransform != null)
            {
                TMP_Text txtNome = nomeTransform.GetComponent<TMP_Text>();
                if (txtNome != null)
                {
                    txtNome.text = config.nomeTorre;
                }
            }

            Transform custoTransform = btnGo.transform.Find("Custo");
            if (custoTransform != null)
            {
                TMP_Text txtCusto = custoTransform.GetComponent<TMP_Text>();
                if (txtCusto != null)
                {
                    txtCusto.text = string.Format("{0} gp", config.custo);
                }
            }

            if (eraSelecionada)
            {
                DestacarBotao(btn, config);
            }
        }

        /// <summary>Seleciona ou deseleciona uma torre para construção (toggle).</summary>
        public void SelecionarTorre(ConfiguracaoTorre config)
        {
            if (config == null)
            {
                RestaurarCorBotaoSelecionado();
                TorreSelecionada = null;
                _botaoSelecionado = null;
                return;
            }

            if (TorreSelecionada == config)
            {
                RestaurarCorBotaoSelecionado();
                TorreSelecionada = null;
                _botaoSelecionado = null;
            }
            else
            {
                TorreSelecionada = config;
            }
        }

        private void DestacarBotao(Button btn, ConfiguracaoTorre config)
        {
            RestaurarCorBotaoSelecionado();
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = corSelecionado;
            _botaoSelecionado = btn;
        }

        private void RestaurarCorBotaoSelecionado()
        {
            if (_botaoSelecionado != null)
            {
                var img = _botaoSelecionado.GetComponent<Image>();
                if (img != null) img.color = corNormal;
            }
        }
    }
}
