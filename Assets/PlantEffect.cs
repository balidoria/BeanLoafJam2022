using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEffect : MonoBehaviour
{
    [Tooltip("How many seconds between castings.")]
    public float secondsBetweenCasting;

    [Tooltip("The target of my effect.")]
    public EffectTarget target;

    [Tooltip("The magnitude of my effect.")]
    public int effectAdjustment;

    [Tooltip("My range of effect.")]
    public int Range;

    // How long since I last casted.
    private float timeSinceLastCasting = 0;

    internal bool IsCastable = false;

    void Update()
    {
        timeSinceLastCasting += Time.deltaTime;
        if (timeSinceLastCasting >= secondsBetweenCasting)
        {
            IsCastable = true;
        }
    }

    public void TryCast(BasePlant originPlant)
    {
        if (!IsCastable)
            return;

        // TODO: Cast the effect based on the given plant origin and range.
        // ex. foreach (BasePlant in plantsInRange) do the thing.
        var grid = GameManager.instance.GameGrid;
        var cellPosition = grid.WorldToCell(originPlant.transform.position);

        var plantsInRange = new List<BasePlant>();
        for (int i = 0; i < Range; i++)
        {
            // var right   = new Vector3Int(cellPosition.x + 1 * (int)grid.cellSize.x, cellPosition.y, 0);
            // var left    = new Vector3Int(cellPosition.x - 1 * (int)grid.cellSize.x, cellPosition.y, 0);
            // var up      = new Vector3Int(cellPosition.x, cellPosition.y + 1 * (int)grid.cellSize.y, 0);
            // var down    = new Vector3Int(cellPosition.x, cellPosition.y - 1 * (int)grid.cellSize.y, 0);
            // var upleft  = new Vector3Int(cellPosition.x - 1 * (int)grid.cellSize.x, cellPosition.y + 1 * (int)grid.cellSize.y, 0);
            // var upright  = new Vector3Int(cellPosition.x + 1 * (int)grid.cellSize.x, cellPosition.y + 1 * (int)grid.cellSize.y, 0);
            // var downleft = new Vector3Int(cellPosition.x - 1 * (int)grid.cellSize.x, cellPosition.y - 1 * (int)grid.cellSize.y, 0);
            // var downright = new Vector3Int(cellPosition.x + 1 * (int)grid.cellSize.x, cellPosition.y - 1 * (int)grid.cellSize.y, 0);

            List<Vector3Int> plantsAffected = new List<Vector3Int>(8)
            {
                new Vector3Int(cellPosition.x + i * (int)grid.cellSize.x, cellPosition.y, 0),
                new Vector3Int(cellPosition.x - i * (int)grid.cellSize.x, cellPosition.y, 0),
                new Vector3Int(cellPosition.x, cellPosition.y + i * (int)grid.cellSize.y, 0),
                new Vector3Int(cellPosition.x, cellPosition.y - i * (int)grid.cellSize.y, 0),
                new Vector3Int(cellPosition.x - i * (int)grid.cellSize.x, cellPosition.y + i * (int)grid.cellSize.y, 0),
                new Vector3Int(cellPosition.x + i * (int)grid.cellSize.x, cellPosition.y + i * (int)grid.cellSize.y, 0),
                new Vector3Int(cellPosition.x - i * (int)grid.cellSize.x, cellPosition.y - i * (int)grid.cellSize.y, 0),
                new Vector3Int(cellPosition.x + i * (int)grid.cellSize.x, cellPosition.y - i * (int)grid.cellSize.y, 0)
            };

            if (!GameManager.instance.TileEmpty())

        }

        IsCastable = false;
        timeSinceLastCasting = 0;
    }
}

public enum EffectTarget
{
    PLANTGROWTHRATE,
    WEEDRESISTANCE
}