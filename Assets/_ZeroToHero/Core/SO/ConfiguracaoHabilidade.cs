using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração de habilidade.
    /// Define nome, ícone, cooldown, custo e estado inicial de desbloqueio.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoHabilidade", menuName = "ZeroToHero/Configuração de Habilidade", order = 5)]
    public class ConfiguracaoHabilidade : ScriptableObject
    {
        [Header("Identificação")]
        [Tooltip("Nome da habilidade (exibido na HUD).")]
        public string nome = "Habilidade";

        [Tooltip("Ícone da habilidade (exibido na HUD).")]
        public Sprite icone;

        [Header("Mecânica")]
        [Tooltip("Tempo de recarga da habilidade em segundos.")]
        public float cooldown = 5f;

        [Tooltip("Custo de mana/energia/vida. 0 = sem custo.")]
        public float custo = 0f;

        [Tooltip("Se a habilidade já começa desbloqueada.")]
        public bool desbloqueadaInicial = true;
    }
}
