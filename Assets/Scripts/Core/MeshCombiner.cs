using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    [ContextMenu("Bake Meshes")]
    public void BakeMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int combineIndex = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject == gameObject) continue;

            combine[combineIndex].mesh = meshFilters[i].sharedMesh;
            combine[combineIndex].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;

            combineIndex++;
        }

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();

        filter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        filter.sharedMesh.CombineMeshes(combine, true, true);
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Debug.Log($"Mesh baking selesai. {combineIndex} sub-meshes digabungkan. {childCount} child hierarki dibersihkan.");
    }
}