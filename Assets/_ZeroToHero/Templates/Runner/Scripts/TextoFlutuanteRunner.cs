using UnityEngine;
using TMPro;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Texto flutuante que sobe com fade out, usado para exibir dano em inimigos.
    /// Usa Time.unscaledDeltaTime para não ser afetado por pause/overlay.
    /// </summary>
    public class TextoFlutuanteRunner : MonoBehaviour
    {
        [Header("Referência")]
        [SerializeField, Tooltip("Componente TMP_Text para exibir o texto.")]
        private TMP_Text texto;

        [Header("Configuração")]
        [SerializeField, Tooltip("Duração total do efeito em segundos.")]
        private float duracao = 1f;

        [SerializeField, Tooltip("Velocidade de subida do texto.")]
        private float velocidadeSubida = 1f;

        private float _timer;
        private Color _corInicial;

        private void Awake()
        {
            if (texto == null)
                texto = GetComponent<TMP_Text>();

            if (texto != null)
                _corInicial = texto.color;

            _timer = duracao;

            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;
        }



        /// <summary>
        /// Inicializa o texto flutuante com a mensagem e cor desejadas.
        /// </summary>
        /// <param name="mensagem">Texto a ser exibido (ex: "-10").</param>
        /// <param name="cor">Cor inicial do texto.</param>
        public void Inicializar(string mensagem, Color cor)
        {
            if (texto != null)
            {
                texto.text = mensagem;
                texto.color = cor;
                _corInicial = cor;
            }

            _timer = duracao;
        }

        private void Update()
        {
            if (_timer <= 0f || texto == null)
            {
                Destroy(gameObject);
                return;
            }

            _timer -= Time.unscaledDeltaTime;

            Vector3 pos = transform.position;
            pos.y += velocidadeSubida * Time.unscaledDeltaTime;
            transform.position = pos;

            float alpha = Mathf.Clamp01(_timer / duracao);
            Color novaCor = _corInicial;
            novaCor.a = alpha;
            texto.color = novaCor;
        }
    }
}
