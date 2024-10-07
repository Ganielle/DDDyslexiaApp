using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceLetterItemData : MonoBehaviour
{
    [Header("WAYPOINTS")]
    public Transform[] startingTracePoints;    // Trace points defined in Render Texture space
    public Transform[] endTracePoints;         // End trace points
}
