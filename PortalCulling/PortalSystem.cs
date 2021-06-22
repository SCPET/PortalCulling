using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    [DefaultExecutionOrder(-100)]
    public class PortalSystem : MonoBehaviour
    {
        public class CullInfo
        {
            public PortalPlayer Player;
            public List<PortalRoom> EnabledRooms = new List<PortalRoom>();
            public List<Portal> EnabledPortals = new List<Portal>();
            public List<PortalRoom> LastEnabledRooms = new List<PortalRoom>();
            public List<Portal> LastEnabledPortals = new List<Portal>();
        }

        private static PortalSystem _system;
        public static PortalSystem Singleton
        {
            get
            {
                if (_system == null)
                {
                    _system = new GameObject("PortalSystem").AddComponent<PortalSystem>();
                }
                return _system;
            }
        }
        public List<PortalPlayer> CurrentlyRendering { get; private set; }
        public List<PortalRoom> AllRooms { get; private set; }
        public List<PortalRoom> EnabledRooms { get; private set; }
        public List<Portal> AllPortals { get; private set; }
        public List<Portal> EnabledPortals { get; private set; }
        public Dictionary<PortalPlayer, CullInfo> CullingInfos { get; private set; }

        public void OnEnable()
        {
            _system = this;
            CurrentlyRendering = new List<PortalPlayer>();
            AllRooms = new List<PortalRoom>();
            EnabledRooms = new List<PortalRoom>();
            AllPortals = new List<Portal>();
            EnabledPortals = new List<Portal>();
            CullingInfos = new Dictionary<PortalPlayer, CullInfo>();
            Camera.onPreRender += PreRender;
            Camera.onPostRender += PostRender;
            RenderPipelineManager.beginCameraRendering += SRPPreRender;
            RenderPipelineManager.endCameraRendering += SRPPostRender;
        }

        public void OnDisable()
        {
            CurrentlyRendering = null;
            AllRooms = null;
            EnabledRooms = null;
            AllPortals = null;
            EnabledPortals = null;
            CullingInfos = null;
            Camera.onPreRender -= PreRender;
            Camera.onPostRender -= PostRender;
            RenderPipelineManager.beginCameraRendering -= SRPPreRender;
            RenderPipelineManager.endCameraRendering -= SRPPostRender;
        }

        private void SRPPostRender(ScriptableRenderContext arg1, Camera arg2)
        {
            // EndTick(arg2);
        }

        private void PostRender(Camera cam)
        {
            // EndTick(cam);
        }

        private void SRPPreRender(ScriptableRenderContext arg1, Camera arg2)
        {
            // if (GraphicsSettings.currentRenderPipeline != null)
                // Tick(arg2);
        }

        private void PreRender(Camera cam)
        {
            // Tick(cam);
        }

        public void Update()
        {
            EndTick();
        }

        public void LateUpdate()
        {
            Tick();
        }

        public void Tick()
        {
            CurrentlyRendering.RemoveAll(p => p == null);
            for (int i = 0; i < CurrentlyRendering.Count; i++)
            {
                if (CurrentlyRendering[i].camera == null)
                {
                    CurrentlyRendering[i].camera = CurrentlyRendering[i].GetComponent<Camera>();
                }
                if (CurrentlyRendering[i].IsMainCamera)
                    Tick(CurrentlyRendering[i].camera);
            }
        }

        public void EndTick()
        {
            var copy = CullingInfos.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                EndTick(copy[i].Key.camera);
            }
        }

        public void Tick(Camera cam)
        {
            if (cam != null && cam.TryGetComponent(out PortalPlayer plr))
            {
                if (!CullingInfos.ContainsKey(plr))
                {
                    CullingInfos.Add(plr, new CullInfo()
                    {
                        Player = plr
                    });
                }
                // else
                    // return;
                for (int j = 0; j < AllRooms.Count; j++)
                {
                    if (PortalSystem.IsInRoom(plr, AllRooms[j]))
                    {
                        Debug.DrawLine(plr.camera.transform.position, AllRooms[j].root.transform.position, Color.yellow);
                        AllRooms[j].Enable(plr, plr, 2);
                    }
                }
            }
        }

        public void EndTick(Camera cam)
        {
            if (cam != null && cam.TryGetComponent(out PortalPlayer plr) && CullingInfos.ContainsKey(plr))
            {

                for (int i = 0; i < CullingInfos[plr].LastEnabledRooms.Count; i++)
                {
                    if (!CullingInfos[plr].EnabledRooms.Contains(CullingInfos[plr].LastEnabledRooms[i]))
                    {
                        try
                        {
                            CullingInfos[plr].LastEnabledRooms[i].Disable(plr, plr, 1);
                        }
                        catch
                        { }
                    }
                    // CullingInfos[plr].EnabledRooms[i].Disable(plr, plr, 1);
                }
                for (int i = 0; i < CullingInfos[plr].EnabledPortals.Count; i++)
                {
                    if (!CullingInfos[plr].LastEnabledPortals.Contains(CullingInfos[plr].EnabledPortals[i]))
                    {
                        try
                        {
                            CullingInfos[plr].EnabledPortals[i].Disable(plr, plr, 1);
                        }
                        catch
                        { }
                    }
                    CullingInfos[plr].EnabledPortals[i].Disable(plr, plr, 1);
                }
                CullingInfos[plr].LastEnabledRooms.Clear();
                CullingInfos[plr].LastEnabledPortals.Clear();
                for (int i = 0; i < CullingInfos[plr].EnabledRooms.Count; i++)
                {
                    CullingInfos[plr].LastEnabledRooms.Add(CullingInfos[plr].EnabledRooms[i]);
                }
                for (int i = 0; i < CullingInfos[plr].EnabledPortals.Count; i++)
                {
                    CullingInfos[plr].LastEnabledPortals.Add(CullingInfos[plr].EnabledPortals[i]);
                }
                CullingInfos[plr].EnabledRooms.Clear();
                CullingInfos[plr].EnabledPortals.Clear();
                // CullingInfos.Remove(plr);
            }
        }

        public static bool IsInRoom(PortalPlayer player, PortalRoom room)
        {
            if (room.box == null || player == null || player.camera == null)
            {
                return false;
            }
            for (int i = 0; i < room.box.Length; i++)
            {
                if (room.box[i] == null)
                {
                    continue;
                }
                if (room.box[i].bounds.Contains(player.camera.transform.position))
                    return true;
            }
            return false;
        }

        public static bool IsPortalVisible(PortalPlayer player, Portal portal)
        {
            Vector2[] poses = new Vector2[]
            {
                new Vector2(portal.portalSize.x / 2f, portal.portalSize.y / 2f),
                new Vector2(portal.portalSize.x / 2f, -portal.portalSize.y / 2f),
                new Vector2(-portal.portalSize.x / 2f, -portal.portalSize.y / 2f),
                new Vector2(-portal.portalSize.x / 2f, portal.portalSize.y / 2f),
                new Vector2(0f, 0f),
                new Vector2(portal.portalSize.x / 2f, 0f),
                new Vector2(-portal.portalSize.x / 2f, 0f),
                new Vector2(0f, portal.portalSize.y / 2f),
                new Vector2(0f, -portal.portalSize.y / 2f),
            };
            bool found = false;
            for (int j = 0; j < poses.Length; j++)
            {
                // if (found)
                    // continue;
                    // break;
                var p = portal.transform.TransformPoint(new Vector3(poses[j].x, poses[j].y, 0f));
                Vector3 viewpos = player.camera.WorldToViewportPoint(p);
                if (viewpos.z > 0f && viewpos.x >= 0f && viewpos.x <= 1f && viewpos.y >= 0f && viewpos.y <= 1f)
                {
                    Debug.DrawLine(player.camera.transform.position, p, Color.red);
                    found = true;
                    // break;
                }
                else
                {
                    Debug.DrawLine(player.camera.transform.position, p, Color.yellow);
                }
            }
            return found;
        }
    }
}