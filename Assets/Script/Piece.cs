
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int x;
    public int y;
    //Board scriptine referans
    private Board board;

    void Start()
    {
        //Sahnedeki board script'ini bulup refarans�m�za ayarl�yoruz
        board = FindObjectOfType<Board>();
    }
    //Bu fonksiyon �st�nde Collider olan bir nesneye fare ile t�kland���nda otomatik olarak �al���r
    private void OnMouseDown()
    {
        //t�kland���nda Board'a haber veriyoruz
        board.PieceClicked(this);
    }
}
