
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int x;
    public int y;
    //Board scriptine referans
    private Board board;

    void Start()
    {
        //Sahnedeki board script'ini bulup refaransýmýza ayarlýyoruz
        board = FindObjectOfType<Board>();
    }
    //Bu fonksiyon üstünde Collider olan bir nesneye fare ile týklandýðýnda otomatik olarak çalýþýr
    private void OnMouseDown()
    {
        //týklandýðýnda Board'a haber veriyoruz
        board.PieceClicked(this);
    }
}
