using BepInEx.Logging;
using Last.Battle;
using Last.Camera;
using Last.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Last.Camera.FrontRenderTarget;

namespace FFPR_BattleSceneScaler
{
    public sealed class ModComponent : MonoBehaviour
    {
        public static ModComponent Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static Configuration Config { get; set; }
        public static CommonCamera CommonCam { get; set; }
        public static Camera BattleCamera { get; set; }
        public static Camera BackgroundCamera { get; set; }
        public static FrontRenderTarget Front { get; set; }
        public static RenderTargetProperty BattleRenderTarget { get; set; }
        public static GameObject BattleSceneRoot { get; set; }
        private static int RenderTargetIndex = -1;
        private bool _isInitialized = false;
        private bool _isSceneScaled = false;
        private Boolean _isDisabled;
        public ModComponent(IntPtr ptr) : base(ptr)
        {
        }
        public void Awake()
        {
            Log = BepInEx.Logging.Logger.CreateLogSource("FFPR-BattleSceneScaler");
            Config = new Configuration(EntryPoint.Instance.Config);
            try
            {
                Instance = this;
                Log.LogMessage($"[{nameof(ModComponent)}].{nameof(Awake)}: Processed successfully.");
            }
            catch (Exception ex)
            {
                _isDisabled = true;
                Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {ex}");
                throw;
            }

        }
        public static void SetResolution(int width, int height)
        {
            BattleCamera.pixelRect = new Rect(0, 0, width, height);
            BattleCamera.orthographicSize = height/2;
            RenderTexture render = new RenderTexture(width, height, 0);
            render.filterMode = FilterMode.Point;
            render.name = "Battle";
            BattleCamera.targetTexture = render;
            Log.LogInfo(BattleRenderTarget.ppu);
            BattleRenderTarget.ppu = 1920 / width; //bit of a weird way to calculate this, but it works? it might be better to use the current main width for this, for consistent pixel scaling
            Log.LogInfo(BattleRenderTarget.ppu);
            BattleRenderTarget.size = new Vector2(width, height);
            BattleRenderTarget.tex = render;
            Front.targets[RenderTargetIndex] = BattleRenderTarget;
        }
        public static void SetBackgroundResolution(int width, int height, BattleBackgroundController controller)
        {
            BackgroundCamera.pixelRect = new Rect(0, 0, width, height);
            BackgroundCamera.orthographicSize = height / 2;
            RenderTexture render = new RenderTexture(width, height, 0);
            render.filterMode = FilterMode.Point;
            BackgroundCamera.targetTexture = render;
            controller.originalRenderTexture = render;
            controller.UpdateBackground();
        }
        public static void ScaleScene(GameObject target)
        {
            List<GameObject> gobs = GetAllChildren(target);
            foreach(GameObject gob in gobs)
            {
                switch (gob.name)
                {
                    case "Entity":
                        GameObject enemies = GetDirectChild(gob, "EnemyParty");
                        if (enemies != null)
                        {
                            enemies.transform.localPosition = new Vector3(enemies.transform.localPosition.x*Config.EnemyScale, enemies.transform.localPosition.y*Config.EnemyScale, enemies.transform.localScale.z);
                            enemies.transform.localScale = new Vector3(Config.EnemyScale, Config.EnemyScale, enemies.transform.localScale.z);
                        }
                        GameObject players = GetDirectChild(gob, "PlayerParty");
                        if (players != null)
                        {
                            players.transform.localPosition = new Vector3(players.transform.localPosition.x*Config.PlayerScale, players.transform.localPosition.y*Config.PlayerScale, players.transform.localScale.z);
                            players.transform.localScale = new Vector3(Config.PlayerScale, Config.PlayerScale, players.transform.localScale.z);
                        }
                        break;
                    case "EffectLayer":
                    case "FxQuadParent":
                        gob.transform.localScale = new Vector3(Config.SceneScale, Config.SceneScale, gob.transform.localScale.z);
                        break;
                    case "BattleBackground":
                        GameObject anchor = GetDirectChild(gob, "Anchor");
                        if (anchor != null) anchor.transform.localScale = new Vector3(Config.SceneScale, Config.SceneScale, anchor.transform.localScale.z);
                        SetBackgroundResolution(Mathf.RoundToInt(512 * Config.BackgroundScale), Mathf.RoundToInt(256 * Config.BackgroundScale),gob.GetComponent<BattleBackgroundController>());
                        break;
                    default:
                        break;
                }
            }
        }
        public static void SetScale(Vector2 resolution)
        {
            SetResolution(Mathf.RoundToInt(resolution.x * Config.SceneScale), Mathf.RoundToInt(resolution.y * Config.SceneScale));
            if (BattleSceneRoot != null) ScaleScene(BattleSceneRoot);

        }
        int isScene_CurrentlyLoaded(string sceneName_no_extention)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name == sceneName_no_extention)
                {
                    //the scene is already loaded
                    return i;
                }
            }

            return -1;//scene not currently loaded in the hierarchy
        }
        public static GameObject GetDirectChild(GameObject obj, string childName)
        {

            if (obj != null)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform child = obj.transform.GetChild(i);
                    if (child != null)
                    {
                        if (child.gameObject != null)
                        {
                            //ModComponent.Log.LogInfo(child.name);
                            if (child.name == childName)
                            {
                                return child.gameObject;
                            }
                        }
                    }


                }
            }
            else
            {
                ModComponent.Log.LogWarning("Root object is null!");
            }

            return null;
        }
        public static List<GameObject> GetAllChildren(GameObject obj)
        {
            List<GameObject> children = new List<GameObject>();

            if (obj != null)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform child = obj.transform.GetChild(i);
                    if (child != null)
                    {
                        if (child.gameObject != null)
                        {
                            children.Add(child.gameObject);
                            if (child.childCount != 0)
                            {
                                children.AddRange(GetAllChildren(child.gameObject));
                            }
                        }
                    }


                }
            }
            else
            {
                ModComponent.Log.LogWarning("Root object is null!");
            }

            return children;
        }
        public void Update()
        {
            try
            {
                if (_isDisabled)
                {
                    return;
                }
                if (!_isInitialized)
                {
                    if (Front == null)
                    {
                        Front = FrontRenderTarget.Instance;
                        if (Front == null) return;
                    }
                    if (CommonCam == null)
                    {
                        CommonCam = CommonCamera.Instance;
                        if (CommonCam == null) return;
                    }
                    BattleCamera = CommonCam.CameraBattle;
                    BackgroundCamera = CommonCam.CameraBattleBackGround;
                    for(int i = 0;i < Front.targets.Count;i++)
                    {
                        if (Front.targets[i].tex.name == "Battle")
                        {
                            BattleRenderTarget = Front.targets[i];
                            RenderTargetIndex = i;
                        }
                    }
                }
                int isSceneLoaded = isScene_CurrentlyLoaded("Battle");
                if (isSceneLoaded != -1)
                {
                    if (!_isSceneScaled)
                    {
                        Scene battleScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(isSceneLoaded);
                        if (battleScene.rootCount > 0)
                        {
                            foreach(GameObject root in new List<GameObject>(battleScene.GetRootGameObjects()))
                            {
                                if (root.name == "RootObject") BattleSceneRoot = root;
                            }
                        }
                    }
                }
                else
                {
                    if(_isSceneScaled) _isSceneScaled = false;
                    BattleSceneRoot = null;
                }
                
            }
            catch (Exception ex)
            {
                _isDisabled = true;
                Log.LogError($"[{nameof(ModComponent)}].{nameof(Update)}(): {ex}");
                throw;
            }

        }
    }
}
