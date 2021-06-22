using System.Collections.Generic;
using UnityEngine;

namespace VirtualBrightPlayz.SCP_ET.PortalCulling
{
    public interface IPortalViewer
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        List<IPortalObject> Objects { get; set; }
    }
}