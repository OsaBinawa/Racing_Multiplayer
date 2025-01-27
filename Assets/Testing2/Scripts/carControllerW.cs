using UnityEngine;

public class carControllerW : MonoBehaviour
{
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyWheelPosition();
    }

    void ApplyWheelPosition(){
        UpdateWheels(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheels(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheels(colliders.BRWheel, wheelMeshes.BRWheel);
        UpdateWheels(colliders.BLWheel, wheelMeshes.BLWheel);
    }

    void UpdateWheels(WheelCollider coll, MeshRenderer wheelMesh){
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }
}

[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider BRWheel;
    public WheelCollider BLWheel;
}

[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer BRWheel;
    public MeshRenderer BLWheel;
}
