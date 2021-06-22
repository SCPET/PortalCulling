using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : MonoBehaviour, IPortalObject, IPortalViewer
    {
        // public Vector3 portalPlane;
        public Vector2 portalSize = Vector2.one;
        public bool isOpen;
        public bool IsVisible { get; set; } = true;
        public PortalPlayer Player { get; private set; }
        public int PlayerIter { get; private set; }
        public List<PortalRoom> Rooms = new List<PortalRoom>();
        public List<IPortalObject> Objects { get; set; } = new List<IPortalObject>();
        private List<PortalRoom> _EnabledRooms = new List<PortalRoom>();

        public void Disable(PortalPlayer invoker, IPortalViewer player, int iter)
        {
            return;
            // return;
            if (!IsVisible)
                return;
            if (invoker is PortalPlayer player1)
            {
                IsVisible = false;
                Player = null;
                /*PortalRoom[] vis = GetVisibleRooms(invoker, true);
                foreach (var room in vis)
                {
                    room.Disable(invoker, this, iter);
                }*/
                Rooms.Clear();
            }
        }

        public void Enable(PortalPlayer invoker, IPortalViewer player, int iter)
        {
            /*if (PortalSystem.Singleton.CullingInfos[invoker].EnabledPortals.Contains(this))
                return;*/
            if (invoker is PortalPlayer player1)
            {
                if (!PortalSystem.Singleton.CullingInfos[invoker].EnabledPortals.Contains(this))
                    PortalSystem.Singleton.CullingInfos[invoker].EnabledPortals.Add(this);
                IsVisible = true;
                Player = player1;
                PlayerIter = iter;
                Rooms.Clear();
                RaycastHit[] hit = Physics.SphereCastAll(transform.position, 1f, Vector3.up, 1f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)), QueryTriggerInteraction.Collide);
                foreach (var item in hit)
                {
                    if (item.transform.TryGetComponent(out PortalRoom room))
                    {
                        Rooms.Add(room);
                        room.Enable(invoker, this, iter);
                    }
                    else
                    {
                        room = item.transform.GetComponentInParent<PortalRoom>();
                        if (room != null)
                        {
                            Rooms.Add(room);
                            room.Enable(invoker, this, iter);
                        }
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            return;
            PortalRoom[] vis = GetVisibleRooms(Player, true);
            foreach (var item in vis)
            {
                // item.Enable(Player, PlayerIter);
            }
        }

        public void Start()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            box.isTrigger = false;
            box.center = Vector3.zero;
            box.size = new Vector3(portalSize.x, portalSize.y, 0.1f);
            Disable(null, null, 0);
            IsVisible = false;
            List<PortalRoom> portalRooms = new List<PortalRoom>();
            RaycastHit[] hit = Physics.SphereCastAll(transform.position, 1f, Vector3.up, 1f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)), QueryTriggerInteraction.Collide);
            foreach (var item in hit)
            {
                if (item.transform.TryGetComponent(out PortalRoom room) && !portalRooms.Contains(room))
                {
                    portalRooms.Add(room);
                    continue;
                }
                room = item.transform.GetComponentInParent<PortalRoom>();
                if (room != null && !portalRooms.Contains(room))
                {
                    portalRooms.Add(room);
                }
            }
            if (portalRooms.Count > 1)
            {
                foreach (var room in portalRooms)
                {
                    room.Portals.Add(this);
                }
            }
        }

        public PortalRoom[] GetVisibleRooms(PortalPlayer player, bool visible = true)
        {
            List<PortalRoom> portalRooms = new List<PortalRoom>();
            if (player != null && player.camera != null)
            {
                Vector2[] poses = new Vector2[]
                {
                    new Vector2(portalSize.x / 2f, portalSize.y / 2f),
                    new Vector2(portalSize.x / 2f, -portalSize.y / 2f),
                    new Vector2(-portalSize.x / 2f, -portalSize.y / 2f),
                    new Vector2(-portalSize.x / 2f, portalSize.y / 2f),
                    new Vector2(0f, 0f),
                    new Vector2(portalSize.x / 2f, 0f),
                    new Vector2(-portalSize.x / 2f, 0f),
                    new Vector2(0f, portalSize.y / 2f),
                    new Vector2(0f, -portalSize.y / 2f),
                };
                bool found = false;
                for (int j = 0; j < poses.Length; j++)
                {
                    if (found)
                        break;
                    var p = transform.TransformPoint(new Vector3(poses[j].x, poses[j].y, 0f));
                    Debug.DrawLine(player.camera.transform.position, p);
                    Vector3 viewpos = player.camera.WorldToViewportPoint(p);
                    if (viewpos.z > 0f && viewpos.x >= 0f && viewpos.x <= 1f && viewpos.y >= 0f && viewpos.y <= 1f)
                    {
                        found = true;
                        float dot2 = Vector3.Dot((transform.position - player.transform.position).normalized, transform.forward);
                        Vector3 pos2 = transform.InverseTransformPoint(player.transform.position);
                        for (int i = 0; i < Rooms.Count; i++)
                        {
                            if (Rooms[i] == null)
                            {
                                Rooms.RemoveAt(i);
                                i--;
                                continue;
                            }
                            var room = Rooms[i];
                            Vector3 pos = transform.InverseTransformPoint(room.transform.position);
                            if ((pos2.z > 0f && pos.z < 0f) || (pos2.z < 0f && pos.z > 0f))
                            {
                                if (visible)
                                    portalRooms.Add(room);
                                // room.Enable(this);
                            }
                            else if (!visible)
                            {
                                portalRooms.Add(room);
                                // room.Disable(this);
                            }
                        }
                    }
                }
                if (!found)
                {
                    Vector3 pos2 = transform.InverseTransformPoint(player.transform.position);
                    for (int i = 0; i < Rooms.Count; i++)
                    {
                        var room = Rooms[i];
                        Vector3 pos = transform.InverseTransformPoint(room.transform.position);
                        if ((pos2.z > 0f && pos.z < 0f) || (pos2.z < 0f && pos.z > 0f))
                        {
                            if (!visible)
                                portalRooms.Add(room);
                            // room.Disable(this);
                        }
                    }
                }
            }
            return portalRooms.ToArray();
        }

        public IPortalCull[] GetCulls(PortalPlayer player, int iter)
        {
            int nextIter = iter - 0;
            List<IPortalCull> objs = new List<IPortalCull>();
            PortalRoom[] rooms = GetVisibleRooms(player);
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] != null)
                {
                    objs.AddRange(rooms[i].GetCulls(player, nextIter));
                }
            }
            return objs.ToArray();
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawMesh(Resources.GetBuiltinResource<Mesh>("Cube.fbx"), transform.position, transform.rotation, portalSize);
        }
    }
}