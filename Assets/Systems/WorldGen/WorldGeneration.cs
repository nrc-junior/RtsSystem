using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;
using System.Linq;


namespace NRC.World {

    [Flags]
    public enum Landmark{
        Hot = 1,
        Temperate = 2,
        Cold = 4,
        Beach = 8,
        Center = 16,
        Ignore_Water = 32 
    }

   public class Chunk{
        public class ChunkSettings{
            public bool isOcean; 
            public bool isBeach; 
        }
        
        public int x,y;

        public ChunkSettings settings = new ChunkSettings();
        public static Point[,] debug;
        public Point[,] points;

        public List<Point> allPoints = new List<Point>();
    }

    public class Point{
        public int x;
        public int y;

        public Chunk parent;
        public bool isOcean;
        public float temperature;
        public Transform texel;
    }


    public class WorldGeneration : MonoBehaviour {
        public int seed = 24;

        public enum MapType {TINY, MEDIUM, LARGE}
        public MapType mapSize;

        public const short TINY = 1024;
        public const short MEDIUM = 2048;
        public const short LARGE = 4096;

        /// <summary> de preferencia multiplo de 2 </summary>
        public int chunkSize = 16;
        float[,] heights;
        int heightmapResolution;
        float texelSize;

        GameObject tObj;
        TerrainData tData;
        Terrain terrain;

        Chunk[,] chunks;
        List<Chunk> allChunks = new();    

        [Header ("Textures")]
        [SerializeField] TerrainLayer grassy;
        [SerializeField] TerrainLayer beach;
        [SerializeField] TerrainLayer ocean;
        [SerializeField] TerrainLayer snow;
        [SerializeField] TerrainLayer desert;
        
        float[,,] alphamaps;
        int alphamapRes;
        int chunksCount;


        int cx,cy; // debug

        void Awake() {
            Random.InitState(seed);
            if(!tObj){
                tObj = new GameObject("~ world gen terrain"); 
                tData = new TerrainData();
                terrain = tObj.AddComponent<Terrain>();
                
                terrain.terrainData = tData;
                tObj.AddComponent<TerrainCollider>().terrainData = tData;
                terrain.materialTemplate = Resources.Load("DefaultTerrainMaterial") as Material;
                
                tData.terrainLayers = new TerrainLayer[5]{
                    grassy, beach, ocean, snow, desert
                };
                
                LeanTween.init( 16640 );
            }

            SetWorldSize();
            GenerateIsland();
            List<Chunk> beaches = new List<Chunk>();

            chunksCount = chunks.GetLength(0);
            for (int x = 0; x < chunksCount; x++){
                for (int y = 0; y < chunksCount; y++){
                    Chunk chunk = chunks[x,y];
                    chunk.x = x;
                    chunk.y = y;
                    if(IsBeach(chunk)) beaches.Add(chunk);
                }   
            }

            // * tera por todas as praias e procura por praias que nao tem oceano ao redor.
            foreach(var beach in beaches){
                Chunk[] neighboors = GetMatrixNeighboors<Chunk>(chunks, beach.x,beach.y, null);
                bool anyOcean = false;
                
                foreach (var neighboor in neighboors){
                    if(neighboor.settings.isOcean){
                        anyOcean = true;
                        break;
                    }
                }

                // seta altura 
                if(!anyOcean){
                    beach.settings.isBeach = false;
                    for (int x = 0; x < chunkSize; x++){
                        for (int y = 0; y < chunkSize; y++){
                            int worldX = beach.points[x,y].x;
                            int worldY = beach.points[x,y].y;

                            heights[worldX,  worldY] = .5f;
                            float temp = beach.points[x,y].temperature;

                            int temperatureIndex = temp < .33f ? 3 :  temp > .66f ? 4 : 0; 
                            if(worldX < alphamapRes && worldY < alphamapRes){
                                alphamaps[worldX, worldY, temperatureIndex] = 1;
                                alphamaps[worldX,worldY,2] = 0;
                            }
                        }
                    }
                }
            }

            foreach(var beach in beaches){
                if(beach.settings.isBeach){
                    PaintBeach(beach.points, 1);
                }
            }

            tData.SetHeights(0,0,heights);
            tData.SetAlphamaps(0,0,alphamaps);

            int size = chunks[0,0].points.GetLength(0);
            Chunk.debug = new Point[size,size];

            for (int x = 0; x < size; x++){
                for (int y = 0; y < size; y++){
                    
                    Point point = chunks[0,0].points[x,y];
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    point.texel = obj.transform;
                    Chunk.debug[x,y] = new Point();
                    Chunk.debug[x,y].texel = obj.transform;
                    point.texel.position = new Vector3(texelSize *  point.y, 20, texelSize * point.x);
                    obj.SetActive(true);
                }
            }

            // WaveFunctionCollapse();
            MinhaFunçãoQuisifoda();
        }

           void MinhaFunçãoQuisifoda(){
            Entropy[] entropies = GetEntropies();
            MapDrawer texGen = GetComponent<MapDrawer>();
            
            float[,] map;
            map = texGen.Generate();
            int res = map.GetLength(0);

            List<Tile> avaiables = new List<Tile>();
            Tile[,] tiles = new Tile[res,res];
            
            for (int x = 0; x < res; x++){
                for (int y = 0; y < res; y++){
                    Tile tile = new Tile(x,y);

                    if( map[x,y] > 0 ){
                        tile.isValid = true;
                        tile.possibleEntropies = new List<Entropy>(entropies);
                        avaiables.Add(tile);
                    }

                    tiles[x,y] = tile;
                }
            }

            Tile nextTile;
            List<Tile> costlySpread = new List<Tile>();
            List<Tile> freeSpread = new List<Tile>();
            Tile[] neighboors;
            Entropy entropy;
            List<List<Tile>> groups = new();
            
            while((nextTile = GetNextTileAvaiable()) != null){ 
                List<Tile> entropyGroup = new();
                
                SpreadTile(nextTile, entropyGroup);
                groups.Add(entropyGroup);
                break;
            }
            
            // texGen.Draw(GetEntropyColors(), "entropyMap");

            // local mehtods
            Tile GetNextTileAvaiable() => avaiables.Find(tile => tile.isValid && tile.entropy == null);

            void SpreadTile(Tile tile, List<Tile> group){
                tile.entropy = new(Entropy.Type.None, 0, new());
                Debug.LogWarning("Todo: sistema de geracao do mundo");
            //     group.Add(tile);
            //     neighboors = GetMatrixNeighboors<Tile>(tiles, tile.x, tile.y, null);


            }
        }


        bool IsBeach(Chunk chunk){
            bool haveOcean = false;
            bool haveContinent = false;

            Point[,] points = chunk.points;
            
            for (int x = 0; x < chunkSize; x++){
                for (int y = 0; y < chunkSize; y++){   
                    if(points[x,y] == null) continue;

                    haveOcean |= points[x,y].isOcean;
                    haveContinent |= !points[x,y].isOcean;
                }
            }
            
            chunk.settings.isBeach = haveOcean && haveContinent;
            chunk.settings.isOcean = haveOcean && !haveContinent;
            return haveOcean && haveContinent;
        }

        bool PaintBeach(Point[,] chunk, int splatIndex){

            for (int x = 0; x < chunkSize; x++){
                for (int y = 0; y < chunkSize; y++){   
                    if(chunk[x,y].isOcean || chunk[x,y].temperature < .33f ) continue;
                    alphamaps[ (int) chunk[x,y].x , (int) chunk[x,y].y, 0] = 0;
                    alphamaps[ (int) chunk[x,y].x , (int) chunk[x,y].y, 4] = 0;
                    alphamaps[ (int) chunk[x,y].x , (int) chunk[x,y].y,splatIndex] = 1;
                }
            }
            return false;
        }
        




        #region Geração de Mundo
        public void SetWorldSize(){
            switch (mapSize){
                case MapType.TINY: 
                    heightmapResolution = 513;
                    tData.heightmapResolution = 513;
                    tData.alphamapResolution = 512;
                    texelSize = (float) TINY / (float) heightmapResolution;
                    tData.size = (new Vector3(TINY, 16, TINY)); 
                break;
                case MapType.MEDIUM: 
                    heightmapResolution = 1025;
                    tData.heightmapResolution = 1025;
                    tData.alphamapResolution = 1024;
                    texelSize = (float) MEDIUM / (float) heightmapResolution;
                    tData.size = (new Vector3(MEDIUM, 16, MEDIUM)); 
                break;
                case MapType.LARGE: 
                    heightmapResolution = 2049;
                    tData.heightmapResolution = 2049;
                    tData.alphamapResolution = 2048;
                    texelSize = (float) LARGE / (float) heightmapResolution;
                    tData.size = (new Vector3(LARGE, 16, LARGE)); 
                break;
            }
            
            alphamapRes =  tData.alphamapResolution;
            alphamaps = new float[alphamapRes, alphamapRes, 5]; 
        }
        
        class Triangle{ public Vector2 a,b,c; }

        public void GenerateIsland(){
            int quadrantSize = Mathf.RoundToInt(heightmapResolution/3);
            Vector2[] points = new Vector2[9];
            Vector2[] borders = new Vector2[8];
            int randomPointIdx = 0;
            int borderIdx = 0;

            // * generate the random points to solve as border
            for (var ver = 1; ver <= 3; ver++){ 
                for (var hor = 1; hor <= 3; hor++){
                    
                    int x = Random.Range(quadrantSize * (hor-1) , quadrantSize * hor);
                    int y = Random.Range(quadrantSize * (ver-1) , quadrantSize * ver);

                    x = Mathf.Max(x, 10);
                    y = Mathf.Max(y, 10);
                    x = Mathf.Min(x, heightmapResolution-10);
                    y = Mathf.Min(y, heightmapResolution-10);

                    Vector2 p = new Vector2(x, y);

                    bool isCenter = randomPointIdx == 4;
                    points[randomPointIdx++] = p;    
                    if(!isCenter) borders[borderIdx++] = new Vector2(p.y, p.x);
                } 
            }
            
            // * generate triangles to mesh the island
            Triangle[] islandShape = new Triangle[]{
                new Triangle(){a = points[0], b = points[2], c= points[5] },
                new Triangle(){a = points[0], b = points[5], c= points[8] },
                new Triangle(){a = points[0], b = points[8], c= points[7] },
                new Triangle(){a = points[0], b = points[6], c= points[7] },
                new Triangle(){a = points[0], b = points[3], c= points[6] }
            };
            
            // * sample a border to set as cold and the opposite side will be the hotter
            int p1 = Random.Range(0,borders.Length);
            int p2 = -1;
            float distanceToP2 = -1;

            for (var i = 0; i < borders.Length; i++){
                if(i == p1) continue;
                float dst = Vector2.Distance(borders[i], borders[p1]);
                if(dst > distanceToP2){
                    distanceToP2 = dst;
                    p2 = i;
                }
            }

            Debug.Log($"p1 is {p1}, p2 is {p2}, {distanceToP2}");
            // chunkSize *= texelSize;

            heights = new float[heightmapResolution,heightmapResolution];
            

            List<Point> tmpPoints = new List<Point>();
            for (int x = 0; x < heightmapResolution; x++) {
                for (int y = 0; y < heightmapResolution; y++) {
                    Vector2 coord = new Vector2(x,y);
                    Point point = new Point();
                    bool isContinent = IsPointInsideShape(coord, islandShape);
                    if(heights[x,y]>0) continue; 
                    heights[x,y] =  isContinent ? .5f : 0;
                    
                    // valor de 0 a 1, sendo que x < .33 é frio,  x > .66 quente
                    float temperature = Mathf.InverseLerp(0, distanceToP2, Vector2.Distance(coord, borders[p1]));
                    point.temperature = temperature;

                    
                    if(coord == borders[p1] || coord == borders[p2]){
                        heights[x,y] = 1f;
                    }else if(isContinent) {
                        point.isOcean = false;
                        
                        int temperatureIndex = temperature < .33f ? 3 :  temperature > .66f ? 4 : 0; 
                        if(x < alphamapRes && y < alphamapRes)
                            alphamaps[x,y,temperatureIndex] = 1;

                        // * mostra temperatura da celula
                        // GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        // LeanTween.color(obj, Color.Lerp(Color.blue, Color.red, temperature),0);
                        // Transform trans = obj.transform;
                        // trans.localScale = Vector3.one * 0.1f;
                        // trans.position = new Vector3(texelSize *  y, 16, texelSize * x);
                    }else{
                        if(x < alphamapRes && y < alphamapRes)
                            alphamaps[x,y,2] = 1;
                        
                        point.isOcean = true;
                    }
                    
                    
                    
                    point.x = (int) coord.x;
                    point.y = (int) coord.y;
                    tmpPoints.Add(point);
                }
            }

            int chuncksCount = heightmapResolution/chunkSize;
            chunks = new Chunk[chuncksCount+1,chuncksCount+1];

            for (var x = 0; x < chunks.GetLength(0); x++){
                for (var y = 0; y < chunks.GetLength(0); y++){
                    chunks[x,y] = new Chunk();
                    chunks[x,y].points = new Point[chunkSize,chunkSize];
                    allChunks.Add(chunks[x,y]);
                }
            }

            foreach (Point p in tmpPoints){
                int chunkX = (int) Mathf.Floor(p.x/chunkSize); // .. chunk x
                int xInChunckIdx = (int) p.x % chunkSize;
                int chunkY = (int) Mathf.Floor(p.y/chunkSize); // .. chunk y
                int yInChunckIdx = (int)p.y % chunkSize;
                
                chunks[chunkX, chunkY].allPoints.Add(p);
                chunks[chunkX, chunkY].points[xInChunckIdx, yInChunckIdx] = p;
            }


            // * local methods * //

            // detecta se cada ponto está no continente
            bool IsPointInsideShape(Vector2 p, Triangle[] triangles) {
                bool isInShape = false;
                int count = 0;

                for (var i = 0; i < triangles.Length; i++){
                    Triangle t = triangles[i];
                    isInShape = PointInTriangle(p, t.a, t.b, t.c);
                    count += isInShape ? 1 : 0;
                }

                return !(count % 2 == 0);
            }   
                
        }

        float sign (Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        bool PointInTriangle (Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;
            Vector2 v1i = new Vector2(v1.y, v1.x);
            Vector2 v2i = new Vector2(v2.y, v2.x);
            Vector2 v3i = new Vector2(v3.y, v3.x);
            
            d1 = sign(pt, v1i, v2i);
            d2 = sign(pt, v2i, v3i);
            d3 = sign(pt, v3i, v1i);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
        #endregion

        /// <returns>Starting bottom left, get in clockwise direction the neighboors of chunk. (NULLABLE)</returns>
        T[] GetMatrixNeighboors<T>(T[,] matrix, int x, int y, T @default){
            List<T> matNeighboors = new List<T>();

            if(x-1 >= 0 && y - 1 >= 0){
                matNeighboors.Add(matrix[x-1,y-1]);
            }else{
                matNeighboors.Add(@default);
            }

            if(x-1 >= 0){
                matNeighboors.Add(matrix[x-1,y]);

                if(y + 1 < chunksCount){
                    matNeighboors.Add(matrix[x+1,y+1]);
                }else{
                    matNeighboors.Add(@default);
                }
            }else{
                matNeighboors.Add(@default);
                matNeighboors.Add(@default);
            }
            
            if(y + 1 < chunksCount){
                matNeighboors.Add(matrix[x,y+1]);
                if(x + 1 < chunksCount){
                    matNeighboors.Add(matrix[x+1,y+1]);
                }else{
                    matNeighboors.Add(@default);
                }

            }else{
                matNeighboors.Add(@default);
                matNeighboors.Add(@default);
            }

            if(x+1 < chunksCount){
                matNeighboors.Add(matrix[x+1,y]);
                if(y-1 >= 0){
                    matNeighboors.Add(matrix[x+1,y-1]);
                }else{
                    matNeighboors.Add(@default);
                }
            }else{
                matNeighboors.Add(@default);
                matNeighboors.Add(@default);
            }

            if(y-1 >= 0 ){
                matNeighboors.Add(matrix[x,y-1]);
            }else{
                matNeighboors.Add(@default);
            }

            return matNeighboors.ToArray();
        }

        enum Model {beach };
        


        // class WaveRule {
        //     internal Model[] neighboors;
        //     internal int value {get => Random.Range(0,values.Length); set => values = new int[1]{value};}
        //     internal int[] values; 
        //     internal int propagation;
        //     internal int propagated;
        // };
        
        // Dictionary<Model, WaveRule> models = new Dictionary<Model, WaveRule>(){
        //     {Model.beach, new WaveRule(){}}
        // };

        
        // class CollapsingEntropy {
        //     Entropy.Type type;
        // }

        [System.Serializable] public class EntropyCreator {
            public Entropy.Type type;
            public Color color = new Color(0.5f,0.5f,1f,1f);
            
            public int lifetime;
            public Entropy.Type neighboors;
        }

        public class EntropyComparer : IEqualityComparer<Entropy>{
            public bool Equals(Entropy x, Entropy y) => x.type == y.type;
            public int GetHashCode(Entropy obj) => obj == null ? 0 : obj.GetHashCode();
        }

        public class Entropy {
            [Flags] public enum Type {
                None = 0,
                mountains = 1, 
                plains = 2, 
                mountainsWithTrees = 4, 
                plainsWithTrees = 8 
            }

            internal Type type = Type.None; 
            internal List<Type> spread = null; 
            internal int lifetime = 0;

            internal Entropy(Type type, int lifetime, List<Entropy.Type> spread){
                this.type = type;
                this.lifetime = lifetime;
                if(!spread.Contains(type)) spread.Add(type);
                this.spread = spread;
            }

            public bool Equals(Type x, Type y){
                return x == y;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        Entropy[] GetEntropies(){
            List<Entropy> entropies = new();
                
            foreach (var data in entropiesData){
                List<Entropy.Type> spreadInfo = new();

                foreach (Entropy.Type value in Enum.GetValues(typeof(Entropy.Type))){
                    if (data.neighboors.HasFlag(value)){
                        spreadInfo.Add(value);
                    }
                }

                entropies.Add(new Entropy(data.type, data.lifetime, spreadInfo));    
            }

            return entropies.ToArray();
        }

        class Tile {
            internal bool isValid = false;
            internal bool occuped = false;
            internal int x;
            internal int y;

            internal Entropy entropy = null;
            internal Entropy baseEntropy = null;
            internal List<Entropy> possibleEntropies;
            internal List<int> neverReturnTo;

            internal Tile(int x, int y){
                this.x = x; this.y = y;
            }
        }

        [UnityEngine.Serialization.FormerlySerializedAs("entropyColors")] public EntropyCreator[] entropiesData = new EntropyCreator[0];
        
    void WaveFunctionCollapse(){
            #region hide
            Entropy[] entropies = GetEntropies();
            MapDrawer texGen = GetComponent<MapDrawer>();
            
            float[,] map;
            map = texGen.Generate();
            int res = map.GetLength(0);

            List<Tile> avaiables = new List<Tile>();
            Tile[,] tiles = new Tile[res,res];
            
            for (int x = 0; x < res; x++){
                for (int y = 0; y < res; y++){
                    Tile tile = new Tile(x,y);

                    if( map[x,y] > 0 ){
                        tile.isValid = true;
                        tile.possibleEntropies = new List<Entropy>(entropies);
                        avaiables.Add(tile);
                    }

                    tiles[x,y] = tile;
                }
            }

            Tile nextTile;
            List<Tile> costlySpread = new List<Tile>();
            List<Tile> freeSpread = new List<Tile>();
            Tile[] neighboors;
            Entropy entropy;

            while((nextTile = GetNextTileAvaiable()) != null){ 
                Wave(nextTile);
            }
            
            texGen.Draw(GetEntropyColors(), "entropyMap");
            #endregion
            // local mehtods
            
            void Wave(Tile tile){
                freeSpread.Clear();
                costlySpread.Clear();

                neighboors = GetMatrixNeighboors<Tile>(tiles, tile.x, tile.y, null);
                entropy = tile.entropy;

                if (entropy == null){ // cria entropia pro ponto
                    tile.baseEntropy = tile.possibleEntropies[Random.Range(0,tile.possibleEntropies.Count)];

                    entropy = NewCopyEntropy();
                    tile.entropy = entropy;
                }
                
                // * entropia garantida, agora precisa informar aos vizinhos 

                foreach (var neighboor in neighboors){ // spread to neighboors
                    if(!neighboor.isValid) continue;
                   
                    neighboor.possibleEntropies.RemoveAll(x => !tile.entropy.spread.Contains(x.type));
                    
                    if(neighboor.entropy == null){
                        costlySpread.Add(neighboor);
                    }else if(neighboor.entropy.type == tile.entropy.type){
                        int bannedHash = neighboor.GetHashCode();
                        if(!tile.neverReturnTo.Contains(bannedHash)){ //! verifica se ja passou ali, se ja, forgeddabuth.
                            tile.neverReturnTo.Add(bannedHash);
                            neighboor.neverReturnTo.Add(tile.GetHashCode());
                            freeSpread.Add(neighboor);
                        }
                    }
                }
                
                int spreadingLifetime = entropy.lifetime - costlySpread.Count;
                entropy.lifetime = 0;
                
                foreach (var neighboor in costlySpread){
                  
                }
                
                bool collapsed = (spreadingLifetime == 0 || costlySpread.Count == 0);
                tiles[tile.x,tile.y] = tile;

                if(collapsed){
                    return; // wave collapse 
                }else{
                    foreach (var neighboor in costlySpread){
                        Wave(neighboor);
                    }
                }

                // local methods
                Entropy NewCopyEntropy() => new Entropy(tile.baseEntropy.type, tile.baseEntropy.lifetime, tile.baseEntropy.spread);
            }



            Tile GetNextTileAvaiable() {
                return avaiables.Find(tile => tile.isValid && tile.entropy == null);
                //avaiables[Random.Range(0, avaiables.Count)];
            }


            // Entropy.Type GetTileEntropy(Tile[] neighboors){
            //     // ! Entropy.Type entropies;
                
            //     foreach (var tile in neighboors){
            //         if(tile == null || !tile.isValid || tile.entropy == null) continue;    
                    
            //     }

            //     return Entropy.Type.None;
            // }
            MapDrawer.Pixel[] GetEntropyColors(){

                Dictionary<Entropy.Type, Color> colors = new();
                List<MapDrawer.Pixel> pixelList = new();
                
                foreach (var item in this.entropiesData){
                    colors.Add(item.type, item.color);
                }

                for (int x = 0; x < res; x++){
                    for (int y = 0; y < res; y++){
                        Color col = Color.black;
                        
                        if(tiles[x,y].entropy != null){
                            col = colors[tiles[x,y].entropy.type];
                        }

                        pixelList.Add(new MapDrawer.Pixel(x,y,col));
                    }
                }

                return pixelList.ToArray();
            }
        }

        public Chunk GetLandmark(Landmark filter){
            List<Chunk> possibleChunks = new(allChunks);
            // < .33 é frio,  x > .66 
            foreach (var chunk in allChunks){
                var points = chunk.allPoints;
                
                bool skip = false;
                    foreach (var point in points)
                    {
                        float temp = point.temperature;

                        if(filter.HasFlag(Landmark.Hot) && temp < .66f){
                            skip = true;
                            break;
                        }
                            
                        if(filter.HasFlag(Landmark.Cold) && temp > .33f){
                            skip = true;
                            break;
                        }

                        if(filter.HasFlag(Landmark.Temperate) && (temp > .66f || temp < .33f)){
                            skip = true;
                            break;
                        }

                        if(filter.HasFlag(Landmark.Ignore_Water) && point.isOcean){
                            skip = true;
                            break;
                        }


                    }

                if(skip){
                    possibleChunks.Remove(chunk);
                }
            }
            return possibleChunks.Count == 0 ? null : possibleChunks[Random.Range(0, possibleChunks.Count)];
        }


        // * ignore
        void Update(){
            DebugChunk();
        }

        void DebugChunk(){
            if(Input.GetKeyDown(KeyCode.I))
                Awake();

            if(Input.GetKeyDown(KeyCode.Keypad1)){
                cx--;
                cy--;
                
                if(cx<0) cx++;
                if(cy<0) cy++;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad2)){
                cy--;

                if(cy<0) cy++;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad3)){
                cx++;
                cy--;

                if(cx>=chunks.GetLength(0)-1) cx--;
                if(cy<0) cy++;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad4)){
                cx--;

                if(cx<0) cx++;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad5)){
                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad6)){
                cx++;
                
                if(cx>=chunks.GetLength(0)-1) cx--;
                
                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad7)){
                cx--;
                cy++;

                if(cx<0) cx++;
                if(cy>=chunks.GetLength(0)-1) cy--;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad8)){
                cy++;

                if(cy>=chunks.GetLength(0)-1) cy--;

                DebugChunk(cx,cy);
            }
            if(Input.GetKeyDown(KeyCode.Keypad9)){
                cx++;
                cy++;

                if(cx>=chunks.GetLength(0)-1) cx--;
                if(cy>=chunks.GetLength(0)-1) cy--;

                DebugChunk(cx,cy);
            }
        }
        void DebugChunk(int c_y, int c_x){
            Debug.Log($"{c_x},{c_y}");
            for (int x = 0; x < chunks[c_x,c_y].points.GetLength(0); x++){
                for (int y = 0; y < chunks[c_x,c_y].points.GetLength(0); y++){
                    
                    Point point = chunks[c_x,c_y].points[x,y];
                    Chunk.debug[x,y].texel.position = new Vector3(texelSize *  point.y, 20, texelSize * point.x);
                    Debug.Log(point.temperature);
                }
            }
        }

        public Vector3 ToWorldPosition(Point p){
            return new Vector3(texelSize * p.y, tData.GetHeight(p.y, p.x), texelSize * p.x);
        }
    }
}