using UnityEngine;
using System.Collections.Generic;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Core.Gerenciadores
{
    /// <summary>
    /// Gerenciador de partículas (Singleton).
    /// Mantém um pool de partículas reutilizáveis.
    /// Acesse de qualquer lugar: GerenciadorParticulas.Instancia
    /// </summary>
    public class GerenciadorParticulas : GerenciadorBase<GerenciadorParticulas>
    {
        [Header("Pool de Partículas")]
        [SerializeField, Tooltip("Prefab da partícula padrão.")]
        private GameObject particulaPrefabPadrao;

        [SerializeField, Tooltip("Quantidade inicial de partículas no pool.")]
        private int tamanhoPool = 20;

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

        protected override void Awake()
        {
            base.Awake();
            CriarPool("Padrao", particulaPrefabPadrao, tamanhoPool);
        }

        /// <summary>
        /// Cria um pool de partículas para um tipo específico.
        /// </summary>
        public void CriarPool(string nome, GameObject prefab, int quantidade)
        {
            if (prefab == null) return;

            if (!_pools.ContainsKey(nome))
            {
                _pools[nome] = new Queue<GameObject>();
            }

            for (int i = 0; i < quantidade; i++)
            {
                GameObject particula = Instantiate(prefab, transform);
                particula.SetActive(false);
                _pools[nome].Enqueue(particula);
            }
        }

        /// <summary>
        /// Emite uma partícula de um tipo específico na posição indicada.
        /// Se o pool estiver vazio, cria uma nova instância.
        /// Se o tipo não existir, cria uma partícula fallback.
        /// </summary>
        /// <param name="nome">Nome do tipo de partícula (ex: "Padrao", "Explosao").</param>
        /// <param name="posicao">Posição onde emitir.</param>
        /// <returns>GameObject da partícula emitida.</returns>
        public GameObject Emitir(string nome, Vector2 posicao)
        {
            if (!_pools.ContainsKey(nome) || _pools[nome].Count == 0)
            {
                if (particulaPrefabPadrao != null)
                {
                    CriarPool(nome, particulaPrefabPadrao, 5);
                }
                else
                {
                    return CriarParticulaFallback(posicao);
                }
            }

            if (_pools[nome].Count == 0)
            {
                return CriarParticulaFallback(posicao);
            }

            GameObject particula = _pools[nome].Dequeue();
            particula.transform.position = posicao;
            particula.SetActive(true);

            StartCoroutine(DevolverAoPool(particula, nome, 1f));
            return particula;
        }

        /// <summary>
        /// Emite uma partícula na posição com duração customizada.
        /// </summary>
        public GameObject Emitir(string nome, Vector2 posicao, float duracao)
        {
            GameObject particula = Emitir(nome, posicao);
            if (particula != null)
            {
                StopAllCoroutines();
                StartCoroutine(DevolverAoPool(particula, nome, duracao));
            }
            return particula;
        }

        /// <summary>
        /// Desativa todas as partículas ativas.
        /// </summary>
        public void PararTodas()
        {
            foreach (var pool in _pools.Values)
            {
                foreach (var particula in pool)
                {
                    if (particula.activeSelf)
                    {
                        particula.SetActive(false);
                    }
                }
            }
        }

        private System.Collections.IEnumerator DevolverAoPool(GameObject particula, string nomePool, float atraso)
        {
            yield return new WaitForSeconds(atraso);

            if (particula != null)
            {
                particula.SetActive(false);
                particula.transform.SetParent(transform);

                if (_pools.ContainsKey(nomePool))
                {
                    _pools[nomePool].Enqueue(particula);
                }
            }
        }

        private GameObject CriarParticulaFallback(Vector2 posicao)
        {
            GameObject go = new GameObject("Particula_Fallback");
            go.transform.position = posicao;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                float dx = (i % 16) - 7.5f;
                float dy = (i / 16) - 7.5f;
                pixels[i] = (dx * dx + dy * dy <= 64f) ? Color.white : Color.clear;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            sr.sortingOrder = 100;

            Destroy(go, 1f);
            return go;
        }
    }
}
