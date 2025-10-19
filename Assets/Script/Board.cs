using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //oyun alan�n�n geni�li�i
    public int width;

    //oyun alan�n�n y�ksekli�i
    public int height;

    public GameObject[] piecePrefabs;

    //oyundaki t�m ta�lar�n referanslar�n� saklayac��m�z 2 boyutlu matris
    private GameObject[,] allPieces;

    private Piece firstSelectedPiece;
    private Piece secondSelectedPiece;

    //oyun ba�lad���nda �al��an ana fonksiyon
    void Start()
    {
        allPieces = new GameObject[width, height];
        SetupBoard();
    }
    //oyunn alan�n� ta�larla dolduran fonksiyon
    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);

                //Prefab dizimizden rastgale bir ta� �ekli se�iyoruz
                int random = Random.Range(0, piecePrefabs.Length);

                //Se�ti�imiz rastgale prefab� kullanarak sahnede yeni bir ta� olu�turuyoruz
                GameObject piece = Instantiate(piecePrefabs[random], position, Quaternion.identity);
                // olu�turulan ta�� daha d�zenli bir hiyarer�i i�in Board nesnesinin alt�na al�yoruz
                piece.transform.parent = this.transform;
                piece.name = "Piece (" + x + ", " + y + ")";

                //Olu�turdu�umuz bu ta��n Piece script'ine ula��yoruz
                Piece pieceScript = piece.GetComponent<Piece>();
                //ona kendi kordinatlar�n�n ne oldu�unu s�yl�oruz
                pieceScript.x = x;
                pieceScript.y = y;

                //olu�turdu�umuz bu ta�� konumuna g�re matrisimize kaydediyoruz art�k ona x,y diyerek ula�abiliriz
                allPieces[x, y] = piece;
            }
        }
    }
    //Piece.cs script'i bir ta�a t�kland���nda bu fonksiyonu �a��r�r
    public void PieceClicked(Piece piece)
    {
        //E�er daha �nce bir ta� se�mediysek
        if (firstSelectedPiece == null)
        {
            firstSelectedPiece = piece;
        }
        else
        {
            secondSelectedPiece = piece;
            //iki ta��n yerini de�i�tirme i�lemi ba�lat
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
        //Swap i�lemi
        yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));

        //E�le�me kontrol�
        List<GameObject> firsPieceMatches = FindMatchesAt(firstSelectedPiece.x, firstSelectedPiece.y);
        List<GameObject> secondPieceMatches = FindMatchesAt(secondSelectedPiece.x, secondSelectedPiece.y);

        //E�er hi�bir e�le�me yoksa..
        if (firsPieceMatches.Count < 3 && secondPieceMatches.Count < 3)
        {
            //ta�lar� geri al
            yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));

        }
        else
        {
            //e�le�en ta�lar� yok et
            DestroyMatches(firsPieceMatches);
            DestroyMatches(secondPieceMatches);
        }
        firstSelectedPiece = null;
        secondSelectedPiece = null;
    }

    //Sadece animasyon ve yer de�i�tirme i�lemi
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
        piece1.transform.position = piece2Position;
        piece2.transform.position = piece1Position;
    }

    //Belirli bir kordinattaki ta� i�in yatay ve dikey e�le�meleri bulan fonksiyon
    private List<GameObject> FindMatchesAt(int x, int y)
    {
        List<GameObject> combinendMatches = new List<GameObject>();
        GameObject centerPiece = allPieces[x, y];
        if (centerPiece == null) return combinendMatches;
        //yatay e�le�me kontrol�
        List<GameObject> horizontalMatches = new List<GameObject> { centerPiece };
        //sola do�ru kontrol et
        for (int i = x - 1; i <= 0; i--)
        {
            if (allPieces[i, y] != null && allPieces[i, y].tag == centerPiece.tag)
            {
                horizontalMatches.Add(allPieces[i, y]);
            }
            else
                break;
        }
        //sa�a do�ru kontrol et
        for (int i = x + 1; i < width; i++)
        {
            if (allPieces[i, y] != null && allPieces[i, y].tag == centerPiece.tag)
            {
                horizontalMatches.Add(allPieces[i, y]);
            }
            else
                break;

        }
        if (horizontalMatches.Count >= 3)
            combinendMatches.AddRange(horizontalMatches);

        //dikey e�le�me kontrol�
        List<GameObject> verticalMatches = new List<GameObject> { centerPiece };
        //a�a�� kontrol et
        for (int i = y - 1; i >= 0; i--)
        {
            if (allPieces[x, i] != null && allPieces[x, i].tag == centerPiece.tag)
                verticalMatches.Add(allPieces[x, i]);
            else
                break;

        }

        //yukar� kontrol et
        for (int i = y + 1; i < height; i++)
        {
            if (allPieces[x, i] != null && allPieces[x, i].tag == centerPiece.tag)
                verticalMatches.Add(allPieces[x, i]);
            else
                break;
        }
        if (verticalMatches.Count >= 3)
            combinendMatches.AddRange(verticalMatches);

        return combinendMatches;
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
}



    