using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer.Source.Util
{
    //
    // Summary:
    //     1st Vector2 is the tile position index in mod TileSize,
    //     2nd Vector2 is top left corner of global position in multiples of TileSize ---- is fucked
    internal class PositionCalculator
    {

        public const int TileSize = 16;

        public static (Vector2, Vector2) GetPosition(Vector2 position)
        {

            float x = Mathf.Floor(position.X / TileSize);

            float y = Mathf.Floor(position.Y / TileSize);

            Vector2 tilePosition = new Vector2(x, y);
            Vector2 globalPosition = new Vector2(x * TileSize, y * TileSize);
            return (tilePosition, globalPosition);
        }



    }
}
