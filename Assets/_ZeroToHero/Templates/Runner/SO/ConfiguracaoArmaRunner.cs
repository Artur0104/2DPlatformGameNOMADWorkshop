using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// Padrões de disparo das armas do Infinite Runner.
    /// </summary>
    public enum PadraoArmaRunner
    {
        /// <summary>Tiro simples pra cima.</summary>
        Frente,
        /// <summary>Leque de projéteis em arco.</summary>
        Arco,
        /// <summary>Vários tiros em sequência rápida.</summary>
        Rajada,
        /// <summary>Projétil que persegue o inimigo mais próximo.</summary>
        Teleguiado,
        /// <summary>Cadência de tiro extremamente alta.</summary>
        Rapido
    }

    /// <summary>
    /// ScriptableObject de configuração de arma do Infinite Runner.
    /// Define o padrão de disparo, dano, cooldown e duração.
    /// Armas com duracao = 0 são permanentes (arma base).
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoArmaRunner",
        menuName = "ZeroToHero/Configuração de Arma Runner", order = 11)]
    public class ConfiguracaoArmaRunner : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField, Tooltip("Nome de exibição da arma.")]
        private string nomeArma = "Arma";

        [SerializeField, Tooltip("Ícone da arma para UI de level up.")]
        private Sprite icone;

        [Header("Padrão")]
        [SerializeField, Tooltip("Padrão de disparo da arma.")]
        private PadraoArmaRunner padrao = PadraoArmaRunner.Frente;

        [Header("Atributos")]
        [SerializeField, Tooltip("Dano por projétil.")]
        private float dano = 10f;

        [SerializeField, Tooltip("Intervalo entre disparos (segundos).")]
        private float cooldown = 0.3f;

        [SerializeField, Tooltip("Velocidade dos projéteis disparados.")]
        private float velocidadeProjetil = 10f;

        [SerializeField, Tooltip("Quantidade de projéteis por disparo.")]
        private int projeteisPorDisparo = 1;

        [Header("Específicos de Padrão")]
        [SerializeField, Tooltip("Ângulo de abertura do leque (Arco).")]
        private float anguloAbertura = 30f;

        [SerializeField, Tooltip("Intervalo entre projéteis da rajada (Rajada).")]
        private float intervaloRajada = 0.05f;

        [Header("Duração")]
        [SerializeField, Tooltip("Duração da arma em segundos. 0 = arma permanente (base).")]
        private float duracao = 0f;

        public string NomeArma => nomeArma;
        public Sprite Icone => icone;
        public PadraoArmaRunner Padrao => padrao;
        public float Dano => dano;
        public float Cooldown => cooldown;
        public float VelocidadeProjetil => velocidadeProjetil;
        public int ProjeteisPorDisparo => projeteisPorDisparo;
        public float AnguloAbertura => anguloAbertura;
        public float IntervaloRajada => intervaloRajada;
        public float Duracao => duracao;

        /// <summary>
        /// True se esta arma é temporária (tem duração limitada).
        /// </summary>
        public bool Temporaria
        {
            get { return duracao > 0f; }
        }
    }
}
