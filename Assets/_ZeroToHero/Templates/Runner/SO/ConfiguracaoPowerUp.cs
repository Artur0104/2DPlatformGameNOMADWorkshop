using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// Tipos de power-up disponíveis no level up.
    /// </summary>
    public enum TipoPowerUp
    {
        /// <summary>Equipa uma arma temporária.</summary>
        ArmaTemporaria,
        /// <summary>Imune a dano por tempo limitado.</summary>
        Invulnerabilidade,
        /// <summary>Aumenta um status permanentemente na run.</summary>
        BoostStatus
    }

    /// <summary>
    /// Tipos de boost de status disponíveis.
    /// </summary>
    public enum TipoBoostStatus
    {
        /// <summary>Adiciona vidas extras.</summary>
        Vida,
        /// <summary>Aumenta a velocidade de movimento.</summary>
        Velocidade,
        /// <summary>Aumenta o dano dos projéteis.</summary>
        Dano,
        /// <summary>Aumenta a frequência de tiro (divisor do cooldown).</summary>
        FrequenciaTiro
    }

    /// <summary>
    /// ScriptableObject de configuração de power-up do Infinite Runner.
    /// Usado na pool de opções do popup de level up.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoPowerUp",
        menuName = "ZeroToHero/Configuração de Power-Up", order = 12)]
    public class ConfiguracaoPowerUp : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField, Tooltip("Nome de exibição do power-up.")]
        private string nomePowerUp = "Power-Up";

        [SerializeField, Tooltip("Ícone do power-up para UI.")]
        private Sprite icone;

        [SerializeField, TextArea(2, 4), Tooltip("Descrição do power-up exibida no popup de level up.")]
        private string descricao;

        [SerializeField, Tooltip("Tipo de power-up.")]
        private TipoPowerUp tipo = TipoPowerUp.ArmaTemporaria;

        [Header("Arma Temporária")]
        [SerializeField, Tooltip("Configuração da arma a equipar (se Tipo = ArmaTemporaria).")]
        private ConfiguracaoArmaRunner arma;

        [Header("Duração (para Arma e Invulnerabilidade)")]
        [SerializeField, Tooltip("Duração do efeito em segundos.")]
        private float duracao = 10f;

        [Header("Boost de Status (duracao > 0 = temporário, = 0 = permanente)")]
        [SerializeField, Tooltip("Tipo de status a aumentar (se Tipo = BoostStatus).")]
        private TipoBoostStatus tipoBoost = TipoBoostStatus.Vida;

        [SerializeField, Tooltip("Valor do aumento (ex: 1 vida, 1.5x velocidade, +5 dano).")]
        private float valorBoost = 1f;

        public string NomePowerUp => nomePowerUp;
        public Sprite Icone => icone;
        public string Descricao => descricao;
        public TipoPowerUp Tipo => tipo;
        public ConfiguracaoArmaRunner Arma => arma;
        public float Duracao => duracao;
        public TipoBoostStatus TipoBoost => tipoBoost;
        public float ValorBoost => valorBoost;
    }
}
