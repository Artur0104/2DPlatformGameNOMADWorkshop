using System.Collections;
using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Inimigo que patrulha entre waypoints.
    /// Move-se horizontalmente entre pontos e inverte direção ao chegar.
    /// Pode causar dano ao jogador ao encostar.
    /// </summary>
    public class InimigoPatrulha : EntidadeBase
    {
        [Header("Patrulha")]
        [SerializeField, Tooltip("Lista de pontos de patrulha (Transform). O inimigo segue a ordem.")]
        private Transform[] waypoints;

        [SerializeField, Tooltip("Velocidade de movimento horizontal.")]
        private float velocidade = 2f;

        [SerializeField, Tooltip("Tempo de espera ao chegar em um waypoint (segundos).")]
        private float tempoEspera = 0.5f;

        [SerializeField, Tooltip("Se ativado, o inimigo volta ao primeiro waypoint ao chegar ao último (loop).")]
        private bool modoLoop = true;

        [SerializeField, Tooltip("Dano causado ao encostar no jogador.")]
        private float danoContato = 1f;

        [SerializeField, Tooltip("Tag do jogador.")]
        private string tagJogador = "Jogador";

        [SerializeField, Tooltip("Intervalo mínimo entre aplicações de dano (segundos).")]
        private float cooldownDano = 0.5f;

        [SerializeField, Tooltip("Força do bounce vertical aplicada ao jogador ao pisar no inimigo (stomp).")]
        private float forcaBounceStomp = 6f;

        [Header("Ajustes Finos")]
        [SerializeField, Tooltip("Distância mínima para considerar que chegou ao waypoint.")]
        private float distanciaWaypoint = 0.1f;
        [SerializeField, Tooltip("Dead zone para flip de direção visual.")]
        private float deadZoneDirecao = 0.01f;
        [SerializeField, Tooltip("Duração do efeito squash na morte (segundos).")]
        private float duracaoSquashMorte = 0.15f;
        [SerializeField, Tooltip("Escala X durante o squash de morte.")]
        private float squashMorteX = 1.3f;
        [SerializeField, Tooltip("Escala Y durante o squash de morte.")]
        private float squashMorteY = 0.2f;
        [SerializeField, Tooltip("Duração da queda na animação de morte (segundos).")]
        private float duracaoQuedaMorte = 0.45f;
        [SerializeField, Tooltip("Fator de aceleração da queda na morte.")]
        private float fatorQuedaMorte = 4f;

        private int _indiceAtual = 0;
        private int _direcao = 1;
        private float _timerEspera = 0f;
        private bool _estaEsperando = false;
        private float _timerDano = 0f;
        private bool _estaMorta = false;

        /// <inheritdoc/>
        protected override void AoIniciar()
        {
            base.AoIniciar();

            if (vida != null)
            {
                vida.onMorte.AddListener(AoMorrerInimigo);

                // Impedir que ComponenteVida destrua o GameObject ao morrer
                // A animacao de morte e gerenciada pelo InimigoPatrulha
                vida.DestruirAoMorrer = false;
            }

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning("[InimigoPatrulha] Nenhum waypoint configurado em " + gameObject.name + ".");
            }
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            if (_estaMorta) return;

            if (_timerDano > 0f) _timerDano -= Time.deltaTime;

            if (GerenciadorJogo.Instancia != null &&
                GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando)
            {
                return;
            }

            if (waypoints == null || waypoints.Length == 0) return;

            if (_estaEsperando)
            {
                _timerEspera -= Time.deltaTime;
                if (_timerEspera <= 0f)
                {
                    _estaEsperando = false;
                    AvancarWaypoint();
                }
                return;
            }

            MoverParaWaypoint();
        }

        /// <summary>
        /// Move o inimigo em direção ao waypoint atual.
        /// </summary>
        private void MoverParaWaypoint()
        {
            Transform alvo = waypoints[_indiceAtual];
            if (alvo == null) return;
            Vector2 direcao = (alvo.position - transform.position).normalized;
            direcao.y = 0f;

            float distanciaX = Mathf.Abs(transform.position.x - alvo.position.x);

            if (distanciaX < distanciaWaypoint)
            {
                _estaEsperando = true;
                _timerEspera = tempoEspera;
                return;
            }

            transform.Translate(direcao * velocidade * Time.deltaTime);

            if (direcao.x > deadZoneDirecao)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (direcao.x < -deadZoneDirecao)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        /// <summary>
        /// Avança para o próximo waypoint.
        /// No modo loop, volta ao primeiro se chegar ao último.
        /// No modo ida-e-volta, inverte a direção.
        /// </summary>
        private void AvancarWaypoint()
        {
            if (modoLoop)
            {
                _indiceAtual = (_indiceAtual + 1) % waypoints.Length;
            }
            else
            {
                _indiceAtual += _direcao;

                if (_indiceAtual >= waypoints.Length)
                {
                    _indiceAtual = waypoints.Length - 2;
                    _direcao = -1;
                }
                else if (_indiceAtual < 0)
                {
                    _indiceAtual = 1;
                    _direcao = 1;
                }
            }
        }

        /// <summary>
        /// Ao entrar em contato com o jogador, aplica dano.
        /// Stomp é detectado pelo StompSensor (trigger no topo) e chamado via AplicarStomp().
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_estaMorta) return;
            if (!collision.gameObject.CompareTag(tagJogador)) return;

            var vidaJogador = collision.gameObject.GetComponent<ComponenteVida>();
            if (vidaJogador != null)
            {
                vidaJogador.ReceberDano(danoContato);
                _timerDano = cooldownDano;
            }
        }

        /// <summary>
        /// Chamado pelo StompSensor quando o jogador pisa no topo do inimigo.
        /// Inicia a sequencia de morte com animacao completa.
        /// </summary>
        public void AplicarStomp(Rigidbody2D jogadorRb)
        {
            if (_estaMorta) return;
            if (vida == null) return;

            _estaMorta = true;

            // Desativa todos os colliders para evitar mais interacoes
            var colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;

            // Bounce no jogador
            jogadorRb.linearVelocity = new Vector2(
                jogadorRb.linearVelocity.x, forcaBounceStomp);

            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            // Inicia animacao de morte
            StartCoroutine(CorrotinaMorrerInimigo());
        }

        private void AoMorrerInimigo()
        {
            _estaMorta = true;
        }

        private IEnumerator CorrotinaMorrerInimigo()
        {
            Vector3 escalaOriginal = transform.localScale;

            // Fase 1: squash (0.0-0.15s)
            float timer = 0f;
            while (timer < duracaoSquashMorte)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duracaoSquashMorte;
                transform.localScale = new Vector3(
                    Mathf.Lerp(escalaOriginal.x, escalaOriginal.x * squashMorteX, t),
                    Mathf.Lerp(escalaOriginal.y, escalaOriginal.y * squashMorteY, t),
                    escalaOriginal.z);
                yield return null;
            }

            // Fase 2: queda + fade (0.15-0.6s)
            float posYInicial = transform.position.y;
            float timerQueda = 0f;

            while (timerQueda < duracaoQuedaMorte)
            {
                timerQueda += Time.unscaledDeltaTime;
                float t = timerQueda / duracaoQuedaMorte;

                // Queda para baixo
                float y = posYInicial - (t * t * fatorQuedaMorte);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);

                // Fade out no sprite
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f - t;
                    sr.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Ao colidir continuamente com o jogador, causa dano com cooldown.
        /// </summary>
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (_estaMorta) return;
            if (_timerDano > 0f) return;
            if (!collision.gameObject.CompareTag(tagJogador)) return;

            ComponenteVida vidaJogador = collision.gameObject.GetComponent<ComponenteVida>();
            if (vidaJogador != null)
            {
                vidaJogador.ReceberDano(danoContato);
                _timerDano = cooldownDano;
            }
        }

        /// <summary>
        /// Desenha gizmos dos waypoints no Editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.red;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);

                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                    else if (modoLoop && waypoints[0] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }
    }
}
