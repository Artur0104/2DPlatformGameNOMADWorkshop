using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Painel lateral que exibe informações da torre selecionada.
    /// Mostra stats atuais, preview de melhoria no hover e botões Melhorar/Vender.
    /// </summary>
    public class PainelTorreInfo : MonoBehaviour
    {
        [Header("Painel")]
        [SerializeField] private GameObject painelRaiz;

        [Header("Textos de Stats")]
        [SerializeField] private TMP_Text textoTitulo;
        [SerializeField] private TMP_Text textoDano;
        [SerializeField] private TMP_Text textoAlcance;
        [SerializeField] private TMP_Text textoVelAtaque;
        [SerializeField] private TMP_Text textoCustoMelhoria;

        [Header("Botões")]
        [SerializeField] private Button btnMelhorar;
        [SerializeField] private Button btnVender;

        [Header("Visual")]
        [SerializeField] private Color corHighlight = new Color(1f, 1f, 0f, 0.3f);

        private Torre _torreAtual;
        private GameObject _fantasmaRange;
        private SpriteRenderer _srFantasmaRange;
        private GameObject _highlightGo;
        private bool _eventosInscritos;
        private bool _hoverMelhorar;
        private static Sprite _spriteRangeCache;

        private void Awake()
        {
            InscreverEventos();
            if (painelRaiz != null)
                painelRaiz.SetActive(false);
        }

        private void Start()
        {
            InscreverEventos();
        }

        private void OnEnable()
        {
            InscreverEventos();
            ConfigurarBotoes();
            ConectarHoverMelhorar();
        }

        private void OnDestroy()
        {
            Torre.OnTorreSelecionada -= SelecionarTorre;
            Torre.OnTorreMelhorada -= AoMelhorarTorre;
        }

        private void InscreverEventos()
        {
            if (_eventosInscritos) return;
            Torre.OnTorreSelecionada += SelecionarTorre;
            Torre.OnTorreMelhorada += AoMelhorarTorre;
            _eventosInscritos = true;
        }

        private void Update()
        {
            if (_torreAtual != null && _hoverMelhorar)
                AtualizarPreviewRange();
        }

        private void ConfigurarBotoes()
        {
            if (btnMelhorar != null)
            {
                btnMelhorar.onClick.RemoveAllListeners();
                btnMelhorar.onClick.AddListener(MelhorarTorre);
            }

            if (btnVender != null)
            {
                btnVender.onClick.RemoveAllListeners();
                btnVender.onClick.AddListener(VenderTorre);
            }
        }

        private void ConectarHoverMelhorar()
        {
            if (btnMelhorar == null) return;

            var trigger = btnMelhorar.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = btnMelhorar.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            trigger.triggers.Clear();

            var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener(delegate { OnHoverMelhorarEnter(); });
            trigger.triggers.Add(entryEnter);

            var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            entryExit.callback.AddListener(delegate { OnHoverMelhorarExit(); });
            trigger.triggers.Add(entryExit);
        }

    private void SelecionarTorre(Torre torre)
    {
        if (torre == null)
        {
            FecharPainel();
            return;
        }

        if (_torreAtual == torre)
            {
                FecharPainel();
                return;
            }

            RemoverHighlight();
            EsconderRangeFantasma();
            _torreAtual = torre;
            AtualizarStats();
            MostrarPainel(true);
            AplicarHighlight(torre);
            MostrarRangeFantasma(torre.ObterAlcance(), false);
        }

        private void AtualizarStats()
        {
            if (_torreAtual == null) return;

            ConfiguracaoTorre config = _torreAtual.ObterConfig();
            string nome = config != null ? config.nomeTorre : "Torre";

            if (textoTitulo != null)
                textoTitulo.text = string.Format("{0} — Nv. {1}", nome, _torreAtual.ObterNivel());

            if (textoDano != null)
                textoDano.text = string.Format("Dano: {0:F0}", _torreAtual.ObterDano());

            if (textoAlcance != null)
                textoAlcance.text = string.Format("Alcance: {0:F1}", _torreAtual.ObterAlcance());

            if (textoVelAtaque != null)
                textoVelAtaque.text = string.Format("Velocidade: {0:F1}/s", _torreAtual.ObterVelocidadeAtaque());

            if (textoCustoMelhoria != null)
                textoCustoMelhoria.text = string.Format("Melhoria: {0} gp", _torreAtual.ObterCustoMelhoria());
        }

        private void MostrarPreviewStats()
        {
            if (_torreAtual == null) return;

            float danoProx, alcanceProx, velProx;
            _torreAtual.ObterStatsProximoNivel(out danoProx, out alcanceProx, out velProx);

            if (textoDano != null)
                textoDano.text = string.Format("Dano: {0:F0} \u2192 <color=#4FC3F7>{1:F0}</color>",
                    _torreAtual.ObterDano(), danoProx);

            if (textoAlcance != null)
                textoAlcance.text = string.Format("Alcance: {0:F1} \u2192 <color=#4FC3F7>{1:F1}</color>",
                    _torreAtual.ObterAlcance(), alcanceProx);

            if (textoVelAtaque != null)
                textoVelAtaque.text = string.Format("Velocidade: {0:F1}/s \u2192 <color=#4FC3F7>{1:F1}/s</color>",
                    _torreAtual.ObterVelocidadeAtaque(), velProx);

            MostrarRangeFantasma(alcanceProx, true);
        }

        private void AtualizarPreviewRange()
        {
            if (_srFantasmaRange != null && _torreAtual != null)
            {
                _srFantasmaRange.transform.position = _torreAtual.transform.position;
            }
        }

        private void MostrarRangeFantasma(float alcance, bool ehPreview)
        {
            EsconderRangeFantasma();
            if (_torreAtual == null) return;

            _fantasmaRange = new GameObject("RangeFantasma");
            _fantasmaRange.transform.position = _torreAtual.transform.position;
            _srFantasmaRange = _fantasmaRange.AddComponent<SpriteRenderer>();
            _srFantasmaRange.sortingOrder = 50;

            if (_spriteRangeCache == null)
            {
                int s = 64;
                Texture2D tex = new Texture2D(s, s);
                Color[] pix = new Color[s * s];
                float centro = s / 2f;
                for (int y = 0; y < s; y++)
                    for (int x = 0; x < s; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                        pix[y * s + x] = d <= centro ? Color.white : Color.clear;
                    }
                tex.SetPixels(pix); tex.Apply();
                _spriteRangeCache = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 64f);
            }

            _srFantasmaRange.sprite = _spriteRangeCache;
            _srFantasmaRange.color = ehPreview
                ? new Color(0.31f, 0.76f, 0.97f, 0.25f)
                : new Color(1f, 1f, 1f, 0.2f);

            float diametro = alcance * 2f;
            _fantasmaRange.transform.localScale = new Vector3(diametro, diametro, 1f);
        }

        private void EsconderRangeFantasma()
        {
            if (_fantasmaRange != null)
            {
                Destroy(_fantasmaRange);
                _fantasmaRange = null;
                _srFantasmaRange = null;
            }
        }

        public void OnHoverMelhorarEnter()
        {
            _hoverMelhorar = true;
            float a, b, c;
            _torreAtual.ObterStatsProximoNivel(out a, out b, out c);
            MostrarPreviewStats();
            MostrarRangeFantasma(b, true);
        }

        public void OnHoverMelhorarExit()
        {
            _hoverMelhorar = false;
            MostrarRangeFantasma(_torreAtual.ObterAlcance(), false);
            AtualizarStats();
        }

        private void MelhorarTorre()
        {
            if (_torreAtual != null)
                _torreAtual.Melhorar();
        }

        private void VenderTorre()
        {
            if (_torreAtual != null)
                _torreAtual.Vender();
            MostrarPainel(false);
            _torreAtual = null;
            EsconderRangeFantasma();
        }

        private void AoMelhorarTorre(int novoNivel)
        {
            AtualizarStats();
            EsconderRangeFantasma();
        }

        public void FecharPainel()
        {
            RemoverHighlight();
            MostrarPainel(false);
            _torreAtual = null;
            EsconderRangeFantasma();
        }

        private void MostrarPainel(bool mostrar)
        {
            if (painelRaiz != null)
                painelRaiz.SetActive(mostrar);
        }

        private void AplicarHighlight(Torre torre)
        {
            if (_highlightGo != null) Destroy(_highlightGo);

            SpriteRenderer sr = torre.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            _highlightGo = new GameObject("Highlight");
            _highlightGo.transform.SetParent(torre.transform, false);
            _highlightGo.transform.localPosition = Vector3.zero;
            _highlightGo.transform.localScale = Vector3.one * 1.3f;

            var srH = _highlightGo.AddComponent<SpriteRenderer>();
            srH.sprite = sr.sprite;
            srH.color = corHighlight;
            srH.sortingOrder = sr.sortingOrder - 1;
        }

        private void RemoverHighlight()
        {
            if (_highlightGo != null)
            {
                Destroy(_highlightGo);
                _highlightGo = null;
            }
        }
    }
}
