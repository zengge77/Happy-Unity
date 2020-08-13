using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
    public enum CandyType { Red, Yellow, Blue, Green, Purple, Orange, Empty }
    public CandyType candyType = CandyType.Empty;

    public List<Sprite> sprite = new List<Sprite>();
    public SpriteRenderer spriteRenderer;
    public Collider2D candyCollider;

    public int height, width;

    private Vector2 originalPosition;
    public Vector2 OriginalPosition
    {
        get { return new Vector2(GameLogic.startPostion.x + width, GameLogic.startPostion.y + height); }
        set { originalPosition = value; }
    }


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        candyCollider = GetComponent<Collider2D>();
    }

    public void ChangeColor(CandyType newState)
    {
        this.candyType = newState;
        spriteRenderer.sprite = sprite[(int)newState];
    }
    public void RandomChangeColor(int maxType)
    {
        ChangeColor((CandyType)Random.Range(0, maxType));
    }
    public void InitializeSelf()
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 0;
        candyCollider.enabled = true;
    }

    public IEnumerator CandyDestroy(float destroyTime)
    {
        float startTime = 0f;
        float flashCD = 0f;
        bool isRandererEnabled = false;
        while (startTime <= destroyTime)
        {
            startTime += Time.deltaTime;
            flashCD += Time.deltaTime;
            if (flashCD > 0)
            {
                flashCD -= 0.1f;
                spriteRenderer.enabled = isRandererEnabled;
                isRandererEnabled = !isRandererEnabled;
            }
            yield return 0;
        }
        Destroy(gameObject);
    }
    public IEnumerator CandyDrop(int cellCount)
    {
        yield return new WaitForSeconds(cellCount * GameLogic.dropTimeEachCell);

        Vector2 oldPosition = transform.position;
        float startTime = 0f;
        float endTime = Mathf.Abs(oldPosition.y - OriginalPosition.y) * GameLogic.dropTimeEachCell;
        while (startTime <= endTime)
        {
            transform.position = Vector2.Lerp(oldPosition, OriginalPosition, (1f / endTime) * (startTime += Time.deltaTime));
            yield return 0;
        }
    }

}
