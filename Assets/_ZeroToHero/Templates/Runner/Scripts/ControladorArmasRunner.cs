using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Controlador de armas do jogador no Infinite Runner.
    /// Gerencia a arma ativa: arma base é permanente, armas temporárias substituem a base.
    /// Delega o disparo ao DisparadorArmasRunner.
    /// </summary>
    public class ControladorArmasRunner : MonoBehaviour
    {
        [Header("Arma Base")]
        [SerializeField, Tooltip("Configuração da arma base (permanente).")]
        private ConfiguracaoArmaRunner armaBase;

        [Header("Referências")]
        [SerializeField, Tooltip("Prefab do projétil a ser instanciado.")]
        private GameObject projetilPrefab;

        [SerializeField, Tooltip("Disparador de armas (pode estar no mesmo GameObject).")]
        private DisparadorArmasRunner disparador;

        [Header("Multiplicadores Globais")]
        [SerializeField, Tooltip("Multiplicador de dano global aplicado a todos os disparos.")]
        private float multiplicadorDanoGlobal = 1f;

        private ConfiguracaoArmaRunner _armaAtual;
        private float _timerArmaTemporaria;
        private float _timerCooldown;
        private bool _temArmaTemporaria;

        public ConfiguracaoArmaRunner ArmaAtual
        {
            get { return _armaAtual; }
        }

        public float MultiplicadorDanoGlobal
        {
            get { return multiplicadorDanoGlobal; }
            set { multiplicadorDanoGlobal = value; }
        }

        private void Start()
        {
            if (disparador == null)
                disparador = GetComponent<DisparadorArmasRunner>();

            _armaAtual = armaBase;
            _temArmaTemporaria = false;
            _timerCooldown = 0f;
            _timerArmaTemporaria = 0f;
        }

        private void Update()
        {
            if (_armaAtual == null) return;

            if (_temArmaTemporaria)
            {
                _timerArmaTemporaria -= Time.deltaTime;
                if (_timerArmaTemporaria <= 0f)
                {
                    RestaurarArmaBase();
                }
            }

            _timerCooldown -= Time.deltaTime;
            if (_timerCooldown <= 0f)
            {
                float cooldownAtual = _armaAtual.Cooldown;
                NaveJogadorRunner nave = GetComponent<NaveJogadorRunner>();
                if (nave != null && nave.FrequenciaTiroBoost > 0f)
                    cooldownAtual /= nave.FrequenciaTiroBoost;
                _timerCooldown = cooldownAtual;

                Disparar();
            }
        }

        /// <summary>
        /// Equipa uma arma temporária que substitui a base durante sua duração.
        /// </summary>
        /// <param name="novaArma">Configuração da arma temporária.</param>
        public void EquiparArmaTemporaria(ConfiguracaoArmaRunner novaArma)
        {
            if (novaArma == null) return;

            _armaAtual = novaArma;
            _temArmaTemporaria = true;
            _timerArmaTemporaria = novaArma.Duracao;
            _timerCooldown = 0f;
        }

        /// <summary>
        /// Restaura a arma base, cancelando qualquer arma temporária ativa.
        /// </summary>
        public void RestaurarArmaBase()
        {
            _armaAtual = armaBase;
            _temArmaTemporaria = false;
            _timerArmaTemporaria = 0f;
            _timerCooldown = 0f;
        }

        private void Disparar()
        {
            if (disparador == null || projetilPrefab == null || _armaAtual == null) return;

            Vector2 origem = transform.position;
            Transform pontoDisparo = transform.Find("PontoDisparo");
            if (pontoDisparo != null)
                origem = pontoDisparo.position;

            NaveJogadorRunner nave = GetComponent<NaveJogadorRunner>();
            float danoMult = multiplicadorDanoGlobal;
            if (nave != null)
                danoMult = nave.DanoAtual / nave.DanoBase;

            disparador.Disparar(_armaAtual, origem, projetilPrefab, danoMult);
        }
    }
}
