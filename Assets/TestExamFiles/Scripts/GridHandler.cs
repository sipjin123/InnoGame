using System.Collections.Generic;
using UnityEngine;

public class GridHandler : MonoBehaviour
{
    [SerializeField]
    private float _GridSize = 12;

    private List<Vector2> _OccupiedGrid = new List<Vector2>();

    [SerializeField]
    private float _GridSpacing = 10;

    [SerializeField]
    private List<GameObject> _ErrorIndicators;

    private Vector2 _SnapGridPosition;
    

    public bool SnapToGrid(Transform bldng, Vector2 size)
    {
        var snappedVector = SnapVector2(new Vector2(bldng.position.x, bldng.position.z), size);

        bldng.position = new Vector3(snappedVector.x, bldng.position.y, snappedVector.y);
        _SnapGridPosition = new Vector2(snappedVector.x, snappedVector.y);

        bool hasFailed = false;
        int indicatorCount = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                var xCoord = snappedVector.x + (x * _GridSpacing);
                var yCoord = snappedVector.y + (y * _GridSpacing);

                if (_OccupiedGrid.Contains(new Vector2(xCoord, yCoord)))
                {
                    hasFailed = true;
                    _ErrorIndicators[indicatorCount].transform.position = new Vector3(xCoord, 1, yCoord);
                    _ErrorIndicators[indicatorCount].SetActive(true);
                }
                else
                {
                    _ErrorIndicators[indicatorCount].SetActive(false);
                }
                indicatorCount++;
            }
        }
        if (hasFailed)
            return false;
        return true;
    }

    public void ClearGrid()
    {
        ClearIndicators();
    }

    public void RemoveGridData(Transform bldng, Vector2 size)
    {
        var snappedVector = SnapVector2(new Vector2(bldng.position.x, bldng.position.z), size);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                var xCoord = snappedVector.x + (x * _GridSpacing);
                var yCoord = snappedVector.y + (y * _GridSpacing);
                _OccupiedGrid.Remove(new Vector2(xCoord, yCoord));
            }
        }
    }

    public void RegisterToGrid(Transform bldng, Vector2 size)
    {
        ClearIndicators();
        var snappedVector = SnapVector2(new Vector2(bldng.position.x, bldng.position.z), size);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                var xCoord = snappedVector.x + (x * _GridSpacing);
                var yCoord = snappedVector.y + (y * _GridSpacing);
                _OccupiedGrid.Add(new Vector2(xCoord, yCoord));
            }
        }
    }

    private void ClearIndicators()
    {
        int indicatorCount = _ErrorIndicators.Count;
        for (int i = 0; i < indicatorCount; i++)
        {
            _ErrorIndicators[i].SetActive(false);
        }
    }

    private Vector2 SnapVector2(Vector2 vec2, Vector2 size)
    {
        var xCoord = Mathf.Round(vec2.x / _GridSpacing) * _GridSpacing;
        var yCoord = Mathf.Round(vec2.y / _GridSpacing) * _GridSpacing;
        var xOffset = (_GridSize * _GridSpacing) - (_GridSpacing * size.x);
        var yOffset = (_GridSize * _GridSpacing) - (_GridSpacing * size.y);
        xCoord = Mathf.Clamp(xCoord, 0, xOffset);
        yCoord = Mathf.Clamp(yCoord, 0, yOffset);

        return new Vector2(xCoord, yCoord);
    }
}