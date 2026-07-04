using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZeroToHero.Core.Editor
{
    /// <summary>
    /// Configurador de projeto para o ZeroToHero.
    /// Executado na primeira abertura do projeto ou via menu ZeroToHero > Configurar Projeto.
    /// Idempotente — pode ser executado múltiplas vezes sem duplicar ou quebrar configurações.
    /// </summary>
    [InitializeOnLoad]
    public static class ConfiguradorProjeto
    {
        private const string CHAVE_SETUP = "ZeroToHero_SetupComplete";
        private const string TAGMANAGER_PATH = "ProjectSettings/TagManager.asset";
        private const string QUALITY_SETTINGS_PATH = "ProjectSettings/QualitySettings.asset";
        private const string PHYSICS2D_PATH = "ProjectSettings/Physics2DSettings.asset";
        private const string GRAPHICS_SETTINGS_PATH = "ProjectSettings/GraphicsSettings.asset";

        private const string URP_PIPELINE_GUID = "681886c5eb7344803b6206f758bf0b1c";
        private const string URP_GLOBALSETTINGS_GUID = "93b439a37f63240aca3dd4e01d978a9f";

        private static readonly string[] TAGS_NECESSARIAS = {
            "Jogador", "Inimigo", "Base", "Checkpoint",
            "ProjetilJogador", "ProjetilInimigo", "ProjetilTorre"
        };

        private static readonly string[] LAYERS_NECESSARIAS = {
            "Chao", "Caminho", "Jogador", "Inimigo",
            "Bordas", "Inimigos", "ProjetilJogador", "ProjetilInimigo"
        };

        private static readonly (string path, string guid, bool enabled)[] CENAS = {
            ("Assets/Scenes/MenuPrincipal.unity",   "8c9cfa26abfee488c85f1582747f6a02", true),
            ("Assets/Scenes/Plataforma.unity",       "76e07d63a01f21c4f84728e16ca0a77a", true),
            ("Assets/Scenes/TowerDefense.unity",     "ff33a224b00cdf047a4b7b147732f5d6", true),
            ("Assets/Scenes/InfiniteRunner.unity",   "97c474ad0aecc2d4c901ad39912c72e4", true),
        };

        static ConfiguradorProjeto()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool("ZeroToHero_InitCheckDone", false))
                {
                    SessionState.SetBool("ZeroToHero_InitCheckDone", true);
                    if (!EditorPrefs.GetBool(CHAVE_SETUP, false))
                    {
                        ConfigurarTudo(silencioso: true);
                    }
                }
            };
        }

        [MenuItem("ZeroToHero/Configurar Projeto", priority = 100)]
        public static void ConfigurarViaMenu()
        {
            ConfigurarTudo(silencioso: false);
        }

        [MenuItem("ZeroToHero/Resetar Configuração (Debug)", priority = 200)]
        public static void ResetarConfiguracao()
        {
            EditorPrefs.DeleteKey(CHAVE_SETUP);
            Debug.Log("[ZeroToHero] Chave de setup removida. Na próxima abertura do projeto, o configurador rodará novamente.");
        }

        private static void ConfigurarTudo(bool silencioso)
        {
            int passosOk = 0;
            int passosErro = 0;

            LogPasso(silencioso, "Iniciando configuração do projeto ZeroToHero...");

            if (VerificarVersaoUnity(silencioso)) passosOk++; else passosErro++;
            if (VerificarURP(silencioso)) passosOk++; else passosErro++;
            if (AdicionarTags(silencioso)) passosOk++; else passosErro++;
            if (AdicionarLayers(silencioso)) passosOk++; else passosErro++;
            if (AdicionarCenasBuild(silencioso)) passosOk++; else passosErro++;
            if (ConfigurarPipelineURP(silencioso)) passosOk++; else passosErro++;

            EditorPrefs.SetBool(CHAVE_SETUP, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LogPasso(silencioso, string.Format("[ZeroToHero] Configuração concluída: {0} OK, {1} falhas.", passosOk, passosErro));

            if (!silencioso && passosErro == 0)
            {
                EditorUtility.DisplayDialog("ZeroToHero",
                    "Projeto configurado com sucesso!\n\n" +
                    string.Format("{0} tags criadas\n{1} layers criadas\n4 cenas no Build Settings\nURP configurado",
                        TAGS_NECESSARIAS.Length, LAYERS_NECESSARIAS.Length),
                    "OK");
            }
        }

        private static bool VerificarVersaoUnity(bool silencioso)
        {
#if UNITY_2022_3_OR_NEWER && !UNITY_6000_0_OR_NEWER
            LogPasso(silencioso, "[ZeroToHero] AVISO: Versão da Unity anterior à 6. Alguns scripts podem não compilar (FindFirstObjectByType requer Unity 2023.1+).");
#endif
            return true;
        }

        private static bool VerificarURP(bool silencioso)
        {
            if (GraphicsSettings.defaultRenderPipeline != null) return true;

            var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                AssetDatabase.GUIDToAssetPath(URP_PIPELINE_GUID));

            if (urpAsset != null) return true;

            LogPasso(silencioso, "[ZeroToHero] ERRO: Universal Render Pipeline não encontrado. Crie o projeto com o template '2D (URP)'.");
            return false;
        }

        private static bool AdicionarTags(bool silencioso)
        {
            var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(TAGMANAGER_PATH);
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                LogPasso(silencioso, "[ZeroToHero] ERRO: TagManager.asset não encontrado.");
                return false;
            }

            var so = new SerializedObject(tagManagerAssets[0]);
            var tagsProp = so.FindProperty("tags");
            if (tagsProp == null)
            {
                LogPasso(silencioso, "[ZeroToHero] ERRO: propriedade 'tags' não encontrada no TagManager.");
                return false;
            }

            int adicionadas = 0;
            foreach (var tag in TAGS_NECESSARIAS)
            {
                if (TagExiste(tagsProp, tag)) continue;

                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                adicionadas++;
            }

            if (adicionadas > 0)
            {
                so.ApplyModifiedProperties();
                LogPasso(silencioso, string.Format("[ZeroToHero] {0} tag(s) adicionada(s).", adicionadas));
            }
            else
            {
                LogPasso(silencioso, "[ZeroToHero] Todas as tags já existiam.");
            }

            return true;
        }

        private static bool TagExiste(SerializedProperty tagsProp, string tag)
        {
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    return true;
            }
            return false;
        }

        private static bool AdicionarLayers(bool silencioso)
        {
            var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(TAGMANAGER_PATH);
            if (tagManagerAssets == null || tagManagerAssets.Length == 0) return false;

            var so = new SerializedObject(tagManagerAssets[0]);
            var layersProp = so.FindProperty("layers");
            if (layersProp == null) return false;

            var usuarios = new Dictionary<string, int>();
            foreach (var layer in LAYERS_NECESSARIAS)
            {
                int indice = EncontrarIndiceLayer(layersProp, layer);
                if (indice >= 0)
                {
                    usuarios[layer] = indice;
                    continue;
                }

                indice = EncontrarVagaLayer(layersProp);
                if (indice < 0)
                {
                    LogPasso(silencioso, string.Format("[ZeroToHero] ERRO: sem vagas para layer '{0}'.", layer));
                    return false;
                }

                layersProp.GetArrayElementAtIndex(indice).stringValue = layer;
                usuarios[layer] = indice;
            }

            so.ApplyModifiedProperties();

            if (usuarios.Any(kv => kv.Value >= 0))
                LogPasso(silencioso, string.Format("[ZeroToHero] Layers configuradas: {0}",
                    string.Join(", ", usuarios.Select(kv => string.Format("{0}={1}", kv.Key, kv.Value)))));

            return true;
        }

        private static int EncontrarIndiceLayer(SerializedProperty layersProp, string nome)
        {
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                if (layersProp.GetArrayElementAtIndex(i).stringValue == nome)
                    return i;
            }
            return -1;
        }

        private static int EncontrarVagaLayer(SerializedProperty layersProp)
        {
            for (int i = 8; i <= 31; i++)
            {
                var el = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                    return i;
            }
            return -1;
        }

        private static bool AdicionarCenasBuild(bool silencioso)
        {
            var cenasExistentes = new HashSet<string>(
                EditorBuildSettings.scenes
                    .Where(s => s != null)
                    .Select(s => s.path));

            var novas = new List<EditorBuildSettingsScene>();

            foreach (var (path, guid, enabled) in CENAS)
            {
                if (cenasExistentes.Contains(path)) continue;

                var cenaAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (cenaAsset == null)
                {
                    LogPasso(silencioso, string.Format("[ZeroToHero] AVISO: cena não encontrada: {0}", path));
                    continue;
                }

                novas.Add(new EditorBuildSettingsScene(path, enabled));
            }

            if (novas.Count > 0)
            {
                var lista = EditorBuildSettings.scenes.ToList();
                lista.AddRange(novas);
                EditorBuildSettings.scenes = lista.ToArray();
                LogPasso(silencioso, string.Format("[ZeroToHero] {0} cena(s) adicionada(s) ao Build Settings.", novas.Count));
            }
            else
            {
                LogPasso(silencioso, "[ZeroToHero] Todas as cenas já estão no Build Settings.");
            }

            return true;
        }

        private static bool ConfigurarPipelineURP(bool silencioso)
        {
            var urpPath = AssetDatabase.GUIDToAssetPath(URP_PIPELINE_GUID);
            if (string.IsNullOrEmpty(urpPath))
            {
                LogPasso(silencioso, "[ZeroToHero] AVISO: asset URP não encontrado. Pipeline não configurado.");
                return false;
            }

            var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(urpPath);
            if (urpAsset == null) return false;

            bool alterado = false;

            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                GraphicsSettings.defaultRenderPipeline = urpAsset;
                alterado = true;
            }

            var qualityAssets = AssetDatabase.LoadAllAssetsAtPath(QUALITY_SETTINGS_PATH);
            if (qualityAssets != null && qualityAssets.Length > 0)
            {
                var so = new SerializedObject(qualityAssets[0]);
                var qualidades = so.FindProperty("m_QualitySettings");
                if (qualidades != null)
                {
                    for (int i = 0; i < qualidades.arraySize; i++)
                    {
                        var qualidade = qualidades.GetArrayElementAtIndex(i);
                        var pipelineProp = qualidade.FindPropertyRelative("customRenderPipeline");
                        if (pipelineProp != null && pipelineProp.objectReferenceValue == null)
                        {
                            pipelineProp.objectReferenceValue = urpAsset;
                            alterado = true;
                        }
                    }
                }
                if (alterado) so.ApplyModifiedProperties();
            }

            if (alterado)
                LogPasso(silencioso, "[ZeroToHero] Universal Render Pipeline configurado.");
            else
                LogPasso(silencioso, "[ZeroToHero] URP já estava configurado.");

            return true;
        }

        private static void LogPasso(bool silencioso, string mensagem)
        {
            if (silencioso)
                Debug.Log(mensagem);
            else
                Debug.Log(mensagem);
        }
    }
}
