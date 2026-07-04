using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de spawn.
    /// Permite spawnar entidades sob demanda ou em ondas configuráveis.
    /// Útil para spawn de inimigos, itens, projéteis, etc.
    /// </summary>
    public class ComponenteSpawn : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Prefabs que podem ser spawnados.")]
        private GameObject[] prefabs;

        [SerializeField, Tooltip("Intervalo entre spawns (em segundos).")]
        private float intervaloSpawn = 2f;

        [SerializeField, Tooltip("Se ativado, spawna automaticamente ao iniciar.")]
        private bool spawnAutomatico = false;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando uma entidade é spawnada. Parâmetro: GameObject spawnado.")]
        private UnityEvent<GameObject> onSpawn;

        private float _timerSpawn = 0f;
        private bool _estaSpawnando = false;

        private void Start()
        {
            if (spawnAutomatico)
            {
                IniciarSpawn();
            }
        }

        private void Update()
        {
            if (!_estaSpawnando) return;

            _timerSpawn -= Time.deltaTime;

            if (_timerSpawn <= 0f)
            {
                SpawnarAleatorio();
                _timerSpawn = intervaloSpawn;
            }
        }

        /// <summary>
        /// Spawna uma entidade aleatória da lista de prefabs na posição do spawner.
        /// </summary>
        public GameObject SpawnarAleatorio()
        {
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning($"[ComponenteSpawn] Nenhum prefab configurado para spawn em {gameObject.name}.");
                return null;
            }

            int indice = Random.Range(0, prefabs.Length);
            return Spawnar(prefabs[indice], transform.position);
        }

        /// <summary>
        /// Spawna um prefab específico na posição indicada.
        /// </summary>
        /// <param name="prefab">Prefab a ser instanciado.</param>
        /// <param name="posicao">Posição de spawn.</param>
        public GameObject Spawnar(GameObject prefab, Vector2 posicao)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ComponenteSpawn] Prefab nulo passado para spawn em {gameObject.name}.");
                return null;
            }

            GameObject entidade = Instantiate(prefab, posicao, Quaternion.identity);
            onSpawn?.Invoke(entidade);
            return entidade;
        }

        /// <summary>
        /// Inicia o spawn automático baseado no intervalo configurado.
        /// </summary>
        public void IniciarSpawn()
        {
            _estaSpawnando = true;
            _timerSpawn = intervaloSpawn;
        }

        /// <summary>
        /// Para o spawn automático.
        /// </summary>
        public void PararSpawn()
        {
            _estaSpawnando = false;
        }

        /// <summary>
        /// Spawna uma onda configurada via ScriptableObject.
        /// </summary>
        /// <param name="onda">Configuração da onda.</param>
        public void SpawnarOnda(ConfiguracaoOnda onda)
        {
            if (onda == null) return;

            StartCoroutine(SpawnarOndaCoroutine(onda));
        }

        private System.Collections.IEnumerator SpawnarOndaCoroutine(ConfiguracaoOnda onda)
        {
            for (int i = 0; i < onda.quantidade; i++)
            {
                if (onda.prefabsInimigos != null && onda.prefabsInimigos.Length > 0)
                {
                    int indice = Random.Range(0, onda.prefabsInimigos.Length);
                    Spawnar(onda.prefabsInimigos[indice], transform.position);
                }

                yield return new WaitForSeconds(onda.intervaloSpawn);
            }
        }
    }
}
