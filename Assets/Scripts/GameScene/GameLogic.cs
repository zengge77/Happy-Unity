using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    //游戏流程已经搭建好，但是重要的消除算法，补全算法未确定，将这俩算法完成后游戏便可正常游玩
    //2019.9.15 基础的消除算法，补全算法已完成，游戏可以正常游玩，但因为消除算法的缺陷，现在仍然有bug：消除后新补全的糖果有些不会再次检测消除
    //2019.9.16 改用了另外的消除算法（获取要消除的糖果），原有bug消除。

    //大功能:消除与补位算法的改进，消除可以解决扎堆糖果的计算，补位可以再糖果下落时不只直下
    //大功能:引入新的cell<gameobject/type>数组，统计全图的实时格子内容，如果使用gameobject，所有格子类型都继承至cell类
    //大功能：新的类或结构，用来储存一组糖果
    //小功能：对象池
    //大功能: 奖励糖果的引入
    //小功能: 计分榜

    public Text log;

    public GameObject cell;
    public Transform cellArea;

    public Candy candy;
    public Transform CandyArea;

    public Transform maskcell;
    public Transform maskCellArea;
    List<Transform> maskCellList;

    enum CellType { NormalCandy, StripedCandy, WrappedCandy, ColorBombs, JellyFish, CoconutWheel, Empty }
    CellType[,] map;
    int height, width;

    [Range(3, 6)]
    public int maxType;
    public static Vector2 startPostion;
    public static float dropTimeEachCell = 0.07f;

    Candy[,] candyArray;
    Candy firstCandy;
    Candy secondCandy;

    enum GameState { SelectFirstCandy, SelectSecondCandy, DecideCanExchange, StartCalculation, Calculating };
    GameState gameState = GameState.SelectFirstCandy;


    void Start()
    {
        //读取csv文件，获取代表格子类型的字符串二维数组
        string[,] cellTypeArray = CsvParse.Parse(Date.CSV);

        height = cellTypeArray.GetLength(0);
        width = cellTypeArray.GetLength(1);
        map = new CellType[height, width];

        //赋值到 map
        for (int i = 0; i < height; i++)
        {

            for (int j = 0; j < width; j++)
            {
                switch (cellTypeArray[height - 1 - i, j])
                {
                    case "普通":
                        map[i, j] = CellType.NormalCandy;
                        break;
                    case "空白":
                        map[i, j] = CellType.Empty;
                        break;

                    default:
                        map[i, j] = CellType.NormalCandy;
                        Debug.Log("未找到对应类型");
                        break;
                }
            }
        }

        candyArray = new Candy[height, width];
        startPostion = new Vector2(0.5f - (float)width / 2, 0.5f - (float)height / 2);
        CreatrCellArea();
        CreatrCandyArea();
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.SelectFirstCandy:
                if (Input.GetMouseButtonDown(0) && SelectCandy(out firstCandy))
                {
                    firstCandy.GetComponent<Collider2D>().enabled = false;
                    firstCandy.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.7f);
                    gameState = GameState.SelectSecondCandy;
                }
                break;

            case GameState.SelectSecondCandy:
                if (Input.GetMouseButton(0) && SelectCandy(out secondCandy))
                {
                    firstCandy.InitializeSelf();
                    gameState = GameState.DecideCanExchange;
                }
                else if (!Input.GetMouseButton(0))
                {
                    firstCandy.InitializeSelf();
                    firstCandy = null;
                    gameState = GameState.SelectFirstCandy;
                }
                break;

            case GameState.DecideCanExchange:
                if (Neighboring(firstCandy, secondCandy))
                {
                    gameState = GameState.StartCalculation;
                }
                else
                {
                    firstCandy = null;
                    secondCandy = null;
                    gameState = GameState.SelectFirstCandy;
                }
                break;

            case GameState.StartCalculation:
                gameState = GameState.Calculating;
                StartCoroutine(Calculation());
                break;

            case GameState.Calculating:
                break;
        }
    }

    public void UpdateLog(System.Object value)
    {
        int lineNum = log.text.Split('\n').Length;
        if (lineNum >= 15)
        {
            log.text = string.Empty;
        }
        log.text += "\n";
        log.text += value.ToString();
    }

    void CreatrCellArea()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] != CellType.Empty)
                {
                    var newCell = Instantiate(cell);
                    newCell.transform.SetParent(cellArea);
                    newCell.transform.position = new Vector2(startPostion.x + j, startPostion.y + i);
                }
            }
        }

        maskCellList = new List<Transform>();
        for (int i = 0; i < width; i++)
        {
            for (int j = height - 1; j >= 0; j--)
            {
                if (map[j, i] != CellType.Empty)
                {
                    Transform newMask = Instantiate(maskcell);
                    maskCellList.Add(newMask);
                    newMask.SetParent(maskCellArea);
                    newMask.position = new Vector2(startPostion.x + i, startPostion.y + j + 1);
                    break;
                }
            }
        }
    }
    public void CreatrCandyArea()
    {
        ResetCandyArray();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (map[i, j] == CellType.NormalCandy)
                {
                    Candy newCandy = Instantiate(candy, CandyArea);
                    newCandy.height = i;
                    newCandy.width = j;
                    newCandy.transform.position = newCandy.OriginalPosition;
                    candyArray[i, j] = newCandy;
                }
            }
        }

        InitializeCandyArrayType();
    }
    void ResetCandyArray()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (candyArray[i, j] != null)
                {
                    Destroy(candyArray[i, j].gameObject);
                    candyArray[i, j] = null;
                }
            }
        }
    }

    //初始化全部糖果的颜色，并使得初始不会出现三连的糖果
    public void InitializeCandyArrayType()
    {
        firstCandy = null;
        secondCandy = null;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //设置颜色：
                //糖果的生成方向是 “右往上”，因此只需让每个糖果生成时的颜色与左和下不产生3连即可，最后全部糖果必定不会出现3连
                //检测糖果的左和下方向的各两个糖果颜色，如果该方向有两个糖果并且颜色相同，就记录下该颜色
                //最后设置颜色时避免生成与之前记录颜色相同的颜色
                if (candyArray[i, j] != null)
                {
                    //记录颜色在candy.candyType的下标，使用下标来记录使得运算更易，默认-1，即不存在
                    int candyTypeIndex_Left = -1;
                    int candyTypeIndex_Down = -1;

                    //左方向的颜色
                    if (j > 1)
                    {
                        Candy candyLeft1 = candyArray[i, j - 1];
                        Candy candyLeft2 = candyArray[i, j - 2];
                        if ((candyLeft1 != null) && (candyLeft2 != null) && candyLeft1.candyType == candyLeft2.candyType)
                        {
                            candyTypeIndex_Left = (int)candyLeft1.candyType;
                        }
                    }
                    //下方向的颜色
                    if (i > 1)
                    {
                        Candy candyDown1 = candyArray[i - 1, j];
                        Candy candyDown2 = candyArray[i - 2, j];
                        if ((candyDown1 != null) && (candyDown2 != null) && candyDown1.candyType == candyDown2.candyType)
                        {
                            candyTypeIndex_Down = (int)candyDown1.candyType;
                        }
                    }

                    //循环直至生成与记录不同的颜色
                    int candyTypeIndex;
                    while (true)
                    {
                        candyTypeIndex = UnityEngine.Random.Range(0, maxType);
                        if (candyTypeIndex != candyTypeIndex_Left && candyTypeIndex != candyTypeIndex_Down)
                        {
                            break;
                        }
                    }
                    candyArray[i, j].InitializeSelf();
                    candyArray[i, j].ChangeColor((Candy.CandyType)candyTypeIndex);
                }
            }
        }
    }

    bool SelectCandy(out Candy candy)
    {
        Vector3 start = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(start, Vector2.zero);

        if (raycastHit2D.transform != null && raycastHit2D.transform.tag == "Candy")
        {
            candy = raycastHit2D.transform.GetComponent<Candy>();
            return true;
        }

        candy = null;
        return false;
    }
    bool Neighboring(Candy candy1, Candy candy2)
    {
        if (candy2.width == candy1.width)
        {
            if (candy2.height == candy1.height + 1 || (candy2.height == candy1.height - 1))
            {
                return true;
            }
        }
        if (candy2.height == candy1.height)
        {
            if (candy2.width == candy1.width + 1 || candy2.width == candy1.width - 1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取要消除的糖果列表
    /// </summary>
    /// <param name="changedCandyList"></param>
    /// <returns></returns>
    List<Candy> GetEliminateCandys(List<Candy> changedCandyList)
    {
        List<Candy> eliminateCandys = new List<Candy>();

        List<Candy> hasBeenAdd = new List<Candy>();

        foreach (Candy candy in changedCandyList)
        {
            List<Candy> sameCandys = new List<Candy>();
            FillSameCandyList(candy, sameCandys, hasBeenAdd);
            NewAddEliminateCandys(candy, sameCandys, eliminateCandys);
        }

        return eliminateCandys;
    }

    /// <summary>
    /// 获取周围的同色糖果
    /// </summary>
    /// <param name="currentCandy"></param>
    /// <returns></returns>
    List<Candy> GetAroundCandys(Candy currentCandy)
    {
        int x = currentCandy.height;
        int y = currentCandy.width;
        List<Candy> aroundCandys = new List<Candy>();

        if (x < height - 1 && candyArray[x + 1, y] != null)
        {
            aroundCandys.Add(candyArray[x + 1, y]);
        }
        if (x > 0 && candyArray[x - 1, y] != null)
        {
            aroundCandys.Add(candyArray[x - 1, y]);
        }
        if (y > 0 && candyArray[x, y - 1] != null)
        {
            aroundCandys.Add(candyArray[x, y - 1]);
        }
        if (y < width - 1 && candyArray[x, y + 1] != null)
        {
            aroundCandys.Add(candyArray[x, y + 1]);
        }
        return aroundCandys;
    }

    /// <summary>
    /// 将于当前糖果相连的同色糖果加入到sameCandys中
    /// </summary>
    /// <param name="currentCandy"></param>
    /// <param name="sameCandys"></param>
    /// <param name="hasBeenAdd"></param>
    void FillSameCandyList(Candy currentCandy, List<Candy> sameCandys, List<Candy> hasBeenAdd)
    {
        if (sameCandys.Contains(currentCandy) || hasBeenAdd.Contains(currentCandy))
        {
            return;
        }

        sameCandys.Add(currentCandy);
        hasBeenAdd.Add(currentCandy);

        //上下左右的Item
        List<Candy> tempCandyList = new List<Candy>();
        tempCandyList.AddRange(GetAroundCandys(currentCandy));

        for (int i = 0; i < tempCandyList.Count; i++)
        {
            if (currentCandy.candyType == tempCandyList[i].candyType)
            {
                FillSameCandyList(tempCandyList[i], sameCandys, hasBeenAdd);
            }
        }
    }

    /// <summary>
    /// 将sameCandys中三连以上的糖果加入到eliminateCandys中
    /// </summary>
    /// <param name="currentcandy"></param>
    /// <param name="sameCandyList"></param>
    /// <param name="eliminateCandys"></param>
    void NewAddEliminateCandys(Candy currentcandy, List<Candy> sameCandyList, List<Candy> eliminateCandys)
    {
        //现在还没做新的candylist类，所以先用List<List<Candy>二维容器，且在整个方法最后还得将容器还原到List<Candy>一维
        List<List<Candy>> container = new List<List<Candy>>();

        //委托，用于将竖向的糖果加入到容器中，如果竖向的糖果里有的已经在在容器中，则将该竖糖果与横向融合在一起；
        Action<List<List<Candy>>, List<Candy>> addToContainer = (MyContainer, MyCandyList) =>
        {
            for (int i = 0; i < MyCandyList.Count; i++)
            {
                for (int j = 0; j < MyContainer.Count; j++)
                {
                    for (int k = 0; k < MyContainer[j].Count; k++)
                    {
                        if (MyCandyList[i] == MyContainer[j][k])
                        {
                            for (int l = 0; l < MyCandyList.Count; l++)
                            {
                                if (!MyContainer[j].Contains(MyCandyList[l]))
                                {
                                    MyContainer[j].Add(MyCandyList[l]);
                                }
                            }
                            return;
                        }
                    }
                }
            }
            MyContainer.Add(MyCandyList);
        };

        //添加到临时数组中，方便计算
        Candy[,] tempCandyArray = new Candy[height, width];
        for (int i = 0; i < sameCandyList.Count; i++)
        {
            tempCandyArray[sameCandyList[i].height, sameCandyList[i].width] = sameCandyList[i];
        }

        //横向
        for (int i = 0; i < height; i++)
        {
            List<Candy> tempCandyList = new List<Candy>();
            for (int j = 0; j < width; j++)
            {
                if (tempCandyArray[i, j] != null)
                {
                    if (tempCandyList.Count == 0)
                    {
                        tempCandyList.Add(tempCandyArray[i, j]);
                    }
                    else if (tempCandyArray[i, j].width == tempCandyList[tempCandyList.Count - 1].width + 1)
                    {
                        tempCandyList.Add(tempCandyArray[i, j]);
                    }
                    else
                    {
                        if (tempCandyList.Count >= 3) { container.Add(tempCandyList); }
                        else
                        {
                            tempCandyList.Clear();
                            tempCandyList.Add(tempCandyArray[i, j]);
                        }
                    }
                }
            }
            if (tempCandyList.Count >= 3) { container.Add(tempCandyList); }
        }

        //竖向
        for (int i = 0; i < width; i++)
        {
            List<Candy> tempCandyList = new List<Candy>();
            for (int j = 0; j < height; j++)
            {
                if (tempCandyArray[j, i] != null)
                {
                    if (tempCandyList.Count == 0)
                    {
                        tempCandyList.Add(tempCandyArray[j, i]);
                    }
                    else if (tempCandyArray[j, i].height == tempCandyList[tempCandyList.Count - 1].height + 1)
                    {
                        tempCandyList.Add(tempCandyArray[j, i]);
                    }
                    else
                    {
                        if (tempCandyList.Count >= 3) { addToContainer(container, tempCandyList); }
                        else
                        {
                            tempCandyList.Clear();
                            tempCandyList.Add(tempCandyArray[j, i]);
                        }
                    }
                }
            }
            if (tempCandyList.Count >= 3) { addToContainer(container, tempCandyList); }
        }

        //还原
        for (int i = 0; i < container.Count; i++)
        {
            eliminateCandys.AddRange(container[i]);
        }

    }

    /// <summary>
    /// 选中两个糖果后开始计算
    /// </summary>
    /// <returns></returns>
    IEnumerator Calculation()
    {
        yield return SwapCandys(firstCandy, secondCandy);

        List<Candy> EliminateCandyList = GetEliminateCandys(new List<Candy> { firstCandy, secondCandy });

        if (EliminateCandyList.Count == 0)
        {
            yield return SwapCandys(secondCandy, firstCandy);
        }
        else
        {
            while (EliminateCandyList.Count != 0)
            {
                //消除
                yield return EliminateCandys(EliminateCandyList);

                //补位
                List<Candy> newChangedCandyList = new List<Candy>();
                yield return CompletionCandyArray(newChangedCandyList);

                EliminateCandyList = GetEliminateCandys(newChangedCandyList);
            }
        }

        firstCandy = null;
        secondCandy = null;
        gameState = GameState.SelectFirstCandy;
    }

    /// <summary>
    /// 交换两个糖果
    /// </summary>
    /// <param name="candy1"></param>
    /// <param name="candy2"></param>
    /// <returns></returns>
    IEnumerator SwapCandys(Candy candy1, Candy candy2)
    {
        candy1.GetComponent<SpriteRenderer>().sortingOrder = 2;
        candy2.GetComponent<SpriteRenderer>().sortingOrder = 1;

        float time = 0f;
        while (time <= 0.5f)
        {
            candy1.transform.position = Vector2.Lerp(candy1.OriginalPosition, candy2.OriginalPosition, 3 * (time += Time.deltaTime));
            candy2.transform.position = Vector2.Lerp(candy2.OriginalPosition, candy1.OriginalPosition, 3 * (time += Time.deltaTime));
            yield return 0;
        }

        int tempWidth = candy1.width;
        int tempHeight = candy1.height;
        candy1.width = candy2.width;
        candy1.height = candy2.height;
        candy2.width = tempWidth;
        candy2.height = tempHeight;

        candyArray[candy1.height, candy1.width] = candy1;
        candyArray[candy2.height, candy2.width] = candy2;

        candy1.GetComponent<SpriteRenderer>().sortingOrder = 0;
        candy2.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    /// <summary>
    /// 消除糖果
    /// </summary>
    /// <param name="EliminateCandyList"></param>
    /// <returns></returns>
    IEnumerator EliminateCandys(List<Candy> EliminateCandyList)
    {
        float destroyTime = 0.4f;
        foreach (Candy candy in EliminateCandyList)
        {
            candyArray[candy.height, candy.width] = null;
            StartCoroutine(candy.CandyDestroy(destroyTime));
        }
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 生成新的糖果
    /// </summary>
    /// <param name="newChangedCandyList"></param>
    /// <returns></returns>
    IEnumerator CompletionCandyArray(List<Candy> newChangedCandyList)
    {
        //已有的糖果开始下落
        for (int i = 0; i < width; i++)
        {
            int count = 0;
            Queue dropQueue = new Queue();
            for (int j = 0; j < height; j++)
            {
                if (candyArray[j, i] != null)
                {
                    count++;
                    dropQueue.Enqueue(candyArray[j, i]);
                }
            }

            for (int k = 0; k < count; k++)
            {
                Candy candy = dropQueue.Dequeue() as Candy;
                if (candy.height != k)
                {
                    candyArray[candy.height, candy.width] = null;
                    candy.height = k;
                    candyArray[candy.height, candy.width] = candy;
                    newChangedCandyList.Add(candy);
                    StartCoroutine(candy.CandyDrop(0));
                }
            }
        }

        //生成新的糖果并下落补上空缺
        int maxCount = 0;
        for (int i = 0; i < width; i++)
        {
            int count = 0;
            for (int j = 0; j < height; j++)
            {
                if (map[j, i] != CellType.Empty && candyArray[j, i] == null)
                {
                    Candy newCandy = Instantiate(candy, CandyArea);
                    newCandy.width = i;
                    newCandy.height = j;
                    newCandy.transform.position = maskCellList[i].position;
                    newCandy.RandomChangeColor(maxType);

                    candyArray[j, i] = newCandy;
                    newChangedCandyList.Add(newCandy);
                    StartCoroutine(newCandy.CandyDrop(count));

                    count++;
                    maxCount = maxCount > count ? maxCount : count;
                }
            }
        }
        yield return new WaitForSeconds(maxCount * dropTimeEachCell + 0.2f);
    }
}
