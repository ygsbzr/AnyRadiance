using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

namespace AnyRadiance
{
    internal class AnyRadiance : Mod
    {
        internal static AnyRadiance Instance { get; private set; }

        public Dictionary<string, AudioClip> AudioClips = new();
        public Dictionary<string, GameObject> GameObjects = new();
        public Dictionary<string, ParticleSystem> Particles = new();

        public static LocalSettings Settings = new();

        public override ModSettings SaveSettings { get => Settings; set => Settings = (LocalSettings)value; }

        public AnyRadiance() : base("Any Radiance") { }

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;
            
            Unload();


            ModHooks.Instance.BeforeSavegameSaveHook += BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook += SaveGame;
            ModHooks.Instance.SavegameSaveHook += SaveGameSave;
            ModHooks.Instance.NewGameHook += AddComponent;
            ModHooks.Instance.LanguageGetHook += OnLangGet;
            ModHooks.Instance.SetPlayerVariableHook += SetVariableHook;
            ModHooks.Instance.GetPlayerVariableHook += GetVariableHook;
            LoadAssets();
        }

        private object SetVariableHook(Type t, string key, object obj)
        {
            if (key != "statueStateAnyRad")
                return obj;

            var completion = (BossStatue.Completion)obj;

            Settings.Completion = completion;

            completion.usingAltVersion = false;

            return completion;
        }

        private object GetVariableHook(Type type, string name, object orig)
        {
            return name == "statueStateAnyRad" ? Settings.Completion : orig;
        }

        private string OnLangGet(string key, string sheetTitle)
        {
            switch (key)
            {
                case "ABSOLUTE_RADIANCE_SUPER":
                    return PlayerData.instance.statueStateRadiance.usingAltVersion ||
                            Settings.InBossDoor && BossSequenceController.IsInSequence
                        ? "Any"
                        : Language.Language.GetInternal(key,sheetTitle);
                case "ANY_RAD_NAME":
                    return "Any Radiance";
                case "ANY_RAD_DESC":
                    return "Why.";
                case "GODSEEKER_ANYRAD_STATUE":
                    return sheetTitle == "CP3" ? "k" : Language.Language.GetInternal(key, sheetTitle);
            }

            return Language.Language.GetInternal(key, sheetTitle);
        }

        private static void BeforeSaveGameSave(SaveGameData data)
        {
            Settings.UsingAltVersion = PlayerData.instance.statueStateRadiance.usingAltVersion;

            PlayerData.instance.statueStateRadiance.usingAltVersion = false;
        }

        private void SaveGame(SaveGameData data)
        {
            SaveGameSave();
            AddComponent();
        }

        private static void SaveGameSave(int id = 0)
        {
            PlayerData.instance.statueStateRadiance.usingAltVersion = Settings.UsingAltVersion;
        }

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<RadianceFinder>();
            GameManager.instance.gameObject.AddComponent<SceneLoader>();
        }



        private void LoadAssets()
        {
            string bundlename= "anyradwin";
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    bundlename = "anyradwin";
                    break;
                case OperatingSystemFamily.Linux:
                    bundlename = "anyradlin";
                    break;
                case OperatingSystemFamily.MacOSX:
                    bundlename = "anyradmac";
                    break;
                default:
                    Log("ERROR UNSUPPORTED SYSTEM: " + SystemInfo.operatingSystemFamily);
                    return;
            }
           Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                Log(resourceName);
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) continue;

                    if (resourceName.Contains(bundlename))
                    {
                        Log("Loading bundle: " + bundlename);
                        byte[] buffer=new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        AssetBundle bundle = AssetBundle.LoadFromMemory(buffer);
                        stream.Dispose();
            
                        
 
                        if (bundle == null)
                        {
                            Log("Bundle is null");
                        }
                       if(bundle != null)
                        {
                            foreach (var clip in bundle.LoadAllAssets<AudioClip>())
                            {
                                AudioClips.Add(clip.name, clip);
                            }

                            foreach (var gameObject in bundle.LoadAllAssets<GameObject>())
                            {
                                GameObjects.Add(gameObject.name, gameObject);
                            }
                        }
                    }
                    //else if (resourceName.Contains("truerad"))
                    //{
                    //    var bundle = AssetBundle.LoadFromStream(stream);
                    //    Bundles.Add(bundle.name, bundle);
                    //}

                }
            }
        }

        public void Unload()
        {
            ModHooks.Instance.BeforeSavegameSaveHook -= BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook -= SaveGame;
            ModHooks.Instance.SavegameSaveHook -= SaveGameSave;
            ModHooks.Instance.NewGameHook -= AddComponent;
            ModHooks.Instance.LanguageGetHook -= OnLangGet;
            ModHooks.Instance.SetPlayerVariableHook -= SetVariableHook;
            ModHooks.Instance.GetPlayerVariableHook -= GetVariableHook;

            var finder = GameManager.instance.gameObject.GetComponent<RadianceFinder>();

            if (finder != null)
                UObject.Destroy(finder);
        }
    }
}