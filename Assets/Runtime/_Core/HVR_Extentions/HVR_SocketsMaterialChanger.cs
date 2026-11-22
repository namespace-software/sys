using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVR_SocketsMaterialChanger : MonoBehaviour
{
    [SerializeField] private Material _defMaterial;
    [SerializeField] private Material _activeMaterial;

    public void ChangeToActive(MeshRenderer mesh)
    {
        mesh.material = _activeMaterial;
    }

    public void ChangeToDefault(MeshRenderer mesh)
    {
        mesh.material = _defMaterial;
    }
}
