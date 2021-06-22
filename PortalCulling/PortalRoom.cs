using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    // [RequireComponent(typeof(BoxCollider))]
    public class PortalRoom : MonoBehaviour
    {
        public GameObject root;
        public BoxCollider[] box;
        public bool IsVisible { get; set; } = true;
        public List<Portal> Portals = new List<Portal>();
        public List<GameObject> Viewers = new List<GameObject>();
        public List<PortalPlayer> Players = new List<PortalPlayer>();

        public void Enable(PortalPlayer invoker, IPortalViewer player, int iter)
        {
            /*if (invoker != null && invoker.transform.IsChildOf(root.transform))
                return;*/
            if (iter <= 0)
            {
                return;
            }
            /*if (PortalSystem.Singleton.CullingInfos[invoker].EnabledRooms.Contains(this))
                return;*/
            for (int i = 0; i < Portals.Count; i++)
            {
                if (PortalSystem.IsPortalVisible(invoker, Portals[i]))
                {
                    Portals[i].Enable(invoker, player, iter - 1);
                }
            }
            if (!PortalSystem.Singleton.CullingInfos[invoker].EnabledRooms.Contains(this))
                PortalSystem.Singleton.CullingInfos[invoker].EnabledRooms.Add(this);
            if (IsVisible)
                return;
            IsVisible = true;
            IPortalCull[] culls = root.GetComponentsInChildren<IPortalCull>(true);
            for (int i = 0; i < culls.Length; i++)
            {
                culls[i].Enable(invoker, player);
            }
        }

        public void Disable(PortalPlayer invoker, IPortalViewer player, int iter, bool skipportals = false)
        {
            IsVisible = false;
            IPortalCull[] culls = root.GetComponentsInChildren<IPortalCull>(true);
            for (int i = 0; i < culls.Length; i++)
            {
                culls[i].Disable(invoker, player);
            }
        }

        public void OnEnable()
        {
            PortalSystem.Singleton.AllRooms.Add(this);
        }

        public void OnDisable()
        {
            PortalSystem.Singleton.AllRooms.Remove(this);
        }

        public void Start()
        {
            if (box == null || box.Length == 0)
            {
                box = GetComponentsInChildren<BoxCollider>();
            }
            foreach (var b in box)
            {
                b.isTrigger = false;
            }
            var lst = box.ToList();
            lst.RemoveAll(p => p == null);
            box = lst.ToArray();
            if (root == null)
                root = gameObject;
            Players.Clear();
            Viewers.Clear();
            Disable(null, null, 1);
            // RaycastHit[] hits = Physics.BoxCastAll(box.bounds.center, box.size, Vector3.up, box.transform.rotation, 1f, LayerMask.GetMask(LayerMask.LayerToName(box.gameObject.layer)), QueryTriggerInteraction.Collide);
        }

        public IPortalCull[] GetCulls(PortalPlayer player, int iter)
        {
            if (iter <= 0)
            {
                return new IPortalCull[0];
            }
            int nextIter = iter - 1;
            List<IPortalCull> objs = new List<IPortalCull>();
            IPortalCull[] culls = root.GetComponentsInChildren<IPortalCull>(true);
            objs.AddRange(culls);
            for (int i = 0; i < Portals.Count; i++)
            {
                objs.AddRange(Portals[i].GetCulls(player, nextIter));
            }
            return objs.ToArray();
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if (box != null)
            {
                foreach (var b in box)
                {
                    if (b == null)
                        continue;
                    Gizmos.DrawWireCube(b.bounds.center, b.bounds.size);
                }
            }
            else
            {
                if (TryGetComponent(out BoxCollider b))
                {
                    Gizmos.DrawWireCube(b.bounds.center, b.bounds.size);
                }
                else
                {
                    Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
                }
            }
        }
    }
}