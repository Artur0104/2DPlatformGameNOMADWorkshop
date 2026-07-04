using UnityEngine;

namespace ZeroToHero.Core.Base
{
    /// <summary>
    /// Singleton genérico para gerenciadores globais.
    /// Use herdando: public class MeuGerenciador : GerenciadorBase&lt;MeuGerenciador&gt;
    /// Acesse de qualquer lugar: MeuGerenciador.Instancia
    /// </summary>
    public class GerenciadorBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instancia;
        public static T Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = FindAnyObjectByType<T>();
                    if (_instancia == null)
                    {
                        Debug.LogWarning($"[GerenciadorBase] Nenhum objeto do tipo {typeof(T).Name} encontrado na cena.");
                    }
                }
                return _instancia;
            }
            private set => _instancia = value;
        }

        [SerializeField, Tooltip("Se ativado, este objeto persiste entre cenas (DontDestroyOnLoad).")]
        protected bool persistirEntreCenas = true;

        protected virtual void Awake()
        {
            if (_instancia != null && _instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this as T;

            if (persistirEntreCenas)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
