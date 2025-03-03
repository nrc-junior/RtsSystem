using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MapDrawer : MonoBehaviour
{
    public class Pixel{
        public int x = 0;
        public int y = 0;
        public Color color = Color.black;

        public Pixel(int x, int y, Color color){
            this.x = x;
            this.y = y;
            this.color = color;
        }

    }
    public float step = 10;
    [HideInInspector] public TerrainData terrain;
    public Color deep;
    public Color shalow;
    int res;
    public float[,] Generate() {
        
        terrain = Terrain.activeTerrain.terrainData;

        res = terrain.heightmapResolution;
        float[,] allHeights = terrain.GetHeights(0,0,res,res);
        
        Vector3[] points = new Vector3[res*res];
        float maxHeight = terrain.size.y;

        float min = float.MaxValue; // minimo global
        float max = float.MinValue; // maximo global
        
        int index = 0;
        float[,] map = new float[res,res];

        for (int i = 0; i < res; i++) {
            for (int j = 0; j < res; j++) {
                float h = allHeights[i, j];
                min = h < min ? h : min;
                max = h > max ? h : max;
                
                points[index++] = new Vector3(i,j,h);
            }
        }

        min *= maxHeight;
        max *= maxHeight;

        // paint steps
        Texture2D canvas = new Texture2D(res,res);
        float stepSize = max / step;
        
        for (int i = 0; i < points.Length; i++) {
            float localheight = points[i].z *= maxHeight;
            float minLocal = stepSize * Mathf.RoundToInt(localheight / stepSize); 
            float maxLocal = minLocal + stepSize;
            
            // step effect
            float halfStep = maxLocal - (stepSize / 2);
            localheight = localheight < halfStep ? minLocal : maxLocal;
            
            // paint
            Color col = Color.Lerp(deep, shalow, Mathf.InverseLerp(min, max, localheight));
            map[(int) points[i].x, (int) points[i].y] = localheight;
            canvas.SetPixel((int) points[(points.Length-1) -i].x, (int) points[i].y, col);
        }

        byte[] file = canvas.EncodeToPNG();
        string path = Application.dataPath + "/Scripts/Terrain/";
        File.WriteAllBytes(path + "generateMap" + ".png", file);
        return map;
    }
    
    public void Draw(Pixel[] pixelList, string artName = "draw") {
        Texture2D canvas = new Texture2D(res,res);
        
        foreach (var pixel in pixelList){
            canvas.SetPixel(pixel.x, pixel.y, pixel.color);
        }

        byte[] file = canvas.EncodeToPNG();
        string path = Application.dataPath + "/Scripts/Terrain/";
        File.WriteAllBytes(path + artName + ".png", file);
    }



}
