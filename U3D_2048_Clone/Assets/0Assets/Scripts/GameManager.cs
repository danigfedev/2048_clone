using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform gameViewport;
    public GameObject gameTile;

    private GameTile[,] gameMatrix;
    private int rowCount = 4;
    private int columnCount = 4;

    private bool gameMatrixChanged = false;

    #region === Monobehaviour methods ===
    void Start()
    {
        InitializeGameMatrix();
        //Add two items to begin the game
        AddNewItemToMatrix();
        AddNewItemToMatrix();
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        // Game restart
        if (Input.GetKeyUp(KeyCode.R))
        {
            RestartGame();
            return;
        }

        // Game mechanics
        if (Input.GetKeyUp(KeyCode.UpArrow))
            MoveTilesVertically(VerticalDirections.UP);

        else if (Input.GetKeyUp(KeyCode.DownArrow))
            MoveTilesVertically(VerticalDirections.DOWN);

        else if (Input.GetKeyUp(KeyCode.LeftArrow))
            MoveTilesHorizontally(HorizontalDirections.LEFT);

        else if (Input.GetKeyUp(KeyCode.RightArrow))
            MoveTilesHorizontally(HorizontalDirections.RIGHT);


        if (gameMatrixChanged)
        {
            gameMatrixChanged = false;
            if (GetEmptyTileIndexes(gameMatrix).Length <= 0)
            {
                Debug.LogError("You Lost");
                RestartGame();
                return;
            }
            //UpdateUI(); //Doing this seems to cause trouble
            AddNewItemToMatrix();
            UpdateUI();
        }
            
    }

    #endregion

    


    private void UpdateUI()
    {
        ClearUI();

        // Draw current matrix
        for(int rowIdx = 0; rowIdx < rowCount; rowIdx++)
        {
            for(int colIdx = 0; colIdx < columnCount; colIdx++)
            {
                //Debug.Log(gameMatrix[rowIdx, colIdx]);
                if(gameMatrix[rowIdx, colIdx] != null)
                {
                    Transform uiTile = gameViewport.GetChild(4 * rowIdx + colIdx);
                    uiTile.GetChild(0).gameObject.SetActive(false);
                    GameObject tile = Instantiate(gameTile, uiTile);
                    //tile.GetComponent<GameTileHandler>().initialize();
                    tile.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = gameMatrix[rowIdx, colIdx].value.ToString();
                }
                
            }
        }

        Debug.LogWarning("Empty Tiles: " + GetEmptyTileIndexes(gameMatrix).Length);
    }


    private void ClearUI()
    {
        for(int uiIdx=0; uiIdx<rowCount*columnCount; uiIdx++)
        {
            gameViewport.GetChild(uiIdx).GetChild(0).gameObject.SetActive(true);
            if(gameViewport.GetChild(uiIdx).childCount > 1)
                Destroy(gameViewport.GetChild(uiIdx).GetChild(1).gameObject);
        }
    }

    #region === Game Mechanics control ===

    private void MoveTilesHorizontally(HorizontalDirections direction)
    {
        Debug.Log("Moving " + direction.ToString());

        gameMatrixChanged = true;

        for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
        {
            GameTile[] row = GetRow(gameMatrix, rowIdx);
            /*GameTile[]*/List<GameTile> notNullRow = (from _gameTile in row
                                     where _gameTile != null
                                     select _gameTile).ToList();
            //ToArray();

            notNullRow = CheckRowForMatches(notNullRow, HorizontalDirections.LEFT);
            //int direction = -1;
            //int origin = ;
            //int destination = 0;
            //for (int colIdx = columnCount-1; colIdx >= 0; colIdx--)
            for (int colIdx = 0; colIdx < columnCount; colIdx++)
            {
                if (colIdx < notNullRow.Count)
                {
                    if(direction == HorizontalDirections.LEFT)
                        row[colIdx] = notNullRow[colIdx];
                    else
                        row[columnCount - 1 - colIdx] = notNullRow[notNullRow.Count - 1 - colIdx];
                    continue;
                }
                if (direction == HorizontalDirections.LEFT)
                    row[colIdx] = null;
                else
                    row[columnCount - 1 - colIdx] = null;
            }

            SetRow(gameMatrix, rowIdx, row);
        }
    }

    private void MoveTilesVertically(VerticalDirections direction)
    {
        gameMatrixChanged = true;

        Debug.Log("Moving " + direction.ToString());

        for (int colIdx = 0; colIdx < columnCount; colIdx++)
        {
            GameTile[] column = GetColumn(gameMatrix, colIdx);
            List<GameTile> notNullColumn = (from _gameTile in column
                                        where _gameTile != null
                                        select _gameTile).ToList();


            notNullColumn = CheckColumnForMatches(notNullColumn, direction);


            //Debug.LogWarning("not null elements: " + notNullColumn.Length);
            for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
            {
                if (rowIdx < notNullColumn.Count)
                {
                    if (direction == VerticalDirections.DOWN)
                        column[rowCount - 1 - rowIdx] = notNullColumn[notNullColumn.Count - 1 - rowIdx];
                    else
                        column[rowIdx] = notNullColumn[rowIdx];

                    continue;
                }
                if (direction == VerticalDirections.DOWN)
                    column[rowCount - 1 - rowIdx] = null;
                else
                    column[rowIdx] = null;
            }

            SetColumn(gameMatrix, colIdx, column);
        }
    }


    private List<GameTile> CheckRowForMatches(List<GameTile> row, HorizontalDirections direction)
    {

        for (int colIdx = 0; colIdx < row.Count - 1; colIdx++)
        {
            if(direction == HorizontalDirections.LEFT)
            {
                if (row[colIdx].value == row[colIdx + 1].value)
                {
                    row[colIdx].value = row[colIdx].value + row[colIdx + 1].value;
                    row[colIdx + 1] = null;
                    colIdx++; //skip comparison with null item
                }
            }
            else//RIGHT
            {
                if (row[row.Count -1 - colIdx].value == row[row.Count - 1 - colIdx - 1].value)
                {
                    row[row.Count - 1 - colIdx].value = row[row.Count - 1 - colIdx].value + row[row.Count - 1 - colIdx - 1].value;
                    row[row.Count - 1 - colIdx - 1] = null;
                    colIdx++;//skip comparison with null item
                }
            }
        }

        //Get non null items
        return (from _gameTile in row
               where _gameTile != null
               select _gameTile).ToList();
    }


    private List<GameTile> CheckColumnForMatches(List<GameTile> column, VerticalDirections direction)
    {

        for (int rowIdx = 0; rowIdx < column.Count - 1; rowIdx++)
        {
            if (direction == VerticalDirections.UP)
            {
                if (column[rowIdx].value == column[rowIdx + 1].value)
                {
                    column[rowIdx].value += column[rowIdx + 1].value;
                    column[rowIdx + 1] = null;
                    rowIdx++; //skip comparison with null item
                }
            }
            else//RIGHT
            {
                if (column[column.Count - 1 - rowIdx].value == column[column.Count - 1 - rowIdx - 1].value)
                {
                    column[column.Count - 1 - rowIdx].value += column[column.Count - 1 - rowIdx - 1].value;//column[column.Count - 1 - rowIdx].value + column[column.Count - 1 - rowIdx - 1].value;
                    column[column.Count - 1 - rowIdx - 1] = null;
                    rowIdx++;//skip comparison with null item
                }
            }
        }

        //Get non null items
        return (from _gameTile in column
                where _gameTile != null
                select _gameTile).ToList();
    }

    #endregion

    #region === Game Workflow ===

    /// <summary>
    /// Clear current game state and starts a new one
    /// </summary>
    private void RestartGame()
    {
        //ClearUI();
        InitializeGameMatrix();
        AddNewItemToMatrix();
        AddNewItemToMatrix();
        UpdateUI();

    }
    private void NextGameTurn()
    {
        //1 - Move Tiles
        //2 - Check for tile matches
        //3 - Update UI
    }

    #endregion

    #region === Game Matrix Utilities ===

    /// <summary>
    /// Initializes game matrix
    /// </summary>
    private void InitializeGameMatrix()
    {
        gameMatrix = new GameTile[rowCount, columnCount];
    }

    /// <summary>
    /// Adds a new game tile to game matrix in a random position among free ones
    /// </summary>
    private void AddNewItemToMatrix()
    {
        Debug.LogWarning("Adding New item to matrix");
        Vector2[] emptyTilesIndexes = GetEmptyTileIndexes(gameMatrix);
        Debug.Log("Total tiles: " + (16 - emptyTilesIndexes.Length +1));
        //2-Pick random index
        int idx = UnityEngine.Random.Range(0, emptyTilesIndexes.Length);
        Vector2 newTileIndex = emptyTilesIndexes[idx];
        
        //3-Add new tile in index
        gameMatrix[(int)newTileIndex.x, (int)newTileIndex.y] = new GameTile(newTileIndex);

    }

    /// <summary>
    /// Retrieve given column's values from matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="rowIdx"></param>
    /// <returns></returns>
    private GameTile[] GetRow(GameTile[,] matrix, int rowIdx)
    {
        return Enumerable.Range(0, matrix.GetLength(1)).
            Select(x => matrix[rowIdx, x]).
            ToArray();
    }

    /// <summary>
    /// Set given row's values in matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="rowIdx"></param>
    /// <param name="row"></param>
    private void SetRow(GameTile[,] matrix, int rowIdx, GameTile[] row)
    {
        for(int cIdx =0; cIdx < row.Length; cIdx++)
        {
            matrix[rowIdx, cIdx] = row[cIdx];
        }
    }

    /// <summary>
    /// Retrieve given column's values from matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="columnIdx"></param>
    /// <returns></returns>
    private GameTile[] GetColumn(GameTile[,] matrix, int columnIdx)
    {
        return Enumerable.Range(0, matrix.GetLength(0)).
            Select(x => matrix[x, columnIdx]).
            ToArray();

    }

    /// <summary>
    /// Set given column's values in matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="colIdx"></param>
    /// <param name="col"></param>
    private void SetColumn(GameTile[,] matrix, int colIdx, GameTile[] col)
    {
        for (int rIdx = 0; rIdx < col.Length; rIdx++)
        {
            matrix[rIdx, colIdx] = col[rIdx];
        }
    }

    /// <summary>
    /// Returns all empty indexes in game Matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private Vector2[] GetEmptyTileIndexes(GameTile[,] matrix)
    {
        List<Vector2> nulIndexArray = new List<Vector2>();

        for(int row = 0; row< matrix.GetLength(0); row++)
            for (int col = 0; col < matrix.GetLength(1); col++)
                if (matrix[row, col] == null) nulIndexArray.Add(new Vector2(row, col));

        return nulIndexArray.ToArray();
    }

    #endregion

}
