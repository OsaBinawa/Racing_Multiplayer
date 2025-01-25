using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SAP2D
{
    public class PathVertexGenerate : MonoBehaviour
    {
        public int gridIndex = 0;
        public SAP2DPathfinder pathfinder;

        public Text uiText;
        // public TextMeshProUGUI uiText; // If using TextMeshPro

        public Vector3 startPoint;
        public Vector3 endPoint;

        void Start()
        {
            if (pathfinder != null)
            {
                string graphText = GenerateGraphText(gridIndex, startPoint, endPoint);
                DisplayGraphText(graphText);
            }
            else
            {
                Debug.LogError("SAP2DPathfinder singleton not found.");
            }
        }

        public string GenerateGraphText(int gridIndex, Vector3 startPoint, Vector3 endPoint)
        {
            SAP_GridSource grid = pathfinder.GetGrid(gridIndex);
            if (grid == null)
            {
                return "Grid not found.";
            }

            StringBuilder sb = new StringBuilder();

            // Get all tiles
            List<SAP_TileData> allTiles = GetAllTiles(grid);
            if (allTiles == null || allTiles.Count == 0)
            {
                return "No tiles found.";
            }

            // Find and add dead-end vertices
            List<SAP_TileData> deadEndVertices = FindDeadEndVertices(allTiles, grid, startPoint, endPoint);
            sb.AppendLine("Vertices:");
            foreach (var vertex in deadEndVertices)
            {
                sb.AppendLine($"{vertex.WorldPosition.x}, {vertex.WorldPosition.y}");
            }

            // Add edges
            sb.AppendLine("\nEdges:");
            foreach (var vertex in deadEndVertices)
            {
                List<SAP_TileData> neighbors = grid.GetNeighborTiles(vertex, false);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.isWalkable && deadEndVertices.Contains(neighbor))
                    {
                        float cost = Vector2.Distance(vertex.WorldPosition, neighbor.WorldPosition);
                        sb.AppendLine($"{vertex.WorldPosition} -> {neighbor.WorldPosition} (cost: {cost})");
                    }
                }
            }

            // Add edges from start to dead ends and from dead ends to end
            AddSpecialEdges(sb, startPoint, endPoint, deadEndVertices, grid);

            return sb.ToString();
        }

        private List<SAP_TileData> GetAllTiles(SAP_GridSource grid)
        {
            // Implement as needed based on your API
            List<SAP_TileData> tiles = new List<SAP_TileData>();
            foreach (SAP_TileData td in grid.tiles)
            {
                tiles.Add(td);
            }
            return tiles;
        }

        private List<SAP_TileData> FindDeadEndVertices(List<SAP_TileData> allTiles, SAP_GridSource grid, Vector3 startPoint, Vector3 endPoint)
        {
            List<SAP_TileData> deadEndVertices = new List<SAP_TileData>();

            foreach (var tile in allTiles)
            {
                if (tile.isWalkable)
                {
                    List<SAP_TileData> neighbors = grid.GetNeighborTiles(tile, false);
                    int walkableNeighborCount = 0;

                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.isWalkable)
                        {
                            walkableNeighborCount++;
                        }
                    }

                    if (walkableNeighborCount == 1 || (tile.WorldPosition == startPoint || tile.WorldPosition == endPoint))
                    {
                        deadEndVertices.Add(tile);
                    }
                }
            }

            return deadEndVertices;
        }

        private void AddSpecialEdges(StringBuilder sb, Vector3 startPoint, Vector3 endPoint, List<SAP_TileData> deadEndVertices, SAP_GridSource grid)
        {
            SAP_TileData startTile = grid.GetTileDataAtWorldPosition(startPoint);
            SAP_TileData endTile = grid.GetTileDataAtWorldPosition(endPoint);

            if (startTile != null && startTile.isWalkable)
            {
                foreach (var vertex in deadEndVertices)
                {
                    float cost = Vector2.Distance(startTile.WorldPosition, vertex.WorldPosition);
                    sb.AppendLine($"{startTile.WorldPosition} -> {vertex.WorldPosition} (cost: {cost})");
                }
            }

            if (endTile != null && endTile.isWalkable)
            {
                foreach (var vertex in deadEndVertices)
                {
                    float cost = Vector2.Distance(endTile.WorldPosition, vertex.WorldPosition);
                    sb.AppendLine($"{endTile.WorldPosition} -> {vertex.WorldPosition} (cost: {cost})");
                }
            }
        }

        private void DisplayGraphText(string text)
        {
            if (uiText != null)
            {
                uiText.text = text;
                Debug.Log("Graph text displayed.");
            }
            else
            {
                Debug.LogError("UI Text component not assigned.");
            }
        }
    }
}
