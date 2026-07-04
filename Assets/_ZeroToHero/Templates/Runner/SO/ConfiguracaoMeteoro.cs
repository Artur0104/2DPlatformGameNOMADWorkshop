using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração de meteoros do Infinite Runner.
    /// Meteoros são obstáculos destrutíveis que descem do topo em linha reta.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoMeteoro",
        menuName = "ZeroToHero/Configuração de Meteoro", order = 10)]
    public class ConfiguracaoMeteoro : ScriptableObject
    {
        [Header("Atributos")]
        [SerializeField, Tooltip("Vida do meteoro (pode ser destruído a tiros).")]
        private float vida = 15f;

        [SerializeField, Tooltip("Velocidade de descida do meteoro.")]
        private float velocidade = 3f;

        [SerializeField, Tooltip("Dano causado ao tocar o jogador.")]
        private float dano = 1f;

        [Header("Spawn")]
        [SerializeField, Tooltip("Peso relativo na pool de spawn.")]
        private float pesoSpawn = 20f;

        [SerializeField, Tooltip("Tempo mínimo de jogo (segundos) antes de meteoros começarem a spawnar.")]
        private float tempoMinimoSpawn = 10f;

        public float Vida => vida;
        public float Velocidade => velocidade;
        public float Dano => dano;
        public float PesoSpawn => pesoSpawn;
        public float TempoMinimoSpawn => tempoMinimoSpawn;
    }
}
