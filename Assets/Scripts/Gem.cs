using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Gem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum GemType
    {
        NONE,//empty
        BOMB,//special tile

        DIAMOND,
        GARNET,
        EMERALD,
        RUBY,
        TOPAZ,
        TOURMALINE,
        SAPPHIRE,
        AMETHYST,
    }

    public GemType type;
    public int x;
    public int y;
    public float maxMoveTime = 2f;//time spent for move 1 unit

    private float currentMoveTime;
    public bool isMoving;
    private Vector2 savedPos;
    private GameManager gm;

    private void Awake()
    {
        gm = GameManager.Instance;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateSprite();
    }

    void UpdateSprite()
    {
        Image img = GetComponent<Image>();
        if (type == GemType.NONE)
        {
            img.sprite = null;
            img.enabled = false;
        }
        else
        {
            img.sprite = (Sprite)Resources.Load("Icons/" + type.ToString().ToLower(), typeof(Sprite));
            img.enabled = true;
        }
    }

    public void Move(Vector2 targetPos)
    {
        if (!isMoving)//before move
        {
            gm.canClear = false;
            isMoving = true;
            currentMoveTime = 0;
            savedPos = transform.position;
            StartCoroutine("MoveCoroutine", targetPos);
        }
    }

    IEnumerator MoveCoroutine(Vector2 targetPos)
    {
        while (Vector2.Distance(transform.position, targetPos) >= 0.05f || currentMoveTime < maxMoveTime)
        {
            currentMoveTime += Time.deltaTime;
            transform.position = Vector2.Lerp(savedPos, targetPos, currentMoveTime / maxMoveTime);
            yield return 0;
        }
        transform.position = targetPos;
        isMoving = false;
        gm.canClear = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!gm.canClear) return;
        if(this.type== GemType.BOMB)
        {
            for (int ty = -1; ty <= 1; ty++)
            {
                for (int tx = -1; tx <= 1; tx++)
                {
                    gm.canClear = false;
                    Animation anim = gm.gems[x + tx, y + ty].GetComponent<Animation>();
                    anim.Play();
                }
            }
            gm.aud.clip = gm.clips[1];
            gm.aud.Play();
            gm.gemsCount += 9;
        }
        else gm.swapGems.swapGem1 = this;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gm.swapGems.swapGem1 = null;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gm.swapGems.swapGem1 == null) return;
        float xValue = Input.GetAxis("Mouse X");
        float yValue = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(xValue) >= Mathf.Abs(yValue))//if drag on x axis
        {
            if (xValue < 0 && gm.swapGems.swapGem1.x-1>=0)//move left
            {
                gm.swapGems.swapGem2 = gm.gems[gm.swapGems.swapGem1.x-1,gm.swapGems.swapGem1.y];
            }
            else if (xValue > 0 && gm.swapGems.swapGem1.x + 1 <= gm.maxX)//move right
            {
                gm.swapGems.swapGem2 = gm.gems[gm.swapGems.swapGem1.x + 1, gm.swapGems.swapGem1.y];
            }
        }
        else if (Mathf.Abs(xValue) < Mathf.Abs(yValue))//if drag on y axis
        {
            if (yValue < 0 && gm.swapGems.swapGem1.y - 1 >= 0)//move down
            {
                gm.swapGems.swapGem2 = gm.gems[gm.swapGems.swapGem1.x, gm.swapGems.swapGem1.y-1];
            }
            else if (yValue > 0 && gm.swapGems.swapGem1.y + 1 < gm.maxY)//move up
            {
                gm.swapGems.swapGem2 = gm.gems[gm.swapGems.swapGem1.x, gm.swapGems.swapGem1.y+1];
            }
        }
        //gm.swapGems
        //Debug.Log("X asix move=" + Input.GetAxis("Mouse X").ToString("f5"));
        //Debug.Log("Y asix move=" + Input.GetAxis("Mouse Y").ToString("f5"));

    }

    public void InitGem()
    {
        type = GemType.NONE;
        GameManager.Instance.canClear = true;
    }
}
