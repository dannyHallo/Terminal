﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseDensity : MonoBehaviour
{
    private TerrainMesh terrainMesh
    {
        get
        {
            return gameObject.GetComponent<TerrainMesh>();
        }
    }

    [Header("Noise")]
    public int seed;

    [Range(0, 1)]
    public float DragMeToUpdate;

    [Range(1, 20)]
    public int numOctaves = 4;

    [Range(1, 2)]
    public float lacunarity = 2;

    [Range(0, 1)]
    public float persistence = .5f;

    public float noiseScale = 0.1f;

    [Range(0, 3f)]
    public float heightGredient = 1;

    [Range(0, 3f)]
    public float multiFractalWeight = 1;
    public float planetRadius = 50;
    public float noiseWeight = 1;
    public bool closeEdges;

    public bool b1;
    public bool b2;
    public float f1;
    public float f2;
    public float f3;

    public float floorOffset = 1;

    public Vector4 shaderParams;

    ComputeBuffer debugBuffer;

    public ComputeShader noiseDensityShader;

    protected List<ComputeBuffer> buffersToRelease;

    [Header("Debug")]
    public bool editorUpdate = false;

    void OnValidate()
    {
        if (editorUpdate && !Application.isPlaying && terrainMesh)
        {
            terrainMesh.RequestMeshUpdate();
        }
    }

    public void CalculateChunkNoise(
        Chunk chunk,
        ComputeBuffer pointsBuffer,
        ComputeBuffer additionalPointsBuffer,
        ComputeBuffer pointsStatus,
        Vector3 worldSize
    )
    {
        int numPoints = chunk.numPointsPerAxis * chunk.numPointsPerAxis * chunk.numPointsPerAxis;
        int numThreadsPerAxis = Mathf.CeilToInt(chunk.numPointsPerAxis / 8.0f);
        buffersToRelease = new List<ComputeBuffer>();

        // Points buffer is populated inside shader with pos (xyz) + density (w).

        var prng = new System.Random(seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++)
        {
            // What does it mean by ( * 2 - 1 )?
            offsets[i] =
                new Vector3(
                    (float)prng.NextDouble() * 2 - 1,
                    (float)prng.NextDouble() * 2 - 1,
                    (float)prng.NextDouble() * 2 - 1
                ) * offsetRange;
        }

        // Sets offset buffer
        ComputeBuffer offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);

        offsetsBuffer.SetData(offsets);
        buffersToRelease.Add(offsetsBuffer);

        noiseDensityShader.SetVector("centre", chunk.centre);
        noiseDensityShader.SetVector("worldSize", worldSize);
        noiseDensityShader.SetVector("params", shaderParams);

        noiseDensityShader.SetBool("b1", b1);
        noiseDensityShader.SetBool("b2", b2);
        noiseDensityShader.SetBool("closeEdges", closeEdges);

        noiseDensityShader.SetInt("octaves", Mathf.Max(1, numOctaves));
        noiseDensityShader.SetInt("numPointsPerAxis", chunk.numPointsPerAxis);

        noiseDensityShader.SetFloat("boundsSize", chunk.boundSize);
        noiseDensityShader.SetFloat("spacing", chunk.pointSpacing);
        noiseDensityShader.SetFloat("lacunarity", lacunarity);
        noiseDensityShader.SetFloat("persistence", persistence);
        noiseDensityShader.SetFloat("noiseScale", noiseScale);
        noiseDensityShader.SetFloat("noiseWeight", noiseWeight);
        noiseDensityShader.SetFloat("f1", f1);
        noiseDensityShader.SetFloat("f2", f2);
        noiseDensityShader.SetFloat("f3", f3);
        noiseDensityShader.SetFloat("floorOffset", floorOffset);
        noiseDensityShader.SetFloat("radius", planetRadius);
        noiseDensityShader.SetFloat("heightGredient", heightGredient);
        noiseDensityShader.SetFloat("multiFractalWeight", multiFractalWeight);

        noiseDensityShader.SetBuffer(1, "offsets", offsetsBuffer);
        noiseDensityShader.SetBuffer(1, "points", pointsBuffer);
        noiseDensityShader.SetBuffer(1, "manualData", additionalPointsBuffer);
        noiseDensityShader.SetBuffer(1, "GroundLevelDataBuffer", chunk.groundLevelDataBuffer);
        noiseDensityShader.SetBuffer(1, "pointsStatus", pointsStatus);

        noiseDensityShader.Dispatch(1, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        foreach (ComputeBuffer buffer in buffersToRelease)
        {
            buffer.Release();
        }
    }
}
