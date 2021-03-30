using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    public int maxX;
    public int maxY;//maxY is the gem spawn level

    [SerializeField]
    public Gem[,] gems;
    public int maxSpawnIndex;

    public int movesLeft;
    public int gemsCount;
    public float timeLeft;

    public GameObject gemPrefab;
    public Transform gemContainer;
    public bool canClear;

    private Vector2 originPos = new Vector2(-66f, -486f);
    public bool isGameStart;
    

    [System.Serializable]
    public class UIManager
    {
        public GameObject levelSelectScreen;
        public GameObject gameScreen;
        public GameObject resultScreen;

        public Text movesLeft;
        public Text gemsCount;
        public Text timeLeft;
        public Slider progressBar;
        public Text[] deleteLines;
        public Image[] stars;
    }
    public UIManager uiManager;

    public struct SwapGems
    {
        public Gem swapGem1;
        public Gem swapGem2;
    }

    public SwapGems swapGems;

    public AudioClip[] clips;
    public AudioSource aud;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameStart) return;
        if (!GameLoop()) return;
        AutoFill();
        FallCheck();
        PlayerSwapCheck();
    }

    void AutoFill()
    {
        for (int x = 0; x <= maxX; x++)
        {
            if(gems[x,maxY].type== Gem.GemType.NONE&&!gems[x,maxY].isMoving)
            {
                gems[x, maxY].type = (Gem.GemType)Random.Range(2,maxSpawnIndex);
            }
        }
    }

    void FallCheck()
    {
        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x <= maxX; x++)
            {
                if(gems[x,y].type == Gem.GemType.NONE&&gems[x,y+1].type!= Gem.GemType.NONE)
                {
                    if (gems[x, y].isMoving || gems[x, y + 1].isMoving) return;
                    Swap(gems[x, y], gems[x, y + 1]);
                }
            }
        }
        if (canClear) ClearCheck();
    }

    void ClearCheck()
    {
        List<Gem> clearList = new List<Gem>();
        List<Gem> checkList = new List<Gem>();
        for (int y = 0; y < maxY; y++)          ////////////////
        {                                       //Foreach gems//
            for (int x = 0; x <= maxX; x++)    ////////////////
            {
                checkList.Add(gems[x, y]);
                //start horizontal check
                for (int xOffset = -1; x+xOffset >= 0; xOffset--)//check left
                {
                    if (gems[x + xOffset, y].type == gems[x, y].type)
                    {
                        checkList.Add(gems[x + xOffset, y]);
                    }
                    else break;
                }
                for (int xOffset = 1; x+xOffset <= maxX; xOffset++)//check right
                {
                    if (gems[x + xOffset, y].type == gems[x, y].type)
                    {
                        checkList.Add(gems[x + xOffset, y]);
                    }
                    else break;
                }
                if (checkList.Count < 3) checkList.Clear();//clear list if <3 
                else //if >=3 then add them to clearlist
                {
                    foreach (Gem gem in checkList)
                    {
                        if (!clearList.Contains(gem))
                        {
                            clearList.Add(gem);
                        }
                    }
                    checkList.Clear();
                } 
                //start vertical check
                checkList.Add(gems[x, y]);
                for (int yOffset = -1; y + yOffset >= 0; yOffset--)//check down
                {
                    if (gems[x, y + yOffset].type == gems[x, y].type)
                    {
                        checkList.Add(gems[x, y + yOffset]);
                    }
                    else break;
                }
                for (int yOffset = 1; y + yOffset < maxY; yOffset++)//check up
                {
                    if (gems[x, y + yOffset].type == gems[x, y].type)
                    {
                        checkList.Add(gems[x, y + yOffset]);
                    }
                    else break;
                }
                if (checkList.Count < 3) checkList.Clear();//clear list if <3 
                else //if >=3 then add them to clearlist
                {
                    foreach (Gem gem in checkList)
                    {
                        if (!clearList.Contains(gem))
                        {
                            clearList.Add(gem);
                        }
                    }
                    checkList.Clear();
                }
            }
        }
        foreach (Gem gem in clearList)
        {
            canClear = false;
            Animation anim = gem.GetComponent<Animation>();
            anim.Play();
        }
        if (clearList.Count >0)
        {
            aud.clip = clips[0];
            aud.Play();
            if (clearList.Count >= 5)
            {
                clearList[0].type = Gem.GemType.BOMB;
                clearList[0].GetComponent<Animation>().Stop();
            }
        }

        gemsCount += clearList.Count;
        clearList.Clear();
    }

    public void Swap(Gem g1, Gem g2)
    {
        int tempX = g1.x;
        int tempY = g1.y;
        Vector2 tempPos = g1.transform.position;
        g1.Move(g2.transform.position);
        g2.Move(tempPos);
        gems[g1.x, g1.y] = g2;
        gems[g2.x, g2.y] = g1;
        g1.x = g2.x;
        g1.y = g2.y;
        g2.x = tempX;
        g2.y = tempY;
    }

    void PlayerSwapCheck()
    {
        if (!canClear)
        {
            InitSwapGems();
            return;
        }
        if (swapGems.swapGem1 && swapGems.swapGem2)
        {
            Swap(swapGems.swapGem1, swapGems.swapGem2);
            movesLeft -= 1;
            InitSwapGems();
        }
    }

    void InitSwapGems()
    {
        swapGems.swapGem1 = null;
        swapGems.swapGem2 = null;
    }

    bool GameLoop()
    {
        UpdateUI();
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            uiManager.resultScreen.SetActive(true);
            return false;
        }
        else if (movesLeft==0)
        {
            uiManager.resultScreen.SetActive(true);
            return false;
        }
        else if (uiManager.progressBar.value>=1f)
        {
            uiManager.resultScreen.SetActive(true);
            return false;
        }
        timeLeft -= Time.deltaTime;
        return true;
    }

    void UpdateUI()
    {
        uiManager.movesLeft.text = "x"+movesLeft;
        uiManager.gemsCount.text = "x" + gemsCount;
        uiManager.timeLeft.text = (int)timeLeft+"s";
        uiManager.progressBar.value = (float)gemsCount / 50;
        if (uiManager.progressBar.value >= 0.3f)
        {
            uiManager.deleteLines[0].gameObject.SetActive(true);
            uiManager.stars[0].color = new Color(1, 1, 1, 1);
        }
        if (uiManager.progressBar.value >= 0.6f)
        {
            uiManager.deleteLines[1].gameObject.SetActive(true);
            uiManager.stars[1].color = new Color(1, 1, 1, 1);
        }
        if (uiManager.progressBar.value >= 1f)
        {
            uiManager.deleteLines[2].gameObject.SetActive(true);
            uiManager.stars[2].color = new Color(1, 1, 1, 1);
        }
    }

    void Init()
    {
        gems = new Gem[maxX+1, maxY+1];
        timeLeft = 60;
        movesLeft = 20;
        gemsCount = 0;
        canClear = false;
        isGameStart = true;
        for (int y = 0; y <= maxY; y++)
        {
            for (int x = 0; x <= maxX; x++)
            {
                GameObject go = Instantiate(gemPrefab,gemContainer);
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(108,108);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(originPos.x+x*108, originPos.y+y * 108);
                gems[x, y] = go.GetComponent<Gem>();
                gems[x, y].x = x;
                gems[x, y].y = y;
            }
        }
    }

    public void PlayBtn()
    {
        uiManager.levelSelectScreen.SetActive(true);
    }

    public void LevelSelect(int _maxSpawnIndex)
    {
        maxSpawnIndex = _maxSpawnIndex;
        uiManager.gameScreen.SetActive(true);
        uiManager.levelSelectScreen.SetActive(false);
        Init();
    }

    public void Back()
    {
        isGameStart = false;
        uiManager.gameScreen.SetActive(false);
        uiManager.levelSelectScreen.SetActive(false);
        uiManager.resultScreen.SetActive(false);
    }
}
