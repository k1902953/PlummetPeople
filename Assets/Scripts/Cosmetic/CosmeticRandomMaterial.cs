using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticRandomMaterial : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials = new List<Material>();

    private void Awake()
    {
        GetComponent<SkinnedMeshRenderer>().material = materials[Random.Range(0, materials.Count)];
    }
}
