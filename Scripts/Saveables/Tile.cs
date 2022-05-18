using System;

[Serializable] // to write to save
public class Tile
{
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorA;
    public float tilePosX;
    public float tilePosY;
    public bool tileShadeable;

    // constructor for the tile file
    public Tile()
    {
        colorR = 1f;
        colorG = 1f;
        colorB = 1f;
        colorA = 1f;
        tilePosX = 0f;
        tilePosY = 0f;
        tileShadeable = true;
    }
}
