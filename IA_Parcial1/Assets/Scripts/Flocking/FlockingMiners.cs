using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

public class FlockingMiners : MonoBehaviour
{
    #region EXPOSED_FIELDS
    public Vector3 baseRotation;
    public float maxSpeed;
    public float maxForce;
    public float checkRadius;

    public float separationMultiplayer;
    public float cohesionMultiplayer;
    public float alignmentMultiplayer;

    public Vector2 velocity;
    public Vector2 acceleration;
    #endregion

    #region PRIVATE_FIELDS
    private Action<Vector2> onUpdatePos;
    private Func<Vector2> onGetPos;
    private bool enabled = false;
    private Vector2 target;
    #endregion

    #region UNITY_CALLS
    private void Start()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private void Update()
    {
        if (!enabled) return;

        Vector2 currentPos = onGetPos.Invoke();
        
        Collider2D[] otherColliders = Physics2D.OverlapCircleAll(currentPos, checkRadius);
        List<FlockingMiners> boids = otherColliders.Select(others => others.GetComponent<FlockingMiners>()).ToList();
        boids.Remove(this);

        if (boids.Any())
            acceleration = Alignment(boids) * alignmentMultiplayer + Separation(boids) * separationMultiplayer + Cohesion(boids) * cohesionMultiplayer + DirectionToTarget();
        else
            acceleration = DirectionToTarget();

        velocity += acceleration;
        velocity = LimitMagnitude(velocity, maxSpeed);
        currentPos += velocity * Time.deltaTime;
        onUpdatePos?.Invoke(currentPos);
        
        float newAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, newAngle) + baseRotation);
    }
    #endregion

    #region PRIVATE_METHODS
    private Vector2 Alignment(IEnumerable<FlockingMiners> boids)
    {
        Vector2 velocity = Vector2.zero;

        foreach (FlockingMiners boid in boids)
        {
            velocity += boid.velocity;
        }

        velocity /= boids.Count();

        var steer = Steer(velocity.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Cohesion(IEnumerable<FlockingMiners> boids)
    {
        Vector2 sumPositions = Vector2.zero;
        foreach (FlockingMiners boid in boids)
        {
            sumPositions += boid.onGetPos.Invoke();
        }

        Vector2 average = sumPositions / boids.Count();
        Vector2 direction = average - onGetPos.Invoke();

        return Steer(direction.normalized * maxSpeed);
    }

    private Vector2 Separation(IEnumerable<FlockingMiners> boids)
    {
        Vector2 direction = Vector2.zero;
        boids = boids.Where(o => DistanceTo(o) <= checkRadius / 2);

        foreach (var boid in boids)
        {
            Vector2 difference = onGetPos.Invoke() - boid.onGetPos.Invoke();
            direction += difference.normalized / difference.magnitude;
        }

        direction /= boids.Count();

        return Steer(direction.normalized * maxSpeed);
    }

    private Vector2 Steer(Vector2 desired)
    {
        var steer = desired - velocity;
        steer = LimitMagnitude(steer, maxForce);

        return steer;
    }

    private float DistanceTo(FlockingMiners boid)
    {
        return Vector3.Distance(boid.transform.position, onGetPos.Invoke());
    }

    private Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
    {
        if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            baseVector = baseVector.normalized * maxMagnitude;
        }

        return baseVector;
    }

    private Vector2 DirectionToTarget()
    {
        if (target == null)
        {
            return Vector2.zero;
        }

        return (target - onGetPos.Invoke()).normalized;
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Action<Vector2> onUpdatePos, Func<Vector2> onGetPos)
    {
        this.onUpdatePos = onUpdatePos;
        this.onGetPos = onGetPos;
    }

    public void UpdateTarget(Vector2 target)
    {
        this.target = target;
    }

    public void ToggleFlocking(bool enabled)
    {
        this.enabled = enabled;
    }
    #endregion
}