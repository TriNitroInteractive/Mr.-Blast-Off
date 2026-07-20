using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class BlastFontSetupUtility : EditorWindow
{
    [MenuItem("Tools/Mr. Blast Off/Apply Blast Font Everywhere")]
    public static void GenerateAndApplyFont()
    {
        string fontSdfPath = "Assets/Blast Font/Blast SDF.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontSdfPath);

        if (fontAsset == null)
        {
            Debug.Log("[BlastFontSetupUtility] Generating new TMP Font Asset for Blast.ttf...");
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Blast Font/Blast.ttf");
            if (sourceFont == null)
            {
                Debug.LogError("[BlastFontSetupUtility] Could not load font at Assets/Blast Font/Blast.ttf!");
                return;
            }

            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (fontAsset == null)
            {
                Debug.LogError("[BlastFontSetupUtility] Failed to create TMP_FontAsset from source font!");
                return;
            }

            // Save the asset
            AssetDatabase.CreateAsset(fontAsset, fontSdfPath);

            // Add atlas texture and material as sub-assets so they are saved correctly
            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = "Blast SDF Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }
            if (fontAsset.material != null)
            {
                fontAsset.material.name = "Blast SDF Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.LogFormat("[BlastFontSetupUtility] Successfully generated and saved Blast SDF at: {0}", fontSdfPath);
        }
        else
        {
            Debug.LogFormat("[BlastFontSetupUtility] Loaded existing Blast SDF from: {0}", fontSdfPath);
        }

        // Apply to all scenes
        string[] targetScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Preparation.unity",
            "Assets/Scenes/Kickoff.unity"
        };

        foreach (string scenePath in targetScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (scene.IsValid())
            {
                var textComponents = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
                int count = 0;
                foreach (var textComp in textComponents)
                {
                    if (textComp.gameObject.scene == scene)
                    {
                        Undo.RecordObject(textComp, "Update Font Asset");
                        textComp.font = fontAsset;
                        EditorUtility.SetDirty(textComp);
                        count++;
                    }
                }

                if (count > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.LogFormat("[BlastFontSetupUtility] Updated {0} TMPro Text components in scene '{1}' (including inactive ones)", count, scene.name);
                }
            }
            else
            {
                Debug.LogErrorFormat("[BlastFontSetupUtility] Could not open scene: {0}", scenePath);
            }
        }

        Debug.Log("[BlastFontSetupUtility] Font generation and global updating complete!");
    }
}
