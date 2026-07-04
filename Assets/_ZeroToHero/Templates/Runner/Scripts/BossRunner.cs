using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Boss do Infinite Runner.
    /// Sempre posicionado no topo da tela (Y relativo ao jogador).
    /// Movimenta-se lateralmente e troca de fase conforme % de vida.
    /// Cada fase tem seu próprio padrão de disparo em leque.
    /// </summary>
    public class BossRunner : EntidadeBase
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("ScriptableObject com parâmetros do boss.")]
        private ConfiguracaoBossRunner config;

        [Header("Projéteis")]
        [SerializeField, Tooltip("Prefab do projétil disparado.")]
        private GameObject projetilPrefab;

        [SerializeField, Tooltip("Pontos de disparo (filhos do boss).")]
        private Transform[] pontosDisparo;

        [Header("Posicionamento")]
        [SerializeField, Tooltip("Limite esquerdo de movimento lateral.")]
        private float limiteEsquerdo = -4f;

        [SerializeField, Tooltip("Limite direito de movimento lateral.")]
        private float limiteDireito = 4f;

        private Transform _jogador;
        private int _faseAtual;
        private float _timerAtaque;
        private float _vidaAtual;
        private float _direcaoMovimento = 1f;
        private GameObject _goBossHp;
        private Slider _sliderBoss;

        public ConfiguracaoBossRunner ConfigSource
        {
            set { config = value; }
        }

        public GameObject PrefabProjetil
        {
            set { projetilPrefab = value; }
        }

        /// <inheritdoc/>
        protected override void AoIniciar()
        {
            if (config == null)
            {
                Destroy(gameObject);
                return;
            }

            _vidaAtual = config.Vida;
            _faseAtual = 0;
            _timerAtaque = 0f;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            if (vida != null)
            {
                vida.DefinirVidaMaxima(_vidaAtual);
                vida.onDano.AddListener(AoReceberDano);
                vida.onMorte.AddListener(AoMorrer);
            }

            if (sr != null && config.Sprite != null)
                sr.sprite = config.Sprite;

            GameObject jogadorGo = GameObject.FindGameObjectWithTag("Jogador");
            if (jogadorGo != null)
                _jogador = jogadorGo.transform;

            _goBossHp = null;
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas != null)
            {
                Transform bossHpTrans = hudCanvas.transform.Find("BossHp");
                if (bossHpTrans != null)
                    _goBossHp = bossHpTrans.gameObject;
            }

            if (_goBossHp != null)
            {
                _goBossHp.SetActive(true);

                Transform nomeTrans = _goBossHp.transform.Find("NomeBoss");
                if (nomeTrans != null)
                {
                    TMP_Text txt = nomeTrans.GetComponent<TMP_Text>();
                    if (txt != null)
                        txt.text = config.NomeBoss;
                }

                Transform sliderTrans = _goBossHp.transform.Find("SliderBoss");
                if (sliderTrans != null)
                {
                    _sliderBoss = sliderTrans.GetComponent<Slider>();
                }
            }
        }

        /// <inheritdoc/>
        protected override void AoAtualizar()
        {
            if (config == null || _jogador == null) return;

            MoverLateralmente();
            ProcessarAtaque();
            VerificarFase();
        }

        private void MoverLateralmente()
        {
            if (config.Fases == null || _faseAtual >= config.Fases.Length) return;

            float velocidadeFase = config.Fases[_faseAtual].Velocidade;

            Vector3 pos = transform.position;
            pos.x += _direcaoMovimento * velocidadeFase * Time.deltaTime;

            if (pos.x >= limiteDireito)
            {
                pos.x = limiteDireito;
                _direcaoMovimento = -1f;
            }
            else if (pos.x <= limiteEsquerdo)
            {
                pos.x = limiteEsquerdo;
                _direcaoMovimento = 1f;
            }

            transform.position = pos;
        }

        private void ProcessarAtaque()
        {
            if (config.Fases == null || _faseAtual >= config.Fases.Length) return;

            _timerAtaque -= Time.deltaTime;
            if (_timerAtaque <= 0f)
            {
                _timerAtaque = config.Fases[_faseAtual].CooldownAtaque;
                DispararLeque();
            }
        }

        private void DispararLeque()
        {
            if (projetilPrefab == null || config.Fases == null || _faseAtual >= config.Fases.Length) return;

            FaseBoss fase = config.Fases[_faseAtual];
            int count = fase.ProjeteisPorDisparo;
            float anguloTotal = fase.AnguloAbertura * (count - 1);
            float anguloInicial = -anguloTotal * 0.5f + 270f;

            Vector2 origem = pontosDisparo != null && pontosDisparo.Length > 0 && pontosDisparo[0] != null
                ? pontosDisparo[0].position
                : transform.position;

            for (int i = 0; i < count; i++)
            {
                float anguloAtual = anguloInicial + i * fase.AnguloAbertura;
                float rad = anguloAtual * Mathf.Deg2Rad;
                Vector2 direcao = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                GameObject projetilGo = Object.Instantiate(projetilPrefab, origem, Quaternion.identity);
                projetilGo.layer = LayerMask.NameToLayer("ProjetilInimigo");

                ComponenteProjetil compProjetil = projetilGo.GetComponent<ComponenteProjetil>();
                if (compProjetil != null)
                {
                    compProjetil.Disparar(direcao);
                    compProjetil.InicializarValores(config.DanoContato, fase.VelocidadeProjetil, 5f, false, LayerMask.GetMask("Default"));
                }
            }
        }

        private void VerificarFase()
        {
            if (config.Fases == null || config.Fases.Length == 0) return;
            if (vida == null) return;

            float pctVida = vida.VidaAtual / vida.VidaMaxima;

            int novaFase = _faseAtual;
            for (int i = config.Fases.Length - 1; i > _faseAtual; i--)
            {
                if (pctVida <= config.Fases[i].VidaMinimaPct)
                {
                    novaFase = i;
                    break;
                }
            }

            if (novaFase != _faseAtual)
            {
                _faseAtual = novaFase;
                _timerAtaque = 0f;

                if (efeito != null)
                    efeito.EmitirParticula(transform.position);
            }
        }

        private void AoReceberDano(float dano)
        {
            if (efeito != null)
                efeito.EmitirParticula(transform.position);

            if (_sliderBoss != null && vida != null)
                _sliderBoss.value = vida.VidaAtual / vida.VidaMaxima;
        }

        private void AoMorrer()
        {
            if (_goBossHp != null)
                _goBossHp.SetActive(false);

            if (efeito != null)
            {
                efeito.EmitirParticula(transform.position);
                efeito.EmitirParticula(transform.position + new Vector3(0.5f, 0f, 0f));
                efeito.EmitirParticula(transform.position + new Vector3(-0.5f, 0f, 0f));
            }

            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.VitoriaJogo();

            Destroy(gameObject, 0.2f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Jogador")) return;

            NaveJogadorRunner jogador = other.GetComponent<NaveJogadorRunner>();
            if (jogador != null)
                jogador.ReceberDano();
        }
    }
}
