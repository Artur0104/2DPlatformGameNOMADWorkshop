using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração base para entidades.
    /// Defina aqui os parâmetros comuns a todas as entidades do jogo.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoEntidade", menuName = "ZeroToHero/Configuração de Entidade", order = 1)]
    public class ConfiguracaoEntidade : ScriptableObject
    {
        [Header("Identificação")]
        [Tooltip("Nome de exibição da entidade (ex: 'Goblin', 'Torre de Fogo').")]
        public string nomeEntidade = "Nova Entidade";

        [Header("Atributos Base")]
        [Tooltip("Vida máxima da entidade.")]
        public float vidaMaxima = 100f;

        [Header("Alianças")]
        [Tooltip("Tags consideradas aliadas.")]
        public string[] tagsAliadas = { "Jogador" };

        [Tooltip("Tags consideradas inimigas.")]
        public string[] tagsInimigas = { "Inimigo" };
    }
}
