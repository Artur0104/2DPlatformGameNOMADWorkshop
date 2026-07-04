using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração de ondas (Tower Defense e afins).
    /// Define quais inimigos, quantos, com qual intervalo e recompensa.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoOnda", menuName = "ZeroToHero/Configuração de Onda", order = 4)]
    public class ConfiguracaoOnda : ScriptableObject
    {
        [Header("Inimigos")]
        [Tooltip("Lista de prefabs de inimigos que podem spawnar nesta onda. O spawn escolhe aleatoriamente.")]
        public GameObject[] prefabsInimigos;

        [Tooltip("Quantidade total de inimigos a spawnar nesta onda.")]
        public int quantidade = 10;

        [Tooltip("Intervalo em segundos entre o spawn de cada inimigo.")]
        public float intervaloSpawn = 1f;

        [Header("Recompensa")]
        [Tooltip("Moedas concedidas ao completar esta onda.")]
        public int recompensa = 50;
    }
}
