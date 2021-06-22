using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    public interface IPortalObject
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        bool IsVisible { get; }
        void Enable(PortalPlayer invoker, IPortalViewer player, int iter);
        void Disable(PortalPlayer invoker, IPortalViewer player, int iter);
    }
}