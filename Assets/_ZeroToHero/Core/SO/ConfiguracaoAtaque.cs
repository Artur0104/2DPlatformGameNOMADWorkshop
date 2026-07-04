using UnityEngine;
using ZeroToHero.Core.Componentes;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração de ataque.
    /// Altere os valores no Inspector para ajustar dano, alcance, tipo, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoAtaque", menuName = "ZeroToHero/Configuração de Ataque", order = 3)]
    public class ConfiguracaoAtaque : ScriptableObject
    {
        [Header("Tipo de Ataque")]
        [Tooltip("Corpo a corpo: dano em área próxima. Distância: dispara projétil. Área: dano circular ao redor.")]
        public ComponenteAtaque.TipoAtaque tipoAtaque = ComponenteAtaque.TipoAtaque.CorpoACorpo;

        [Header("Atributos")]
        [Tooltip("Dano causado por cada ataque.")]
        public float dano = 10f;

        [Tooltip("Alcance do ataque (raio para corpo a corpo e área).")]
        public float alcance = 1.5f;

        [Tooltip("Tempo de recarga entre ataques (segundos).")]
        public float cooldown = 0.5f;

        [Tooltip("Força de empurrão aplicada ao alvo atingido.")]
        public float forcaEmpurrao = 5f;

        [Header("Projétil (ataque à distância)")]
        [Tooltip("Prefab do projétil a ser instanciado.")]
        public GameObject projetilPrefab;

        [Header("Alvos")]
        [Tooltip("LayerMask: camadas que podem ser atingidas pelo ataque.")]
        public LayerMask camadaAlvo = -1;
    }
}
