using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Interfaces;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de ataque genérico.
    /// Suporta ataque corpo a corpo, à distância (projétil) e em área.
    /// Configurável via ScriptableObject ConfiguracaoAtaque ou diretamente no Inspector.
    /// </summary>
    public class ComponenteAtaque : MonoBehaviour
    {
        [Header("Configuração (via SO ou manual)")]
        [SerializeField, Tooltip("ScriptableObject com a configuração de ataque. Se definido, sobrescreve os campos abaixo.")]
        private ConfiguracaoAtaque config;

        [SerializeField, Tooltip("Tipo de ataque.")]
        private TipoAtaque tipoAtaque = TipoAtaque.CorpoACorpo;

        [SerializeField, Tooltip("Dano causado pelo ataque.")]
        private float dano = 10f;

        [SerializeField, Tooltip("Alcance do ataque (raio para corpo a corpo e área).")]
        private float alcance = 1.5f;

        [SerializeField, Tooltip("Tempo de recarga entre ataques (em segundos).")]
        private float cooldown = 0.5f;

        [SerializeField, Tooltip("Prefab do projétil (para ataques à distância).")]
        private GameObject projetilPrefab;

        [SerializeField, Tooltip("Camadas que serão atingidas pelo ataque.")]
        private LayerMask camadaAlvo = -1;

        [SerializeField, Tooltip("Empurrão aplicado ao alvo ao ser atingido.")]
        private float forcaEmpurrao = 5f;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o ataque é executado.")]
        private UnityEvent onAtacar;

        [SerializeField, Tooltip("Disparado quando o ataque atinge um alvo. Parâmetro: GameObject atingido.")]
        private UnityEvent<GameObject> onAtingirAlvo;

        public enum TipoAtaque
        {
            CorpoACorpo,
            Distancia,
            Area
        }

        private float _timerCooldown = 0f;
        private Transform _alvo;
        private bool _alvoDefinido = false;

        public bool EstaPronto => _timerCooldown <= 0f;

        private void Awake()
        {
            if (config != null)
            {
                AplicarConfiguracao(config);
            }
        }

        private void Update()
        {
            if (_timerCooldown > 0f)
            {
                _timerCooldown -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Executa o ataque. O comportamento depende do tipo configurado:
        /// Corpo a corpo: causa dano a todos os alvos no alcance.
        /// Distância: instancia um projétil na direção do alvo.
        /// Área: causa dano em área ao redor do atacante.
        /// </summary>
        public virtual void Atacar()
        {
            if (!EstaPronto) return;

            _timerCooldown = cooldown;
            onAtacar?.Invoke();

            switch (tipoAtaque)
            {
                case TipoAtaque.CorpoACorpo:
                    AtacarCorpoACorpo();
                    break;

                case TipoAtaque.Distancia:
                    AtacarDistancia();
                    break;

                case TipoAtaque.Area:
                    AtacarArea();
                    break;
            }
        }

        /// <summary>
        /// Define um alvo para o ataque.
        /// Para ataques à distância, o projétil será disparado na direção do alvo.
        /// </summary>
        public void DefinirAlvo(Transform novoAlvo)
        {
            _alvo = novoAlvo;
            _alvoDefinido = novoAlvo != null;
        }

        private void AtacarCorpoACorpo()
        {
            Vector2 origem = transform.position;
            float direcao = transform.localScale.x > 0f ? 1f : -1f;
            Vector2 centro = origem + new Vector2(direcao * alcance * 0.5f, 0f);

            Collider2D[] atingidos = Physics2D.OverlapCircleAll(centro, alcance, camadaAlvo);

            foreach (Collider2D col in atingidos)
            {
                if (col.gameObject == gameObject) continue;

                AplicarDano(col.gameObject);
            }
        }

        private void AtacarDistancia()
        {
            if (projetilPrefab == null)
            {
                Debug.LogWarning($"[ComponenteAtaque] Projétil prefab não definido em {gameObject.name}. Ataque à distância não pode ser executado.");
                return;
            }

            Vector2 direcao;

            if (_alvoDefinido && _alvo != null)
            {
                direcao = (_alvo.position - transform.position).normalized;
            }
            else
            {
                direcao = transform.localScale.x > 0f ? Vector2.right : Vector2.left;
            }

            Vector2 posicaoDisparo = (Vector2)transform.position + direcao * 1f;
            GameObject projetil = Instantiate(projetilPrefab, posicaoDisparo, Quaternion.identity);

            ComponenteProjetil cp = projetil.GetComponent<ComponenteProjetil>();
            if (cp != null)
            {
                cp.Disparar(direcao);
            }
            else
            {
                Rigidbody2D rb = projetil.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direcao * 10f;
                }
            }

            IgnorarColisaoProjetil(projetil);
        }

        private void AtacarArea()
        {
            Collider2D[] atingidos = Physics2D.OverlapCircleAll(transform.position, alcance, camadaAlvo);

            foreach (Collider2D col in atingidos)
            {
                if (col.gameObject == gameObject) continue;

                AplicarDano(col.gameObject);
            }
        }

        private void AplicarDano(GameObject alvo)
        {
            IDanificavel danificavel = alvo.GetComponent<IDanificavel>();
            if (danificavel != null)
            {
                danificavel.ReceberDano(dano);
            }

            Rigidbody2D rbAlvo = alvo.GetComponent<Rigidbody2D>();
            if (rbAlvo != null && forcaEmpurrao > 0f)
            {
                Vector2 direcaoEmpurrao = (alvo.transform.position - transform.position).normalized;
                rbAlvo.AddForce(direcaoEmpurrao * forcaEmpurrao, ForceMode2D.Impulse);
            }

            onAtingirAlvo?.Invoke(alvo);
        }

        private void IgnorarColisaoProjetil(GameObject projetil)
        {
            Collider2D colProjetil = projetil.GetComponent<Collider2D>();
            Collider2D colAtacante = GetComponent<Collider2D>();

            if (colProjetil != null && colAtacante != null)
            {
                Physics2D.IgnoreCollision(colProjetil, colAtacante);
            }
        }

        /// <summary>
        /// Aplica as configurações de um ScriptableObject ConfiguracaoAtaque.
        /// </summary>
        public void AplicarConfiguracao(ConfiguracaoAtaque novaConfig)
        {
            if (novaConfig == null) return;

            config = novaConfig;
            tipoAtaque = novaConfig.tipoAtaque;
            dano = novaConfig.dano;
            alcance = novaConfig.alcance;
            cooldown = novaConfig.cooldown;
            projetilPrefab = novaConfig.projetilPrefab;
            camadaAlvo = novaConfig.camadaAlvo;
            forcaEmpurrao = novaConfig.forcaEmpurrao;
        }
    }
}
