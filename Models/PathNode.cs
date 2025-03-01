using SFML.System;

public class PathNode
{
    public Vector2i Position { get; set; }
    public int GCost { get; set; }  // Distance from start
    public int HCost { get; set; }  // Distance to end (heuristic)
    public int FCost => GCost + HCost;
    public PathNode Parent { get; set; }

    public PathNode(Vector2i pos)
    {
        Position = pos;
    }
} 