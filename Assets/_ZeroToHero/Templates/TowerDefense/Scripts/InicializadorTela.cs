using UnityEngine;
using UnityEngine.UI;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Auto-configura botões de telas overlay (Vitória, Game Over, Pause).
    /// Coloque este script no GameObject raiz de cada tela canvas.
    /// Em OnEnable(), busca os botões por nome e conecta os callbacks.
    /// </summary>
    public class InicializadorTela : MonoBehaviour
    {
        private void OnEnable()
        {
            ControladorTelasFinais ctrl = FindFirstObjectByType<ControladorTelasFinais>();
            if (ctrl == null) return;

            ConectarBotao(transform, "Btn_Reiniciar", ctrl.Reiniciar);
            ConectarBotao(transform, "Btn_Menu", ctrl.IrParaMenu);
            ConectarBotao(transform, "Btn_Retomar", ctrl.Retomar);
        }

        private void ConectarBotao(Transform raiz, string nome, UnityEngine.Events.UnityAction callback)
        {
            Button btn = BuscarBotao(raiz, nome);
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(callback);
            }
        }

        private Button BuscarBotao(Transform t, string nome)
        {
            if (t.name == nome)
                return t.GetComponent<Button>();

            for (int i = 0; i < t.childCount; i++)
            {
                Button found = BuscarBotao(t.GetChild(i), nome);
                if (found != null) return found;
            }
            return null;
        }
    }
}
