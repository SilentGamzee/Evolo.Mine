using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FOW.Script;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;


public delegate void TargetsVisibilityChange(List<Transform> newTargets);

[ExecuteInEditMode]
public class FieldOfView : MonoBehaviour
{
    public bool Is2D;
    public float viewRadius;

    public static float viewAngle = 360;

    public float viewDepth;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public static LayerMask static_targetMask;
    public static LayerMask static_obstacleMask;

    [HideInInspector] public static List<FieldOfView> visions = new List<FieldOfView>();
    [HideInInspector] public static List<Transform> visibleTargets = new List<Transform>();
    [HideInInspector] public static List<Transform> visibleBefore = new List<Transform>();

    public int meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;


    public MeshFilter viewMeshFilter;
    public bool debug;
    Mesh viewMesh;

    public static event TargetsVisibilityChange OnTargetsVisibilityChange;

    public static bool Is2D_static;

    public FogProjector fogProjector;
    public float updateDistance = 1;
    Vector3 lastUpdatePos;

    public static bool Inited = false;

    void Start()
    {
        if (!ChunkManager.staticFogEnabled)
        {
            this.enabled = false;
            return;
        }
        Inited = true;
        Is2D_static = Is2D;
        visions.Add(this);
        viewMesh = new Mesh {name = "View Mesh"};
        viewMeshFilter.mesh = viewMesh;

        static_targetMask = targetMask;
        static_obstacleMask = obstacleMask;

        fogProjector = fogProjector ?? FindObjectOfType<FogProjector>();

        UpdateFog();
    }

    public static void UpdateFog()
    {
        //var field = ent.transform.GetComponentsInChildren<FieldOfView>();
        FindAllVisibleTargets();
        Recolor.OnFogChange();
    }

    void LateUpdate()
    {
        DrawFieldOfView();
        if (Vector3.Distance(transform.position, lastUpdatePos) > updateDistance || Time.time < .5f)
        {
            lastUpdatePos = transform.position;
            fogProjector.UpdateFog();
        }
    }

    public static bool IsVisible(GameEntity ent)
    {
        if (ent == null) return false;
        return Inited
               && (visibleBefore.Contains(ent.transform)
                   || visibleTargets.Contains(ent.transform));
    }
    

    static bool IsVisible(Transform from, Transform target)
    {
        var dirToTarget = (target.position - from.position).normalized;

        if (Vector3.Angle(from.forward, dirToTarget) < viewAngle / 2)
        {
            float dstToTarget = Vector3.Distance(from.position, target.position);
            if (!Physics.Raycast(from.position, dirToTarget, dstToTarget, static_obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    static void FindAllVisibleTargets()
    {
        visibleTargets.Clear();

        foreach (var vision in visions)
        {
            if (vision == null) continue;
            var pos = vision.transform.position;

            if (!Is2D_static)
            {
                //3D
                Collider[] targetsInViewRadius = Physics.OverlapSphere(pos, vision.viewRadius, vision.targetMask);
                foreach (var t in targetsInViewRadius)
                {
                    var target = t.transform;
                    if (IsVisible(vision.transform, target) && !visibleTargets.Contains(target))
                        visibleTargets.Add(target);
                }
            }
            else
            {
                //2D
                var circle = Physics2D.OverlapCircleAll(new Vector2(pos.x, pos.y), vision.viewRadius, vision.targetMask);
                foreach (var t in circle)
                {
                    var target = t.transform;
                    if (IsVisible(vision.transform, target) && !visibleTargets.Contains(target))
                        visibleTargets.Add(target);
                }
            }
        }
        if (OnTargetsVisibilityChange != null) OnTargetsVisibilityChange(visibleTargets);
        foreach (var target in visibleTargets)
            if (!visibleBefore.Contains(target))
                visibleBefore.Add(target);
    }

    void DrawFieldOfView()
    {
        float stepAngleSize = viewAngle / meshResolution;
        List<Vector3> viewPoints = new List<Vector3>();
        ObstacleInfo oldObstacle = new ObstacleInfo();
        for (int i = 0; i <= meshResolution; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ObstacleInfo newObstacle = FindObstacles(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldObstacle.dst - newObstacle.dst) > edgeDstThreshold;
                if (oldObstacle.hit != newObstacle.hit ||
                    oldObstacle.hit && edgeDstThresholdExceeded)
                {
                    EdgeInfo edge = FindEdge(oldObstacle, newObstacle);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }


            viewPoints.Add(newObstacle.point);
            oldObstacle = newObstacle;
        }

        int vertexCount = viewPoints.Count + 1;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }


    EdgeInfo FindEdge(ObstacleInfo minObstacle, ObstacleInfo maxObstacle)
    {
        float minAngle = minObstacle.angle;
        float maxAngle = maxObstacle.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ObstacleInfo newObstacle = FindObstacles(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minObstacle.dst - newObstacle.dst) > edgeDstThreshold;
            if (newObstacle.hit == minObstacle.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newObstacle.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newObstacle.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    ObstacleInfo FindObstacles(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (DebugRayCast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ObstacleInfo(true, hit.point + hit.normal * -viewDepth, hit.distance, globalAngle);
        }
        return new ObstacleInfo(false, transform.position + dir * (viewRadius - viewDepth), viewRadius, globalAngle);
    }

    bool DebugRayCast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, int mask)
    {
        if (Physics.Raycast(origin, direction, out hit, maxDistance, mask))
        {
            if (debug)
                Debug.DrawLine(origin, hit.point);
            return true;
        }
        if (debug)
            Debug.DrawLine(origin, origin + direction * maxDistance);
        return false;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ObstacleInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ObstacleInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}