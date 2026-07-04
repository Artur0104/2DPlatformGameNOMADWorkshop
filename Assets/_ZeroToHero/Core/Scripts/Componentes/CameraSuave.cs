using UnityEngine;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Câmera com follow suave, antecipação (look-ahead) e confinamento opcional.
    /// Adicione este script à câmera principal e defina o Transform alvo.
    /// </summary>
    public class CameraSuave : MonoBehaviour
    {
        [Header("Alvo")]
        [SerializeField, Tooltip("Transform que a câmera deve seguir.")]
        private Transform alvo;

        [Header("Suavidade")]
        [SerializeField, Tooltip("Tempo de suavização (damping). Menor = mais rápido.")]
        private float suavidade = 0.2f;

        [SerializeField, Tooltip("Offset fixo em relação ao alvo.")]
        private Vector2 offset = Vector2.zero;

        [Header("Antecipação")]
        [SerializeField, Tooltip("Quanto a câmera antecipa na direção do movimento. X = horizontal, Y = vertical.")]
        private Vector2 antecipacao = new Vector2(0.3f, 0.1f);

        [Header("Confinamento")]
        [SerializeField, Tooltip("Se ativado, a câmera não ultrapassa os limites definidos.")]
        private bool confinar = false;

        [SerializeField, Tooltip("Limite mínimo (canto inferior esquerdo).")]
        private Vector2 limiteMin;

        [SerializeField, Tooltip("Limite máximo (canto superior direito).")]
        private Vector2 limiteMax;

        private Vector3 _velocidade = Vector3.zero;
        private Rigidbody2D _rbAlvo;
        private Camera _cam;

        private void Start()
        {
            _cam = GetComponent<Camera>();

            if (alvo == null)
            {
                var jogador = GameObject.FindGameObjectWithTag("Jogador");
                if (jogador != null) alvo = jogador.transform;
            }

            if (alvo != null)
                _rbAlvo = alvo.GetComponent<Rigidbody2D>();
        }

        private void LateUpdate()
        {
            if (alvo == null) return;

            Vector3 posAlvo = alvo.position;

            // Antecipação: câmera olha para onde o alvo está indo
            if (_rbAlvo != null)
                posAlvo += (Vector3)(_rbAlvo.linearVelocity * antecipacao);

            Vector3 destino = posAlvo + (Vector3)offset;
            destino.z = transform.position.z;

            // Confinamento
            if (confinar && _cam != null)
            {
                float halfH = _cam.orthographicSize;
                float halfW = halfH * _cam.aspect;
                float minX = Mathf.Min(limiteMin.x, limiteMax.x) + halfW;
                float maxX = Mathf.Max(limiteMin.x, limiteMax.x) - halfW;
                float minY = Mathf.Min(limiteMin.y, limiteMax.y) + halfH;
                float maxY = Mathf.Max(limiteMin.y, limiteMax.y) - halfH;
                destino.x = Mathf.Clamp(destino.x, minX, maxX);
                destino.y = Mathf.Clamp(destino.y, minY, maxY);
            }

            transform.position = Vector3.SmoothDamp(
                transform.position, destino, ref _velocidade, suavidade);
        }

        /// <summary>
        /// Define o alvo da câmera em tempo de execução.
        /// </summary>
        public void DefinirAlvo(Transform novoAlvo)
        {
            alvo = novoAlvo;
            if (alvo != null)
                _rbAlvo = alvo.GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Zera a velocidade interna do SmoothDamp. Útil para evitar
        /// que a câmera continue se movendo após o alvo parar (ex: morte).
        /// </summary>
        public void ResetarSuavidade()
        {
            _velocidade = Vector3.zero;
        }
    }
}
