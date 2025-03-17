using Microsoft.Extensions.Logging;
using SFML.System;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;
using WolfRender.Models;

namespace WolfRender.Services
{
    internal class MapService : IMapService
    {
        private readonly ILogger<MapService> _logger;
        private readonly ITextureService _textureService;
        private int[,] _data { get; set; }
        private int[,] _pathData { get; set; }
        private Vector2i _size;
        public double[] WallDistances { get; set; }
        public int MapWidth => _size.X;
        public int MapHeight => _size.Y;
        public int[,] PathData => _pathData;

        private List<Vector2i> _directions = new List<Vector2i>
        {
            new Vector2i(0, 1),   // North
            new Vector2i(1, 0),   // East
            new Vector2i(0, -1),  // South
            new Vector2i(-1, 0),  // West
            new Vector2i(1, 1),   // Northeast
            new Vector2i(1, -1),  // Southeast
            new Vector2i(-1, -1), // Southwest
            new Vector2i(-1, 1)   // Northwest
        };

        public MapService(
            ILogger<MapService> logger,
            ITextureService textureService)
        {
            _logger = logger;
            _textureService = textureService;
            _logger.LogInformation("MapService starting");

            Init();
        }

        private void Init()
        {
            _data = GetMapData("level1");
            _pathData = GetPathData("level1_path");

            var textureSize = (int)Math.Sqrt(_data.Length);
            _size = new Vector2i(textureSize, textureSize);
            _logger.LogInformation("MapService initialized");
        }

        public int Get(Vector2i position)
        {
            if (position.X < 0 || position.X >= _size.X ||
                position.Y < 0 || position.Y >= _size.Y)
                return 1; // Return wall for out of bounds

            return _data[position.X, position.Y];
        }

        public int GetPath(Vector2i position)
        {
            if (position.X < 0 || position.X >= _size.X ||
                position.Y < 0 || position.Y >= _size.Y)
                return 1; // Return wall for out of bounds

            return _pathData[position.X, position.Y];
        }

        private int[,] GetMapData(string name)
        {
            _textureService.LoadTexture(name, $"{name}.bmp");
            var image = _textureService.GetTextureImage(name);
            var width = image.Size.X;
            var height = image.Size.Y;
            int[,] data = new int[width, height];

            // Convert each pixel to map data
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    var pixel = image.GetPixel((uint)x, (uint)y);

                    // Convert pixel to grayscale and threshold
                    float brightness = (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);

                    //// If pixel is darker than 50% gray, it's a wall
                    data[x, y] = brightness < 0.5f ? 1 : 0;

                    // No wall
                    if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                    {
                        data[x, y] = 0;
                    }

                    // Default wall (greystone)
                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    {
                        data[x, y] = 1;
                    }

                    // redstone
                    if(pixel.R == 255 && pixel.G == 0 && pixel.B == 0)
                    {
                        data[x, y] = 2;
                    }

                    //// wood floor
                    if (pixel.R == 185 && pixel.G == 122 && pixel.B == 87)
                    {
                        data[x, y] = 3;
                    }

                    // wood wall
                    if (pixel.R == 74 && pixel.G == 49 && pixel.B == 35)
                    {
                        data[x, y] = 4;
                    }

                    // bluestone
                    if (pixel.R == 63 && pixel.G == 72 && pixel.B == 204)
                    {
                        data[x, y] = 5;
                    }
                }
            }
            return data;
        }

        private int[,] GetPathData(string name)
        {
            _textureService.LoadTexture(name, $"{name}.bmp");
            var image = _textureService.GetTextureImage(name);
            var width = image.Size.X;
            var height = image.Size.Y;
            int[,] data = new int[width, height];

            // Convert each pixel to map data
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    var pixel = image.GetPixel((uint)x, (uint)y);

                    // Convert pixel to grayscale and threshold
                    float brightness = (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);

                    //// If pixel is darker than 50% gray, it's a wall
                    data[x, y] = brightness < 0.5f ? 1 : 0;

                    // No wall
                    if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                    {
                        data[x, y] = 0;
                    }

                    // Path tracing avoidance
                    if (pixel.R == 200 && pixel.G == 191 && pixel.B == 231)
                    {
                        data[x, y] = 6;
                    }

                    if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                    {
                        data[x, y] = (int)EntityType.Guard;
                    }

                    if (pixel.R == 255 && pixel.G == 169 && pixel.B == 122)
                    {
                        data[x, y] = (int)EntityType.Barrel;
                    }
                }
            }
            return data;
        }

        public List<Vector2f> PathFind(Vector2i from, Vector2i to)
        {
            var openSet = new List<PathNode>();
            var closedSet = new HashSet<Vector2i>();
            
            PathNode startNode = new PathNode(from);
            PathNode targetNode = new PathNode(to);
            
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                // Get node with lowest FCost
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost || 
                        (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                    {
                        currentNode = openSet[i];
                    }
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.Position);
                
                // Check if we reached the target
                if (currentNode.Position == targetNode.Position)
                {
                    return RetracePath(startNode, currentNode);
                }
                
                // Check all neighbors
                foreach (var direction in _directions)
                {
                    Vector2i neighborPos = new Vector2i(
                        currentNode.Position.X + direction.X,
                        currentNode.Position.Y + direction.Y
                    );
                    
                    // Skip if out of bounds or not walkable
                    if (!IsPositionValid(neighborPos) || !IsWalkable(neighborPos) || 
                        closedSet.Contains(neighborPos))
                    {
                        continue;
                    }
                    
                    int newGCost = currentNode.GCost + GetDistance(currentNode.Position, neighborPos);
                    
                    PathNode neighborNode = openSet.Find(n => n.Position == neighborPos);
                    
                    if (neighborNode == null)
                    {
                        neighborNode = new PathNode(neighborPos);
                        neighborNode.GCost = newGCost;
                        neighborNode.HCost = GetDistance(neighborPos, targetNode.Position);
                        neighborNode.Parent = currentNode;
                        openSet.Add(neighborNode);
                    }
                    else if (newGCost < neighborNode.GCost)
                    {
                        neighborNode.GCost = newGCost;
                        neighborNode.Parent = currentNode;
                    }
                }
            }
            
            // No path found
            return null;
        }
        
        private bool IsWalkable(Vector2i pos)
        {
            // Check if the position is a wall or other unwalkable terrain
            int tileType = GetPath(pos);
            return tileType == 0; // Assuming 0 is empty space 
        }
        
        private bool IsPositionValid(Vector2i pos)
        {
            return pos.X >= 0 && pos.X < _size.X && 
                   pos.Y >= 0 && pos.Y < _size.Y;
        }
        
        private int GetDistance(Vector2i a, Vector2i b)
        {
            // Manhattan distance for 4-directional movement
            // return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
            
            // Diagonal distance for 8-directional movement
            int dstX = Math.Abs(a.X - b.X);
            int dstY = Math.Abs(a.Y - b.Y);
            return (dstX > dstY) ? 
                14 * dstY + 10 * (dstX - dstY) : 
                14 * dstX + 10 * (dstY - dstX);
        }
        
        private List<Vector2f> RetracePath(PathNode startNode, PathNode endNode)
        {
            List<Vector2f> path = new List<Vector2f>();
            PathNode currentNode = endNode;
            
            while (currentNode != startNode)
            {
                // Convert to Vector2f and add 0.5f to center in tile
                path.Add(new Vector2f(
                    currentNode.Position.X + 0.5f,
                    currentNode.Position.Y + 0.5f
                ));
                currentNode = currentNode.Parent;
            }
            
            path.Add(new Vector2f(startNode.Position.X + 0.5f, startNode.Position.Y + 0.5f));
            path.Reverse();
            return path;
        }
    }
}
