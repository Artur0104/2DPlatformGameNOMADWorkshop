using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// Configuração de uma fase do boss (padrão de ataque).
    /// </summary>
    [System.Serializable]
    public struct FaseBoss
    {
        [SerializeField, Tooltip("Porcentagem de vida (0 a 1) para ativar esta fase. Ex: 0.5 = metade da vida.")]
        private float vidaMinimaPct;

        [SerializeField, Tooltip("Velocidade de movimento lateral do boss nesta fase.")]
        private float velocidade;

        [SerializeField, Tooltip("Intervalo entre ataques (segundos).")]
        private float cooldownAtaque;

        [SerializeField, Tooltip("Quantidade de projéteis por disparo.")]
        private int projeteisPorDisparo;

        [SerializeField, Tooltip("Ângulo de abertura do leque de disparo (graus).")]
        private float anguloAbertura;

        [SerializeField, Tooltip("Velocidade dos projéteis disparados.")]
        private float velocidadeProjetil;

        public float VidaMinimaPct => vidaMinimaPct;
        public float Velocidade => velocidade;
        public float CooldownAtaque => cooldownAtaque;
        public int ProjeteisPorDisparo => projeteisPorDisparo;
        public float AnguloAbertura => anguloAbertura;
        public float VelocidadeProjetil => velocidadeProjetil;
    }

    /// <summary>
    /// ScriptableObject de configuração do Boss do Infinite Runner.
    /// Define vida, dano de contato, fases de ataque e recompensa.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoBossRunner",
        menuName = "ZeroToHero/Configuração de Boss Runner", order = 9)]
    public class ConfiguracaoBossRunner : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField, Tooltip("Nome de exibição do boss.")]
        private string nomeBoss = "Boss";

        [SerializeField, Tooltip("Sprite do boss.")]
        private Sprite sprite;

        [Header("Atributos")]
        [SerializeField, Tooltip("Vida máxima do boss.")]
        private float vida = 300f;

        [SerializeField, Tooltip("Dano causado ao tocar o jogador.")]
        private float danoContato = 1f;

        [Header("Fases")]
        [SerializeField, Tooltip("Fases do boss. Cada fase define padrão de ataque.")]
        private FaseBoss[] fases = new FaseBoss[2];

        [Header("Recompensa")]
        [SerializeField, Tooltip("XP concedida ao derrotar o boss.")]
        private int xpDrop = 500;

        public string NomeBoss => nomeBoss;
        public Sprite Sprite => sprite;
        public float Vida => vida;
        public float DanoContato => danoContato;
        public FaseBoss[] Fases => fases;
        public int XpDrop => xpDrop;
    }
}
