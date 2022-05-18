using System.Collections.Generic;
using System;

[Serializable] // to write to save
public class Project
{
    public string SaveName;
    public List<Tile> Tiles;

    // constructor for the project file
    public Project()
    {
        SaveName = "";
        Tiles = new List<Tile>();
    }
}
