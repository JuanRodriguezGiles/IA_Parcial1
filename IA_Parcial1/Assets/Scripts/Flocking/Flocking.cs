using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class Flocking : MonoBehaviour
{
    public Vector3 baseRotation;
    public float maxSpeed;
    public float maxForce;
    public float checkRadius;

    public float separationMultiplayer;
    public float cohesionMultiplayer;
    public float alignmentMultiplayer;

    public Vector2 velocity;
    public Vector2 acceleration;

    private GameObject target;
    
    private Vector2 Position
    {
        get
        {
            return gameObject.transform.position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }

    private void Start()
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        target = GameObject.Find("Target");
    }

    private void Update()
    {
        Collider2D[] otherColliders = Physics2D.OverlapCircleAll(Position, checkRadius);
        List<Flocking> boids = otherColliders.Select(others => others.GetComponent<Flocking>()).ToList();
        boids.Remove(this);

        if (boids.Any())
            acceleration = Alignment(boids) * alignmentMultiplayer + Separation(boids) * separationMultiplayer + Cohesion(boids) * cohesionMultiplayer + DirectionToTarget();
        else
            acceleration = Vector2.zero;

        velocity += acceleration;
        velocity = LimitMagnitude(velocity, maxSpeed);
        Position += velocity * Time.deltaTime;
        float newAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, newAngle) + baseRotation);
    }

    private Vector2 Alignment(IEnumerable<Flocking> boids)
    {
        Vector2 velocity = Vector2.zero;

        foreach (Flocking boid in boids)
        {
            velocity += boid.velocity;
        }
        velocity /= boids.Count();

        var steer = Steer(velocity.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Cohesion(IEnumerable<Flocking> boids)
    {
        Vector2 sumPositions = Vector2.zero;
        foreach (Flocking boid in boids)
        {
            sumPositions += boid.Position;
        }
        Vector2 average = sumPositions / boids.Count();
        Vector2 direction = average - Position;

        return Steer(direction.normalized * maxSpeed);
    }

    private Vector2 Separation(IEnumerable<Flocking> boids)
    {
        Vector2 direction = Vector2.zero;
        boids = boids.Where(o => DistanceTo(o) <= checkRadius / 2);

        foreach (var boid in boids)
        {
            Vector2 difference = Position - boid.Position;
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

    private float DistanceTo(Flocking boid)
    {
        return Vector3.Distance(boid.transform.position, Position);
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

        return ((Vector2)target.transform.position - Position).normalized;
    }
}