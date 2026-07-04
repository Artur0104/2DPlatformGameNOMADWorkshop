using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// Tipos de inimigos do Infinite Runner.
    /// </summary>
    public enum TipoInimigoRunner
    {
        Pequeno,
        Medio,
        Grande
    }

    /// <summary>
    /// ScriptableObject de configuração de inimigos do Infinite Runner.
    /// Define atributos, comportamento, recompensas e peso de spawn.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoInimigoRunner",
        menuName = "ZeroToHero/Configuração de Inimigo Runner", order = 8)]
    public class ConfiguracaoInimigoRunner : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField, Tooltip("Nome de exibição do inimigo.")]
        private string nomeInimigo = "Inimigo";

        [SerializeField, Tooltip("Tamanho/categoria do inimigo.")]
        private TipoInimigoRunner tipo = TipoInimigoRunner.Pequeno;

        [SerializeField, Tooltip("Sprite do inimigo.")]
        private Sprite sprite;

        [Header("Atributos")]
        [SerializeField, Tooltip("Vida máxima do inimigo.")]
        private float vida = 10f;

        [SerializeField, Tooltip("Velocidade de descida.")]
        private float velocidade = 2f;

        [SerializeField, Tooltip("Dano causado ao tocar o jogador (sempre 1 por hit).")]
        private float danoContato = 1f;

        [Header("Comportamento")]
        [SerializeField, Tooltip("Força do desvio lateral em direção ao jogador.")]
        private float desvioX = 0.5f;

        [SerializeField, Tooltip("Se true, este inimigo atira projéteis.")]
        private bool atiraProjeteis = false;

        [SerializeField, Tooltip("Intervalo entre disparos (segundos).")]
        private float cooldownTiro = 2f;

        [SerializeField, Tooltip("Velocidade dos projéteis disparados.")]
        private float velocidadeProjetil = 4f;

        [SerializeField, Tooltip("Dano dos projéteis disparados.")]
        private float danoProjetil = 1f;

        [Header("Recompensa")]
        [SerializeField, Tooltip("Quantidade de XP concedida ao morrer.")]
        private int xpDrop = 5;

        [SerializeField, Tooltip("Chance (0 a 1) de dropar uma vida ao morrer.")]
        [Range(0f, 1f)]
        private float chanceDropVida = 0.1f;

        [SerializeField, Tooltip("Chance (0 a 1) de dropar XP ao morrer.")]
        [Range(0f, 1f)]
        private float chanceDropXp = 1f;

        [Header("Spawn")]
        [SerializeField, Tooltip("Peso relativo na pool de spawn. Maior = mais frequente.")]
        private float pesoSpawn = 50f;

        [SerializeField, Tooltip("Tempo mínimo de jogo (segundos) antes deste inimigo começar a spawnar.")]
        private float tempoMinimoSpawn = 0f;

        public string NomeInimigo => nomeInimigo;
        public TipoInimigoRunner Tipo => tipo;
        public Sprite Sprite => sprite;
        public float Vida => vida;
        public float Velocidade => velocidade;
        public float DanoContato => danoContato;
        public float DesvioX => desvioX;
        public bool AtiraProjeteis => atiraProjeteis;
        public float CooldownTiro => cooldownTiro;
        public float VelocidadeProjetil => velocidadeProjetil;
        public float DanoProjetil => danoProjetil;
        public int XpDrop => xpDrop;
        public float ChanceDropVida => chanceDropVida;
        public float ChanceDropXp => chanceDropXp;
        public float PesoSpawn => pesoSpawn;
        public float TempoMinimoSpawn => tempoMinimoSpawn;
    }
}
