using UnityEngine;
using TMPro;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Controla textos flutuantes de dano, DOT e debuffs.
    /// Acumulativo: valores somam enquanto o texto está visível.
    /// </summary>
    public class TextoDanoFlutuante : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text textoDano;
        [SerializeField] private TMP_Text textoDOT;
        [SerializeField] private TMP_Text textoDebuff;

        [Header("Configuração")]
        [SerializeField] private float velocidadeSubida = 1.5f;
        [SerializeField] private float duracao = 1f;
        [SerializeField] private Color corDano = Color.red;
        [SerializeField] private Color corDOT = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color corDebuff = new Color(0.5f, 0.5f, 1f);
        [SerializeField] private Color corMoeda = new Color(1f, 0.85f, 0.3f);

        private float _timerDano;
        private float _timerDOT;
        private float _timerDebuff;
        private float _acumuladoDano;
        private float _acumuladoDOT;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>Exibe dano direto acumulativo (soma enquanto visível).</summary>
        public void MostrarDano(float valor)
        {
            if (textoDano == null) return;

            if (_timerDano > 0f)
                _acumuladoDano += valor;
            else
                _acumuladoDano = valor;

            _timerDano = duracao;
            textoDano.text = string.Format("-{0:F0}", _acumuladoDano);
            textoDano.color = corDano;
            textoDano.enabled = true;
            IniciarAnimacao();
        }

        /// <summary>Exibe dano contínuo (DOT) acumulativo.</summary>
        public void MostrarDOT(float valor)
        {
            if (textoDOT == null) return;

            if (_timerDOT > 0f)
                _acumuladoDOT += valor;
            else
                _acumuladoDOT = valor;

            _timerDOT = duracao;
            textoDOT.text = string.Format("-{0:F0}", _acumuladoDOT);
            textoDOT.color = corDOT;
            textoDOT.enabled = true;
            IniciarAnimacao();
        }

        /// <summary>Exibe texto de debuff (ex: "-40% MV").</summary>
        public void MostrarDebuff(string texto)
        {
            if (textoDebuff == null) return;

            _timerDebuff = duracao;
            textoDebuff.text = texto;
            textoDebuff.color = corDebuff;
            textoDebuff.enabled = true;
            IniciarAnimacao();
        }

        /// <summary>Exibe valor de moeda obtida (+25). Reseta acumulador de dano.</summary>
        public void MostrarMoeda(int valor)
        {
            if (textoDano == null) return;

            _timerDano = duracao;
            _acumuladoDano = 0f;
            textoDano.text = string.Format("+{0}", valor);
            textoDano.color = corMoeda;
            textoDano.enabled = true;
            IniciarAnimacao();
        }

        private void IniciarAnimacao()
        {
            _canvasGroup.alpha = 1f;
            transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float subida = velocidadeSubida * dt;
            transform.Translate(Vector2.up * subida);

            bool ativo = false;

            if (_timerDano > 0f)
            {
                _timerDano -= dt;
                if (_timerDano <= 0f && textoDano != null)
                {
                    textoDano.enabled = false;
                    _acumuladoDano = 0f;
                }
                else ativo = true;
            }

            if (_timerDOT > 0f)
            {
                _timerDOT -= dt;
                if (_timerDOT <= 0f && textoDOT != null)
                {
                    textoDOT.enabled = false;
                    _acumuladoDOT = 0f;
                }
                else ativo = true;
            }

            if (_timerDebuff > 0f)
            {
                _timerDebuff -= dt;
                if (_timerDebuff <= 0f && textoDebuff != null)
                    textoDebuff.enabled = false;
                else ativo = true;
            }

            if (ativo)
            {
                float menorTimer = Mathf.Min(
                    _timerDano > 0f ? _timerDano : 999f,
                    _timerDOT > 0f ? _timerDOT : 999f,
                    _timerDebuff > 0f ? _timerDebuff : 999f);
                _canvasGroup.alpha = menorTimer / duracao;
            }
        }
    }
}
