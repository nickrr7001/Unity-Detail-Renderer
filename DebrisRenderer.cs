using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisRenderer : MonoBehaviour
{
    public Mesh meshToRender;
    public int seed = 1000;
    public int density = 100;
    public int renderRange = 50;
    public float debrisRandomness = 3f;

    public Transform toRenderAround;
    public Terrain ourTerrain;

    public debrisSet[] debris;

    public int debrisSpawnChance = 4;
    public int deprisSpawnDistance = 1;

    private int curFrame = 0;
    private int RenderDelay = 10;//10 means recalculate what to render every 10th frame
    private void Update()
    {
        bool rerender = curFrame == 0;
        curFrame = curFrame == RenderDelay ? 0 : curFrame + 1;
        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].renderDebris(toRenderAround,renderRange,ourTerrain, rerender,i);
        }
    }
}
[System.Serializable]
public class debrisSet
{
    public Mesh mesh;
    public Material mat;
    private List<Matrix4x4> matrix = new List<Matrix4x4>();
    public int deprisSpawnDistance;
    public int debrisSpawnChance;
    public int debrisRandomness;
    public void renderDebris(Transform toRenderAround, int renderRange, Terrain ourTerrain, bool reCalculate, int index_seed)
    {
        if (!reCalculate)
        {
            List<Matrix4x4> recalc = new List<Matrix4x4>(matrix);
            while (recalc.Count > 1023)
            {
                Graphics.DrawMeshInstanced(mesh, 0, mat, recalc.GetRange(0, 1023));
                recalc.RemoveRange(0, 1023);
            }
            Graphics.DrawMeshInstanced(mesh, 0, mat, recalc);
            return;
        }
        matrix = new List<Matrix4x4>();
        int xPosMin = (int)(toRenderAround.position.x - renderRange);
        int xPosMax = (int)(toRenderAround.position.x + renderRange);
        int zPosMin = (int)(toRenderAround.position.z - renderRange);
        int zPosMax = (int)(toRenderAround.position.z + renderRange);
        for (int i = xPosMin; i < xPosMax; i += deprisSpawnDistance)
        {
            for (int j = zPosMin; j < zPosMax; j += deprisSpawnDistance)
            {
                Random.InitState(index_seed+(i * j));
                if (Random.Range(0, debrisSpawnChance) != 1)
                    continue;
                Vector3 origin = new Vector3(i, 0, j);
                origin.x += Random.Range(-debrisRandomness, debrisRandomness);
                origin.z += Random.Range(-debrisRandomness, debrisRandomness);
                origin.y = ourTerrain.terrainData.GetHeight((int)origin.x * 4, (int)origin.z * 4);
                Quaternion rot = Quaternion.FromToRotation(Vector3.up,
                    new Vector3(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2))
                    * Random.Range(-1f, 1f));
                matrix.Add(Matrix4x4.TRS(origin, rot, Vector3.one));
            }
        }
        List<Matrix4x4> matrixCopy = new List<Matrix4x4>(matrix);
        while (matrixCopy.Count > 1023)
        {
            Graphics.DrawMeshInstanced(mesh, 0, mat, matrixCopy.GetRange(0, 1023));
            matrixCopy.RemoveRange(0, 1023);
        }
        Graphics.DrawMeshInstanced(mesh, 0, mat, matrixCopy);
    }
}
