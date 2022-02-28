using BepInEx.Logging;
using Last.Camera;
using Last.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FFPR_ResolutionFix
{
    public sealed class ModComponent : MonoBehaviour
    {
        public static ModComponent Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        private Boolean _isDisabled;
        public ModComponent(IntPtr ptr) : base(ptr)
        {
        }
        public void Awake()
        {
            Log = BepInEx.Logging.Logger.CreateLogSource("FFPR-StutterFix");
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
        public void Update()
        {
            try
            {
                if (_isDisabled)
                {
                    return;
                }
                GameObject CameraSingleton = GameObject.Find("Cameras");
                if(CameraSingleton != null)
                {
                    CommonCamera cameras = CameraSingleton.GetComponent<CommonCamera>();
                    if(cameras != null)
                    {
                        Camera battle = cameras.CameraBattle;
                        battle.pixelRect = new Rect(0, 0, 320, 180);
                        battle.orthographicSize = 90;
                        RenderTexture render = new RenderTexture(320, 180, 0);
                        render.filterMode = FilterMode.Point;
                        render.name = "Battle";
                        battle.targetTexture = render;
                        _isDisabled = true;
                    }
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
