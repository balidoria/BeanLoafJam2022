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

    public async void TryCast(BasePlant originPlant)
    {
        if (!IsCastable)
            return;

        // TODO: Cast the effect based on the given plant origin and range.
        // ex. foreach (BasePlant in plantsInRange) do the thing.
        var grid = GameManager.instance.GameGrid;
        var cellPosition = grid.WorldToCell(originPlant.transform.position);

        var plantsInRange = new List<BasePlant>();
        for (int i = 1; i <= Range; i++)
        {
            // List<Vector3> plantsVecsInRange = new List<Vector3>(8)
            // {
            //     new Vector3(cellPosition.x + i * grid.cellSize.x, cellPosition.y, 0),
            //     new Vector3(cellPosition.x - i * grid.cellSize.x, cellPosition.y, 0),
            //     new Vector3(cellPosition.x, cellPosition.y + i * grid.cellSize.y + 1, 0),
            //     new Vector3(cellPosition.x, cellPosition.y - i * grid.cellSize.y + 1, 0),
            //     new Vector3(cellPosition.x - i * grid.cellSize.x + 1, cellPosition.y + i * grid.cellSize.y + 1, 0),
            //     new Vector3(cellPosition.x + i * grid.cellSize.x - 1, cellPosition.y + i * grid.cellSize.y + 1, 0),
            //     new Vector3(cellPosition.x - i * grid.cellSize.x + 1, cellPosition.y - i * grid.cellSize.y + 1, 0),
            //     new Vector3(cellPosition.x + i * grid.cellSize.x - 1, cellPosition.y - i * grid.cellSize.y + 1, 0)
            // };

            var plantsAffected = new HashSet<BasePlant>();

            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.up * 3, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.down * 3, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.left * 3, Color.red, 1.0f);
            Debug.DrawLine(originPlant.transform.position, originPlant.transform.position + Vector3.right * 3, Color.red, 1.0f);

            
            var hit = Physics2D.OverlapCircleAll(Vector2Int.FloorToInt(new Vector2(originPlant.transform.position.x, originPlant.transform.position.y))
            , 3);

            // TODO: Look at this when life makes sense again after breakfast. -Elisha


            // int j = 0;
            // foreach (var plant in plants)
            // {
            //     j++;
            //     Debug.Log(i);
            // }
            // if (!GameManager.instance.TileEmpty())

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