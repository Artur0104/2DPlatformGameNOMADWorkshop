using UnityEngine;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de efeitos visuais e sonoros.
    /// Fornece métodos para emitir partículas, tocar sons e vibrar a câmera.
    /// Possui fallback seguro: se nenhum asset estiver configurado, o jogo não quebra.
    /// </summary>
    public class ComponenteEfeito : MonoBehaviour
    {
        [Header("Partículas")]
        [SerializeField, Tooltip("Prefab de partícula padrão. Se vazio, usa um círculo branco genérico.")]
        private GameObject particulaPrefab;

        [SerializeField, Tooltip("Duração padrão da partícula em segundos.")]
        private float duracaoParticula = 1f;

        [Header("Sons")]
        [SerializeField, Tooltip("AudioSource para efeitos sonoros. Se vazio, usa PlayClipAtPoint.")]
        private AudioSource audioSrcEfeitos;

        [SerializeField, Tooltip("Som de dano (opcional).")]
        private AudioClip somDano;

        [SerializeField, Tooltip("Som de pulo (opcional).")]
        private AudioClip somPulo;

        [SerializeField, Tooltip("Som de ataque (opcional).")]
        private AudioClip somAtaque;

        [SerializeField, Tooltip("Som de coleta (opcional).")]
        private AudioClip somColeta;

        [Header("Câmera")]
        [SerializeField, Tooltip("Intensidade da vibração da câmera.")]
        private float intensidadeVibracao = 0.1f;

        [SerializeField, Tooltip("Duração da vibração da câmera em segundos.")]
        private float duracaoVibracao = 0.2f;

        private void Awake()
        {
            if (audioSrcEfeitos == null)
            {
                audioSrcEfeitos = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Emite uma partícula na posição especificada.
        /// Se não houver prefab configurado, cria um círculo branco padrão.
        /// </summary>
        /// <param name="posicao">Posição onde a partícula será emitida.</param>
        public void EmitirParticula(Vector2 posicao)
        {
            if (GerenciadorParticulas.Instancia != null)
            {
                GerenciadorParticulas.Instancia.Emitir("Padrao", posicao);
                return;
            }

            if (particulaPrefab != null)
            {
                GameObject particula = Instantiate(particulaPrefab, posicao, Quaternion.identity);
                Destroy(particula, duracaoParticula);
                return;
            }

            CriarParticulaFallback(posicao);
        }

        /// <summary>
        /// Cria uma partícula fallback: um círculo branco que faz fade out.
        /// </summary>
        private void CriarParticulaFallback(Vector2 posicao)
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

            ParticulaFallback pf = go.AddComponent<ParticulaFallback>();
            pf.IniciarFade(duracaoParticula);
        }

        /// <summary>
        /// Toca um AudioClip. Se o clip for nulo, não faz nada (sem erro).
        /// </summary>
        /// <param name="clip">AudioClip a ser tocado. Pode ser nulo.</param>
        public void TocarSom(AudioClip clip)
        {
            if (clip == null) return;

            if (audioSrcEfeitos != null)
            {
                audioSrcEfeitos.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        /// <summary>
        /// Toca o som de dano configurado no Inspector.
        /// </summary>
        public void TocarSomDano()
        {
            TocarSom(somDano);
        }

        /// <summary>
        /// Toca o som de pulo configurado no Inspector.
        /// </summary>
        public void TocarSomPulo()
        {
            TocarSom(somPulo);
        }

        /// <summary>
        /// Toca o som de ataque configurado no Inspector.
        /// </summary>
        public void TocarSomAtaque()
        {
            TocarSom(somAtaque);
        }

        /// <summary>
        /// Toca o som de coleta configurado no Inspector.
        /// </summary>
        public void TocarSomColeta()
        {
            TocarSom(somColeta);
        }

        /// <summary>
        /// Vibra a câmera principal.
        /// Se Cinemachine estiver presente, usa Cinemachine.
        /// Caso contrário, faz vibração manual via transform.
        /// </summary>
        public void VibrarCamera()
        {
            StartCoroutine(VibracaoCameraCoroutine());
        }

        private System.Collections.IEnumerator VibracaoCameraCoroutine()
        {
            Camera cam = Camera.main;
            if (cam == null) yield break;

            Transform camTransform = cam.transform;
            Vector3 posicaoOriginal = camTransform.position;
            float tempo = 0f;

            while (tempo < duracaoVibracao)
            {
                tempo += Time.deltaTime;
                float forca = intensidadeVibracao * (1f - tempo / duracaoVibracao);
                Vector3 deslocamento = Random.insideUnitCircle * forca;
                camTransform.position = posicaoOriginal + deslocamento;
                yield return null;
            }

            camTransform.position = posicaoOriginal;
        }
    }

    /// <summary>
    /// Helper interno para efeito de fade out em partículas fallback.
    /// </summary>
    internal class ParticulaFallback : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private float _duracao;
        private float _timer;

        public void IniciarFade(float duracao)
        {
            _sr = GetComponent<SpriteRenderer>();
            _duracao = duracao;
            _timer = duracao;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = _timer / _duracao;
                _sr.color = c;
            }

            if (_timer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
