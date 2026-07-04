using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração da nave do jogador no Infinite Runner.
    /// Define vida inicial, velocidade, dano base, cooldown de tiro e invencibilidade.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoNaveJogador",
        menuName = "ZeroToHero/Configuração de Nave Jogador", order = 7)]
    public class ConfiguracaoNaveJogador : ScriptableObject
    {
        [Header("Vida")]
        [SerializeField, Tooltip("Quantidade inicial de vidas do jogador.")]
        private int vidasIniciais = 3;

        [SerializeField, Tooltip("Tempo de invencibilidade após receber dano (segundos).")]
        private float tempoInvencibilidadeAposDano = 1.5f;

        [Header("Movimento")]
        [SerializeField, Tooltip("Velocidade de movimento da nave nas 4 direções.")]
        private float velocidade = 5f;

        [Header("Ataque Base")]
        [SerializeField, Tooltip("Dano base de cada projétil disparado.")]
        private float danoBase = 10f;

        [SerializeField, Tooltip("Intervalo entre disparos (segundos).")]
        private float cooldownTiro = 0.3f;

        [SerializeField, Tooltip("Velocidade dos projéteis disparados.")]
        private float velocidadeProjetil = 10f;

        public int VidasIniciais => vidasIniciais;
        public float TempoInvencibilidadeAposDano => tempoInvencibilidadeAposDano;
        public float Velocidade => velocidade;
        public float DanoBase => danoBase;
        public float CooldownTiro => cooldownTiro;
        public float VelocidadeProjetil => velocidadeProjetil;
    }
}
