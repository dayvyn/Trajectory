using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] [Range(.1f, .25f)] float timeBetween;
    [SerializeField] [Range(10,100)] int lineCount;
    ProjectileTurret turret;
    float turretProjectileSpeed;
    List<Vector3> laserPoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        turret = FindAnyObjectByType<ProjectileTurret>();
        turretProjectileSpeed = turret.ProjectileSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);
        TrajectoryLine();

        //if (Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, 1000.0f, targetLayer))
        //{
        //    laserPoints.Add(hit.point);
        //}

        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void TrajectoryLine()
    {
        Vector3 startPos = turret.BarrelEnd.position;
        Vector3 projectileVelocity = turretProjectileSpeed * turret.BarrelEnd.forward;
        line.positionCount = Mathf.CeilToInt(lineCount / timeBetween) + 1;
        int i = 0;
        for (float time = 0; time < lineCount; time += timeBetween)
        {
            i++;
            Vector3 laserPoint = startPos + time * projectileVelocity;
            laserPoint.y = startPos.y + projectileVelocity.y * time + (Physics.gravity.y / 2f * Mathf.Pow(time,2));
            laserPoints.Add(laserPoint);
            Vector3 lastPos = line.GetPosition(i - 1);

            if (Physics.Raycast(lastPos, (laserPoint - lastPos).normalized, out RaycastHit hit, (laserPoint - lastPos).magnitude, targetLayer))
            {
                //laserPoints.Add(lastPos - 2 * Vector3.Dot(crosshair.transform.forward, lastPos) * crosshair.transform.forward);
                laserPoints.Add(Vector3.Reflect((laserPoint ), crosshair.transform.forward));
                line.positionCount = i + 2;
                return;
            }
            
            
        }
    }

    
}
