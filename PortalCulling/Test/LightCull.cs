using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    [RequireComponent(typeof(Light))]
    public class LightCull : MonoBehaviour, IPortalCull
    {
        public void Enable(PortalPlayer invoker, IPortalViewer player)
        {
            GetComponent<Light>().enabled = true;
        }

        public void Disable(PortalPlayer invoker, IPortalViewer player)
        {
            GetComponent<Light>().enabled = false;
        }
    }
}