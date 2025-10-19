using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    //oyun alanýnýn geniþliði
    public int width;

    //oyun alanýnýn yüksekliði
    public int height;

    public GameObject[] piecePrefabs;

    //oyundaki tüm taþlarýn referanslarýný saklayacðýmýz 2 boyutlu matris
    private GameObject[,] allPieces;

    private Piece firstSelectedPiece;
    private Piece secondSelectedPiece;

    //oyun baþladýðýnda çalýþan ana fonksiyon
    void Start()
    {
        allPieces = new GameObject[width, height];
        SetupBoard();
    }
    //oyunn alanýný taþlarla dolduran fonksiyon
    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);

                //Prefab dizimizden rastgale bir taþ þekli seçiyoruz
                int random = Random.Range(0, piecePrefabs.Length);

                //Seçtiðimiz rastgale prefabý kullanarak sahnede yeni bir taþ oluþturuyoruz
                GameObject piece = Instantiate(piecePrefabs[random], position, Quaternion.identity);
                // oluþturulan taþý daha düzenli bir hiyarerþi için Board nesnesinin altýna alýyoruz
                piece.transform.parent = this.transform;
                piece.name = "Piece (" + x + ", " + y + ")";

                //Oluþturduðumuz bu taþýn Piece script'ine ulaþýyoruz
                Piece pieceScript = piece.GetComponent<Piece>();
                //ona kendi kordinatlarýnýn ne olduðunu söylüoruz
                pieceScript.x = x;
                pieceScript.y = y;

                //oluþturduðumuz bu taþý konumuna göre matrisimize kaydediyoruz artýk ona x,y diyerek ulaþabiliriz
                allPieces[x, y] = piece;
            }
        }
    }
    //Piece.cs script'i bir taþa týklandýðýnda bu fonksiyonu çaðýrýr
    public void PieceClicked(Piece piece)
    {
        //Eðer daha önce bir taþ seçmediysek
        if (firstSelectedPiece == null)
        {
            firstSelectedPiece = piece;
        }
        else
        {
            secondSelectedPiece = piece;
            //iki taþýn yerini deðiþtirme iþlemi baþlat
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
        //Swap iþlemi
        yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));

        //Eþleþme kontrolü
        List<GameObject> firsPieceMatches = FindMatchesAt(firstSelectedPiece.x, firstSelectedPiece.y);
        List<GameObject> secondPieceMatches = FindMatchesAt(secondSelectedPiece.x, secondSelectedPiece.y);

        //Eðer hiçbir eþleþme yoksa..
        if (firsPieceMatches.Count < 3 && secondPieceMatches.Count < 3)
        {
            //taþlarý geri al
            yield return StartCoroutine(SwapPiecesAnimation(firstSelectedPiece, secondSelectedPiece));

        }
        else
        {
            //eþleþen taþlarý yok et
            DestroyMatches(firsPieceMatches);
            DestroyMatches(secondPieceMatches);
        }
        firstSelectedPiece = null;
        secondSelectedPiece = null;
    }

    //Sadece animasyon ve yer deðiþtirme iþlemi
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

    //Belirli bir kordinattaki taþ için yatay ve dikey eþleþmeleri bulan fonksiyon
    private List<GameObject> FindMatchesAt(int x, int y)
    {
        List<GameObject> combinendMatches = new List<GameObject>();
        GameObject centerPiece = allPieces[x, y];
        if (centerPiece == null) return combinendMatches;
        //yatay eþleþme kontrolü
        List<GameObject> horizontalMatches = new List<GameObject> { centerPiece };
        //sola doðru kontrol et
        for (int i = x - 1; i <= 0; i--)
        {
            if (allPieces[i, y] != null && allPieces[i, y].tag == centerPiece.tag)
            {
                horizontalMatches.Add(allPieces[i, y]);
            }
            else
                break;
        }
        //saða doðru kontrol et
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

        //dikey eþleþme kontrolü
        List<GameObject> verticalMatches = new List<GameObject> { centerPiece };
        //aþaðý kontrol et
        for (int i = y - 1; i >= 0; i--)
        {
            if (allPieces[x, i] != null && allPieces[x, i].tag == centerPiece.tag)
                verticalMatches.Add(allPieces[x, i]);
            else
                break;

        }

        //yukarý kontrol et
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



    