using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core
{
    [CreateAssetMenu(menuName = "InsaneOne/Core Data")]
    public sealed class CoreData : ScriptableObject
    {
        const string RepoName = "OlegDzhuraev";
        
        static CoreData instance;

        [Header("Features settings")]
        public TeamsSettings TeamsSettings;
        
        [Tooltip("Place here prefab, which will be used as UI fader.")]
        public GameObject UiFaderTpl;

        [Header("Setup Project Tool settings")]
        public List<LinkHolder> GitPackages = new List<LinkHolder>()
        {
            new LinkHolder() { Name = "Modifiers", Link = $"https://github.com/{RepoName}/Modifiers.git" },
            new LinkHolder() { Name = "Perseids Pooling", Link = $"https://github.com/{RepoName}/PerseidsPooling.git" },
            new LinkHolder() { Name = "NavMesh Avoidance", Link = $"https://github.com/{RepoName}/NavMeshAvoidance.git" },
            new LinkHolder() { Name = "Tags", Link = $"https://github.com/{RepoName}/Tags.git" },
            new LinkHolder() { Name = "Serialize Reference Drawer", Link = $"https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git" },
            new LinkHolder() { Name = "Toolbar Extender", Link = $"https://github.com/marijnz/unity-toolbar-extender.git" },
            new LinkHolder() { Name = "Tri Inspector", Link = $"https://github.com/codewriter-packages/Tri-Inspector.git" },
        };
        
        public List<LinkHolder> Packages = new List<LinkHolder>()
        {
            new LinkHolder() { Name = "Post Effects", Link = "com.unity.postprocessing" },
            new LinkHolder() { Name = "Recorder", Link = "com.unity.recorder" },
            new LinkHolder() { Name = "Cinemachine", Link = "com.unity.cinemachine" },
            new LinkHolder() { Name = "Animation Rigging", Link = "com.unity.animation.rigging" },
            new LinkHolder() { Name = "Terrain Tools", Link = "com.unity.terrain-tools" },
            new LinkHolder() { Name = "Input System", Link = "com.unity.inputsystem" },
        };
        
        public List<LinkHolder> AssetLinks = new List<LinkHolder>()
        {
            new LinkHolder() { Name = "DOTween", Link = "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676" },
            new LinkHolder() { Name = "More Effective Coroutines", Link = "https://assetstore.unity.com/packages/tools/animation/more-effective-coroutines-free-54975" },
            new LinkHolder() { Name = "ReWired", Link = "https://assetstore.unity.com/packages/tools/utilities/rewired-21676" },
            new LinkHolder() { Name = "Odin Inspector", Link = "https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041" },
            new LinkHolder() { Name = "vFolders 2", Link = "https://assetstore.unity.com/packages/tools/utilities/vfolders-2-255470" },
            new LinkHolder() { Name = "FMOD", Link = "https://www.fmod.com/unity" },
            new LinkHolder() { Name = "MicroSplat Terrain", Link = "https://assetstore.unity.com/packages/tools/terrain/microsplat-96478" },
            new LinkHolder() { Name = "NG Soft Shadows", Link = "https://assetstore.unity.com/packages/vfx/shaders/next-gen-soft-shadows-137380" },
            new LinkHolder() { Name = "Dynamic Decals", Link = "https://github.com/EricFreeman/DynamicDecals" },
            new LinkHolder() { Name = "Prototype Materials", Link = "https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127" },
            new LinkHolder() { Name = "Lit Particles Materials", Link = "https://assetstore.unity.com/packages/vfx/shaders/particle-shaders-vol-1-35069" },
            new LinkHolder() { Name = "Amplify Shader Editor", Link = "https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570" },
        };
        
        [Header("Debugging")]
        [Tooltip("If you don't want to see plugin logs, set this.")]
        public bool SuppressLogs;

        public static CoreData Load()
        {
            var isLoaded = TryLoad(out var result);

            if (!isLoaded)
                throw new NullReferenceException("Possible, InsaneOne.Core initialization was failed, no CoreData found!");
            
            return result;
        }

        public static bool TryLoad(out CoreData result)
        {
            if (!instance)
                instance = Resources.Load<CoreData>("InsaneOne/CoreData");

            if (!instance)
            {
                result = default;
                return false;
            }

            result = instance;
            return true;
        }

        /// <summary> For internal usage. </summary>
        public static void Log(string text)
        {
            var coreData = Resources.Load<CoreData>("InsaneOne/CoreData");

            if (coreData && coreData.SuppressLogs)
                return;

            Debug.Log("<b>InsaneOne.Core:</b> " + text);
        }
    }

    [Serializable]
    public struct LinkHolder
    {
        public string Name;
        public string Link;
    }
}