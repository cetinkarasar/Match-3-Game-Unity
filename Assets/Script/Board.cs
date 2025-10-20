using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Duplikasyonlarý silmek için eklendi

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject[] piecePrefabs;
    private GameObject[,] allPieces;

    private Piece firstSelectedPiece;
    private Piece secondSelectedPiece;

    void Start()
    {
        allPieces = new GameObject[width, height];
        SetupBoard();
    }

    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                int randomPieceIndex = GetRandomPieceIndexWithoutMatches(x, y);
                GameObject pieceObject = Instantiate(piecePrefabs[randomPieceIndex], position, Quaternion.identity);

                pieceObject.transform.parent = this.transform;
                pieceObject.name = "Piece (" + x + ", " + y + ")";

                Piece pieceScript = pieceObject.GetComponent<Piece>();
                pieceScript.x = x;
                pieceScript.y = y;

                allPieces[x, y] = pieceObject;
            }
        }
    }
    private int GetRandomPieceIndexWithoutMatches(int x, int y)
    {
        int randomPieceIndex = Random.Range(0, piecePrefabs.Length);
        // Eðer soldaki iki komþu veya alttaki iki komþuyla eþleþiyorsa
        while ((x > 1 && allPieces[x - 1, y].tag == piecePrefabs[randomPieceIndex].tag && allPieces[x - 2, y].tag == piecePrefabs[randomPieceIndex].tag) ||
               (y > 1 && allPieces[x, y -1].tag == piecePrefabs[randomPieceIndex].tag && allPieces[x, y - 2].tag == piecePrefabs[randomPieceIndex].tag))
        {
            randomPieceIndex = Random.Range(0, piecePrefabs.Length);
        }
        return randomPieceIndex;
    }

    public void PieceClicked(Piece piece)
    {
        if (firstSelectedPiece == null)
        {
            firstSelectedPiece = piece;
        }
        else
        {
            secondSelectedPiece = piece;
            StartCoroutine(SwapAndCheckMatches());
        }
    }

    private IEnumerator SwapAndCheckMatches()
    {
        if (Vector2.Distance(firstSelectedPiece.transform.position, secondSelectedPiece.transform.position) > 1.1f)
        {
            firstSelectedPiece = null;
            secondSelectedPiece = null;
            yield break;
        }

        yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));

        List<GameObject> firstPieceMatches = FindMatchesAt(firstSelectedPiece.x, firstSelectedPiece.y);
        List<GameObject> secondPieceMatches = FindMatchesAt(secondSelectedPiece.x, secondSelectedPiece.y);

        if (firstPieceMatches.Count < 3 && secondPieceMatches.Count < 3)
        {
            yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));
        }
        else
        {
            List<GameObject> allMatches = firstPieceMatches.Union(secondPieceMatches).ToList();
            //eþleþenleri yok et
            DestroyMatches(allMatches);

            //Tablayý doldurmak için yeni ana fonksiyonumuzu çaðýralým
            yield return StartCoroutine(RefillBoard());

            //Zincirleme eþleþmeleri kontrol et
            while (FindAllMatchesOnBoard().Count > 0)
            {
                DestroyMatches(FindAllMatchesOnBoard());
                yield return StartCoroutine(RefillBoard());
            }
        }

        firstSelectedPiece = null;
        secondSelectedPiece = null;
    }

    private IEnumerator SwapPiecesAnimation(Piece piece1, Piece piece2)
    {
        Vector2 piece1Position = piece1.transform.position;
        Vector2 piece2Position = piece2.transform.position;

        allPieces[piece1.x, piece1.y] = piece2.gameObject;
        allPieces[piece2.x, piece2.y] = piece1.gameObject;

        int tempX = piece1.x;
        int tempY = piece1.y;
        piece1.x = piece2.x;
        piece1.y = piece2.y;
        piece2.x = tempX;
        piece2.y = tempY;

        float duration = 0.3f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            piece1.transform.position = Vector2.Lerp(piece1Position, piece2Position, elapsedTime / duration);
            piece2.transform.position = Vector2.Lerp(piece2Position, piece1Position, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Animasyon sonunda pozisyonlarýn tam olduðundan emin ol
        piece1.transform.position = piece2Position;
        piece2.transform.position = piece1Position; // <-- DÜZELTÝLMÝÞ SATIR
    }

    private List<GameObject> FindMatchesAt(int x, int y)
    {
        List<GameObject> combinedMatches = new List<GameObject>();
        GameObject centerPiece = allPieces[x, y];
        if (centerPiece == null) return combinedMatches;

        List<GameObject> horizontalMatches = new List<GameObject> { centerPiece };
        for (int i = x - 1; i >= 0; i--) { if (allPieces[i, y] != null && allPieces[i, y].tag == centerPiece.tag) horizontalMatches.Add(allPieces[i, y]); else break; }
        for (int i = x + 1; i < width; i++) { if (allPieces[i, y] != null && allPieces[i, y].tag == centerPiece.tag) horizontalMatches.Add(allPieces[i, y]); else break; }
        if (horizontalMatches.Count >= 3) combinedMatches.AddRange(horizontalMatches);

        List<GameObject> verticalMatches = new List<GameObject> { centerPiece };
        for (int i = y - 1; i >= 0; i--) { if (allPieces[x, i] != null && allPieces[x, i].tag == centerPiece.tag) verticalMatches.Add(allPieces[x, i]); else break; }
        for (int i = y + 1; i < height; i++) { if (allPieces[x, i] != null && allPieces[x, i].tag == centerPiece.tag) verticalMatches.Add(allPieces[x, i]); else break; }
        if (verticalMatches.Count >= 3) combinedMatches.AddRange(verticalMatches);

        return combinedMatches;
    }

    void DestroyMatches(List<GameObject> matches)
    {
        foreach (GameObject piece in matches)
        {
            if (piece != null)
            {
                allPieces[piece.GetComponent<Piece>().x, piece.GetComponent<Piece>().y] = null;
                Destroy(piece);
            }
        }
    }
    //Tablayý yeniden dolduran ana Coroutine 
    private IEnumerator RefillBoard()
    {
        yield return StartCoroutine(CollapseColumns());
        yield return StartCoroutine(FillTopRows());
    }

    //sütunlardaki boþluklarý kapatmak için yerçekimi
    private IEnumerator CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allPieces[x, y] == null)
                {
                    // yukarda dolu kare ara
                    for (int k = y + 1; k < height; k++)
                    { 
                        if (allPieces[x, k] != null)
                        {
                            //parçaya yerçekimi uygula
                            allPieces[x, y] = allPieces[x, k];
                            allPieces[x, k] = null;
                            allPieces[x, y].GetComponent<Piece>().y = y;
                            StartCoroutine(MovePieceToPosition(allPieces[x, y], new Vector2(x, y)));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.4f);
    }
    //Üst satýrlara yeni parçalar ekle
    private IEnumerator FillTopRows()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allPieces[x, y] == null)
                {
                    Vector2 position = new Vector2(x, height);
                    int randomPieceIndex = Random.Range(0, piecePrefabs.Length);
                    GameObject pieceObject = Instantiate(piecePrefabs[randomPieceIndex], position, Quaternion.identity);

                    //Gerekli ayaralrý yap ve mantýksal tablaya ekle
                    pieceObject.transform.parent = this.transform;
                    pieceObject.name = "Piece (" + x + ", " + y + ")";
                    Piece pieceScript = pieceObject.GetComponent<Piece>();
                    pieceScript.x = x;
                    pieceScript.y = y;
                    allPieces[x, y] = pieceObject;

                    //ve son olarak animasyonlu olmasý gereken yere indir
                    StartCoroutine(MovePieceToPosition(pieceObject, new Vector2(x, y)));
                }
            }
        }
        yield return new WaitForSeconds(0.4f);
    }
    
    //taþý belirli posisyona animasyonla götür
    private IEnumerator MovePieceToPosition(GameObject piece, Vector2 targetPosition)
    {
        float duration = 0.3f;
        float elapsedTime = 0;
        Vector2 startPosition = piece.transform.position;
        while (elapsedTime < duration)
        {
            if (piece == null) yield break;

            piece.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (piece != null)
            piece.transform.position = targetPosition;
    }

    //Tablodaki tüm eþleþmeleri bul(zincirleme için)
    private List<GameObject> FindAllMatchesOnBoard()
    {
        List<GameObject> allMatches = new List<GameObject>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allPieces[x, y] != null)
                {
                    allMatches.AddRange(FindMatchesAt(x, y));
                }
            }

        }
        //Duplikasyonlarý sil 
        return allMatches.Distinct().ToList();
    }

}