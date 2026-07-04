using UnityEngine;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Core.Gerenciadores
{
    /// <summary>
    /// Gerenciador de áudio (Singleton).
    /// Fornece métodos para tocar efeitos sonoros e música de fundo.
    /// Mantém um pool de AudioSources para efeitos simultâneos.
    /// Acesse de qualquer lugar: GerenciadorAudio.Instancia
    /// </summary>
    public class GerenciadorAudio : GerenciadorBase<GerenciadorAudio>
    {
        [Header("Música de Fundo")]
        [SerializeField, Tooltip("AudioSource dedicado para música de fundo.")]
        private AudioSource musicaSource;

        [SerializeField, Tooltip("Clipe de música de fundo padrão (opcional).")]
        private AudioClip musicaFundoPadrao;

        [Header("Efeitos Sonoros")]
        [SerializeField, Tooltip("Prefab de AudioSource para efeitos (opcional).")]
        private AudioSource efeitoSourcePrefab;

        [SerializeField, Tooltip("Quantidade de AudioSources no pool de efeitos.")]
        private int tamanhoPoolEfeitos = 10;

        [SerializeField, Tooltip("Volume geral (0 a 1).")]
        [Range(0f, 1f)]
        private float volumeGeral = 1f;

        [SerializeField, Tooltip("Volume de música (0 a 1).")]
        [Range(0f, 1f)]
        private float volumeMusica = 0.5f;

        [SerializeField, Tooltip("Volume de efeitos (0 a 1).")]
        [Range(0f, 1f)]
        private float volumeEfeitos = 0.8f;

        private AudioSource[] _poolEfeitos;
        private int _indicePool = 0;

        protected override void Awake()
        {
            base.Awake();

            if (musicaSource == null)
            {
                musicaSource = gameObject.AddComponent<AudioSource>();
                musicaSource.loop = true;
                musicaSource.playOnAwake = false;
            }

            InicializarPoolEfeitos();

            if (musicaFundoPadrao != null)
            {
                TocarMusica(musicaFundoPadrao);
            }
        }

        private void InicializarPoolEfeitos()
        {
            _poolEfeitos = new AudioSource[tamanhoPoolEfeitos];

            for (int i = 0; i < tamanhoPoolEfeitos; i++)
            {
                if (efeitoSourcePrefab != null)
                {
                    _poolEfeitos[i] = Instantiate(efeitoSourcePrefab, transform);
                }
                else
                {
                    GameObject go = new GameObject($"Efeito_Audio_{i}");
                    go.transform.SetParent(transform);
                    _poolEfeitos[i] = go.AddComponent<AudioSource>();
                    _poolEfeitos[i].playOnAwake = false;
                }
            }
        }

        /// <summary>
        /// Toca um efeito sonoro.
        /// Se o clip for nulo, não faz nada (sem erro).
        /// </summary>
        /// <param name="clip">AudioClip do efeito.</param>
        public void TocarEfeito(AudioClip clip)
        {
            if (clip == null) return;

            AudioSource source = _poolEfeitos[_indicePool];
            _indicePool = (_indicePool + 1) % _poolEfeitos.Length;

            source.volume = volumeEfeitos * volumeGeral;
            source.PlayOneShot(clip);
        }

        /// <summary>
        /// Toca um efeito sonoro em uma posição específica no mundo.
        /// </summary>
        public void TocarEfeitoNaPosicao(AudioClip clip, Vector3 posicao)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, posicao, volumeEfeitos * volumeGeral);
        }

        /// <summary>
        /// Inicia a música de fundo.
        /// </summary>
        /// <param name="clip">AudioClip da música.</param>
        public void TocarMusica(AudioClip clip)
        {
            if (clip == null || musicaSource == null) return;

            musicaSource.clip = clip;
            musicaSource.volume = volumeMusica * volumeGeral;
            musicaSource.Play();
        }

        /// <summary>
        /// Para a música de fundo.
        /// </summary>
        public void PararMusica()
        {
            if (musicaSource != null)
            {
                musicaSource.Stop();
            }
        }

        /// <summary>
        /// Define o volume geral (0 a 1).
        /// </summary>
        public void DefinirVolume(float volume)
        {
            volumeGeral = Mathf.Clamp01(volume);

            if (musicaSource != null)
            {
                musicaSource.volume = volumeMusica * volumeGeral;
            }
        }

        /// <summary>
        /// Define o volume da música (0 a 1).
        /// </summary>
        public void DefinirVolumeMusica(float volume)
        {
            volumeMusica = Mathf.Clamp01(volume);

            if (musicaSource != null)
            {
                musicaSource.volume = volumeMusica * volumeGeral;
            }
        }

        /// <summary>
        /// Define o volume dos efeitos (0 a 1).
        /// </summary>
        public void DefinirVolumeEfeitos(float volume)
        {
            volumeEfeitos = Mathf.Clamp01(volume);
        }
    }
}
