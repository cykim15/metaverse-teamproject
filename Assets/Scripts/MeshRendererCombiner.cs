using UnityEngine;

public class MeshRendererCombiner : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        CombineMeshRenderers();
    }

    void CombineMeshRenderers()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine, true, true);

        meshFilter.sharedMesh = combinedMesh;
    }
}