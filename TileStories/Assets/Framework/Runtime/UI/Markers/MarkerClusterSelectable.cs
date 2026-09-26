using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileStories
{
    // Thin tap target on POI_Cluster.prefab: a tap on a cluster aggregate reports its member POIs
    // through SelectionEventBus.SelectCluster (zoom-on-select zooms in so the cluster breaks apart).
    // A cluster is not a POI, so it never becomes the selection.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MarkerClusterView))]
    public sealed class MarkerClusterSelectable : MonoBehaviour, IPointerClickHandler
    {
        // Raised by uGUI's GraphicRaycaster when the cluster is tapped
        public void OnPointerClick(PointerEventData eventData)
        {
            var view = GetComponent<MarkerClusterView>();
            if (view != null && view.MemberPoiIds != null)
                SelectionEventBus.SelectCluster(new List<string>(view.MemberPoiIds));
        }
    }
}
