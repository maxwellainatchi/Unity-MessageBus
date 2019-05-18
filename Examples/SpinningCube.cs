using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinningCube : MonoBehaviour
{
    SwitchDirection.ShouldChange.Handler directionHandler;
    ChangeColor.ShouldChange.Handler colorHandler;

    private void Awake()
    {
        // Intiialize the handlers with what they need to effect their changes.
        this.directionHandler = new SwitchDirection.ShouldChange.Handler(this.GetComponent<Rigidbody>());
        this.colorHandler = new ChangeColor.ShouldChange.Handler(this.GetComponent<MeshRenderer>());
    }
    
    void Start()
    {
        // Set default values for the cube
        this.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 2);
        this.GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
