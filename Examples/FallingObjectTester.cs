using UnityEngine;
using System.Collections.Generic;
namespace EyE.Debug.Examples
{

    public class FallingObjectTester : MonoBehaviour
    {
        public Rigidbody testRigidbody;
        public float maxExpectedVelocity = 20f;
        public List<Vector3> aListOfPositions;

        void Start()
        {

            aListOfPositions = new List<Vector3>();
            for (int i = 0; i < 100; i++)
                aListOfPositions.Add(Random.insideUnitSphere * i);

            PhysicsDebug.Log("Starting falling object test...");

        }

        void FixedUpdate()
        {
            foreach (Vector3 pos in aListOfPositions)
            {
                PhysicsDebug.AppendToNextLog("\nPosition Distance: " + (pos - transform.position).magnitude);
            }
            PhysicsDebug.Log("Computed Distances--");

            PhysicsAssert.IsNotNull(testRigidbody, "Rigid body to test is currently null.");

            // Log the current velocity and height
            PhysicsDebug.Log($"Velocity: {testRigidbody.velocity.magnitude:F2} m/s, Height: {transform.position.y:F2} m");

            // Assert that velocity doesn't exceed expected max
            PhysicsAssert.IsTrue(testRigidbody.velocity.magnitude <= maxExpectedVelocity,
                $"Velocity exceeded expected max ({maxExpectedVelocity})!", this);

            // Optional: assert that object hasn't fallen below zero
            PhysicsAssert.IsTrue(transform.position.y >= 0f, "Object fell below ground!", this);



        }

        void OnCollisionEnter(Collision collision)
        {
            PhysicsDebug.Log($"Object collided with: {collision.gameObject.name}");
        }
    }

}