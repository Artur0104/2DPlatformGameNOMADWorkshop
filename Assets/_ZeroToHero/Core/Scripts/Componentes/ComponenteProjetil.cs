using UnityEngine;
using ZeroToHero.Core.Interfaces;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de projétil.
    /// Ao ser disparado, move-se na direção configurada e causa dano ao colidir.
    /// Suporta múltiplos modos de trajetória: reto, parábola, teleguiado, circular.
    /// </summary>
    public class ComponenteProjetil : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Velocidade do projétil.")]
        private float velocidade = 8f;

        [SerializeField, Tooltip("Dano causado ao atingir um alvo.")]
        private float dano = 10f;

        [SerializeField, Tooltip("Tempo de vida do projétil em segundos. 0 = vida infinita (destruído apenas ao colidir).")]
        private float tempoVida = 5f;

        [SerializeField, Tooltip("Se ativado, o projétil é destruído ao colidir com qualquer coisa.")]
        private bool destruirAoColidir = true;

        [SerializeField, Tooltip("Se ativado, o projétil atravessa inimigos sem ser destruído.")]
        private bool atravessaInimigos = false;

        [SerializeField, Tooltip("Modo de trajetória do projétil.")]
        private ModoTrajetoria modo = ModoTrajetoria.Reto;

        [SerializeField, Tooltip("Alvo para modo Teleguiado.")]
        private Transform alvo;

        [SerializeField, Tooltip("Velocidade de rotação para modo Teleguiado (graus por segundo).")]
        private float velocidadeRotacao = 180f;

        [SerializeField, Tooltip("Raio para modo Circular.")]
        private float raioCircular = 3f;

        [SerializeField, Tooltip("Velocidade angular para modo Circular.")]
        private float velocidadeAngular = 180f;

        [SerializeField, Tooltip("LayerMask para filtrar o que o projétil atinge.")]
        private LayerMask camadaAlvo = -1;

        public enum ModoTrajetoria
        {
            Reto,
            Parabola,
            Teleguiado,
            Circular
        }

        private Vector2 _direcao;
        private float _timerVida;
        private float _anguloAtual;
        private Vector2 _centroCirculo;
        private float _gravidade = 9.8f;
        private float _velocidadeVertical;

        private void Start()
        {
            _timerVida = tempoVida;
            _centroCirculo = transform.position;

            if (modo == ModoTrajetoria.Parabola)
            {
                _velocidadeVertical = velocidade * 0.5f;
            }
        }

        private void Update()
        {
            if (tempoVida > 0f)
            {
                _timerVida -= Time.deltaTime;
                if (_timerVida <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            switch (modo)
            {
                case ModoTrajetoria.Reto:
                    transform.Translate(_direcao * velocidade * Time.deltaTime, Space.World);
                    break;

                case ModoTrajetoria.Parabola:
                    _velocidadeVertical -= _gravidade * Time.deltaTime;
                    Vector2 movimentoParabola = new Vector2(_direcao.x * velocidade, _velocidadeVertical);
                    transform.Translate(movimentoParabola * Time.deltaTime);
                    break;

                case ModoTrajetoria.Teleguiado:
                    if (alvo != null)
                    {
                        Vector2 direcaoAlvo = (alvo.position - transform.position).normalized;
                        float angulo = Mathf.Atan2(direcaoAlvo.y, direcaoAlvo.x) * Mathf.Rad2Deg;
                        Quaternion alvoRot = Quaternion.Euler(0f, 0f, angulo);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, alvoRot, velocidadeRotacao * Time.deltaTime);
                        _direcao = transform.right;
                    }
                    transform.Translate(_direcao * velocidade * Time.deltaTime);
                    break;

                case ModoTrajetoria.Circular:
                    _anguloAtual += velocidadeAngular * Time.deltaTime;
                    float rad = _anguloAtual * Mathf.Deg2Rad;
                    Vector2 posicao = _centroCirculo + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * raioCircular;
                    transform.position = posicao;
                    break;
            }
        }

        /// <summary>
        /// Dispara o projétil na direção especificada.
        /// </summary>
        /// <param name="direcao">Direção normalizada do disparo.</param>
        public void Disparar(Vector2 direcao)
        {
            _direcao = direcao.normalized;

            if (modo != ModoTrajetoria.Circular)
            {
                float angulo = Mathf.Atan2(_direcao.y, _direcao.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angulo);
            }
        }

        /// <summary>
        /// Configura a trajetória do projétil em tempo de execução.
        /// </summary>
        public void ConfigurarTrajetoria(ModoTrajetoria novoModo, Transform novoAlvo = null)
        {
            modo = novoModo;
            alvo = novoAlvo;
        }

        /// <summary>
        /// Inicializa os valores do projétil em runtime (usado pelo DisparadorArmas).
        /// </summary>
        public void InicializarValores(float novoDano, float novaVelocidade, float novoTempoVida, bool novoAtravessa, int novaCamadaAlvo = -1)
        {
            dano = novoDano;
            velocidade = novaVelocidade;
            tempoVida = novoTempoVida;
            _timerVida = novoTempoVida;
            atravessaInimigos = novoAtravessa;
            if (novaCamadaAlvo >= 0)
                camadaAlvo = novaCamadaAlvo;
        }

        /// <summary>
        /// Define o alvo para o modo Teleguiado.
        /// </summary>
        public void DefinirAlvo(Transform novoAlvo)
        {
            alvo = novoAlvo;
        }

        /// <summary>
        /// Define o modo de trajetória sem alterar o alvo.
        /// </summary>
        public void DefinirModo(ModoTrajetoria novoModo)
        {
            modo = novoModo;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!destruirAoColidir) return;

            if (((1 << other.gameObject.layer) & camadaAlvo) != 0)
            {
                Bater(other.gameObject);

                if (!atravessaInimigos)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Chamado quando o projétil atinge um alvo.
        /// Aplica dano se o alvo implementar IDanificavel.
        /// Sobrescreva para adicionar efeitos de impacto.
        /// </summary>
        public virtual void Bater(GameObject alvoAtingido)
        {
            IDanificavel danificavel = alvoAtingido.GetComponent<IDanificavel>();
            if (danificavel != null)
            {
                danificavel.ReceberDano(dano);
            }
        }
    }
}
