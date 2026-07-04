using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Gerenciador de ondas para Tower Defense.
    /// Spawna inimigos em ondas configuradas via ScriptableObject ConfiguracaoOnda.
    /// Dispara eventos a cada onda iniciada/completada.
    /// </summary>
    public class GerenciadorOndas : MonoBehaviour
    {
        [Header("Ondas")]
        [SerializeField, Tooltip("Lista de configurações de ondas (ScriptableObjects).")]
        private ConfiguracaoOnda[] ondas;

        [SerializeField, Tooltip("Pausa entre ondas (em segundos).")]
        private float pausaEntreOndas = 5f;

        [SerializeField, Tooltip("Se ativado, inicia a primeira onda automaticamente.")]
        private bool iniciarAutomatico = true;

        [Header("Spawn")]
        [SerializeField, Tooltip("Ponto de spawn dos inimigos.")]
        private Transform pontoSpawn;

        [SerializeField, Tooltip("Waypoints do caminho que os inimigos devem seguir.")]
        private Transform[] waypointsCena;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando uma onda inicia. Parâmetro: índice da onda.")]
        private UnityEvent<int> onOndaIniciada;

        [SerializeField, Tooltip("Disparado quando uma onda é completada. Parâmetro: índice da onda.")]
        private UnityEvent<int> onOndaCompleta;

        [SerializeField, Tooltip("Disparado quando todas as ondas são completadas.")]
        private UnityEvent onTodasOndasCompletas;

        private int _ondaAtual = -1;
        private int _inimigosRestantes = 0;
        private bool _ondaEmAndamento = false;
        private float _timerPausa = 0f;
        private bool _todasOndasCompletas = false;

                /// <summary>Índice da onda atual (base 1).</summary>
public int OndaAtual { get { return _ondaAtual + 1; } }
                /// <summary>Total de ondas configuradas.</summary>
public int TotalOndas { get { return ondas != null ? ondas.Length : 0; } }
                /// <summary>Inimigos restantes na onda atual.</summary>
public int InimigosRestantes { get { return _inimigosRestantes; } }
                /// <summary>Tempo restante para a próxima onda.</summary>
public float TimerProximaOnda { get { return _timerPausa; } }
                /// <summary>True se todas as ondas foram completadas.</summary>
public bool TodasOndasCompletas { get { return _todasOndasCompletas; } }

        private void Start()
        {
            if (TotalOndas == 0)
            {
                Debug.LogWarning("[GerenciadorOndas] Nenhuma onda configurada.");
                return;
            }

            if (iniciarAutomatico)
            {
                IniciarProximaOnda();
            }
        }

        private void Update()
        {
            if (_todasOndasCompletas) return;

            if (_ondaEmAndamento)
            {
                if (_inimigosRestantes <= 0)
                {
                    CompletarOnda();
                }
            }
            else if (_timerPausa > 0f)
            {
                _timerPausa -= Time.deltaTime;

                if (_timerPausa <= 0f && _ondaAtual < ondas.Length - 1)
                {
                    IniciarProximaOnda();
                }
            }
        }

        /// <summary>
        /// Inicia a próxima onda da lista.
        /// </summary>
        public void IniciarProximaOnda()
        {
            if (ondas == null || _ondaAtual + 1 >= ondas.Length)
            {
                _todasOndasCompletas = true;
                if (onTodasOndasCompletas != null)
                    onTodasOndasCompletas.Invoke();
                return;
            }

            _ondaAtual++;
            _ondaEmAndamento = true;
            _inimigosRestantes = ondas[_ondaAtual].quantidade;

            if (onOndaIniciada != null)
                onOndaIniciada.Invoke(OndaAtual);

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.AtualizarOnda(OndaAtual, TotalOndas);
                GerenciadorHUD.Instancia.MostrarMensagem(string.Format("Onda {0} iniciando!", OndaAtual));
            }

            StartCoroutine(SpawnarOnda(ondas[_ondaAtual]));
        }

        private System.Collections.IEnumerator SpawnarOnda(ConfiguracaoOnda onda)
        {
            for (int i = 0; i < onda.quantidade; i++)
            {
                if (onda.prefabsInimigos == null || onda.prefabsInimigos.Length == 0)
                {
                    Debug.LogWarning("[GerenciadorOndas] Onda sem prefabs de inimigos.");
                    _inimigosRestantes = i;
                    break;
                }

                int indice = Random.Range(0, onda.prefabsInimigos.Length);

                Vector2 posicaoSpawn = pontoSpawn != null ? pontoSpawn.position : transform.position;
                GameObject inimigo = Instantiate(onda.prefabsInimigos[indice], posicaoSpawn, Quaternion.identity);

                InimigoCaminho inimigoCaminho = inimigo.GetComponent<InimigoCaminho>();
                if (inimigoCaminho != null && waypointsCena != null && waypointsCena.Length > 0)
                {
                    inimigoCaminho.DefinirWaypoints(waypointsCena);
                }

                ComponenteVida vida = inimigo.GetComponent<ComponenteVida>();
                if (vida != null)
                {
                    vida.onMorte.AddListener(delegate {
                        _inimigosRestantes--;
                    });
                }

                yield return new WaitForSeconds(onda.intervaloSpawn);
            }
        }

        private void CompletarOnda()
        {
            _ondaEmAndamento = false;
            _timerPausa = pausaEntreOndas;

            if (onOndaCompleta != null)
                onOndaCompleta.Invoke(OndaAtual);

            if (ondas[_ondaAtual] != null)
            {
                if (GerenciadorHUD.Instancia != null)
                {
                    GerenciadorHUD.Instancia.AdicionarMoedas(ondas[_ondaAtual].recompensa);
                    GerenciadorHUD.Instancia.MostrarMensagem(string.Format("Onda {0} completa! +{1} moedas", OndaAtual, ondas[_ondaAtual].recompensa));
                }
            }

            if (_ondaAtual >= ondas.Length - 1)
            {
                _todasOndasCompletas = true;
                if (onTodasOndasCompletas != null)
                    onTodasOndasCompletas.Invoke();

                if (GerenciadorJogo.Instancia != null)
                {
                    GerenciadorJogo.Instancia.VitoriaJogo();
                }
            }
        }
    }
}
