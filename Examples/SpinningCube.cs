using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinningCube : MonoBehaviour
{
    // The handlers must be held in memory or the deconstructor will kill the bus listener
    List<Messaging.IHandler> handlers;
    private void Awake()
    {
        // Intiialize the handlers with what they need to effect their changes.
        this.handlers = new List<Messaging.IHandler> {
            new SwitchDirection.ShouldChange.Handler(this.GetComponent<Rigidbody>()),
            new ChangeColor.ShouldChange.SetColorHandler(this.GetComponent<MeshRenderer>())
        };
    }
    
    void Start()
    {
        // Set default values for the cube
        this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 2);
        this.GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
