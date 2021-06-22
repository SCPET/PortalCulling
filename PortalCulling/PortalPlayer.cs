using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Camera))]
    public class PortalPlayer : MonoBehaviour, IPortalViewer
    {
        [HideInInspector]
        public Camera camera;
        public List<IPortalObject> Objects { get; set; } = new List<IPortalObject>();
        public List<PortalRoom> Rooms { get; set; } = new List<PortalRoom>();
        public List<PortalRoom> _EnabledRooms = new List<PortalRoom>();
        private List<IPortalCull> prevCulls = new List<IPortalCull>();
        public bool IsMainCamera
        {
            get
            {
                return _mainCamera;
            }
            set
            {
                _mainCamera = value;
                if (enabled)
                    OnEnable();
                else
                    OnDisable();
            }
        }
        [SerializeField]
        private bool _mainCamera = true;

        public void Start()
        {
            camera = GetComponent<Camera>();
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        public void OnEnable()
        {
            PortalSystem.Singleton.CurrentlyRendering.Add(this);
            PortalSystem.Singleton.Tick(GetComponent<Camera>());
        }

        public void OnDisable()
        {
            PortalSystem.Singleton.CurrentlyRendering.Remove(this);
        }

        public IPortalCull[] GetCulls()
        {
            List<IPortalCull> objs = new List<IPortalCull>();
            for (int i = 0; i < Rooms.Count; i++)
            {
                for (int j = 0; i < Rooms[i].box.Length; j++)
                {
                    if (Rooms[i] != null && Rooms[i].box[j].bounds.Contains(transform.position))
                    {
                        objs.AddRange(Rooms[i].GetCulls(this, 5));
                    }
                }
            }
            return objs.ToArray();
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetComponent<SphereCollider>().bounds.center, GetComponent<SphereCollider>().radius);
            Gizmos.DrawRay(new Ray(transform.position, transform.forward * GetComponent<SphereCollider>().radius));
        }
    }
}