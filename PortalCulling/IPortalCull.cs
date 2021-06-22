using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    public interface IPortalCull
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        void Enable(PortalPlayer invoker, IPortalViewer player);
        void Disable(PortalPlayer invoker, IPortalViewer player);
    }
}