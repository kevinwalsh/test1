using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGChecked_TurtleChallenge
{

    public class Position
    {
        public int x;
        public int y;

        public Position(int posX, int posY)
        {
            this.x = posX;
            this.y = posY;
        }
    }
    
    public enum Direction { N, E, S, W }

    public enum Tile { Empty, Exit, Mine }

    enum MoveType { Forward, TurnL, TurnR }

    class Turtle
    {
        public Position pos { get; set; }
        public Direction dir { get; set; }

        public Turtle(int x, int y, Direction d)
        {
            this.pos = new Position(x, y);
            this.dir = d;
        }

        public Position Move()
        {
            Position newPos = new Position(pos.x, pos.y);
            switch (dir)
            {
                case Direction.N:
                    newPos.y--;                 // Array [0,0] is topleft; y++ to go south; x++ to go east
                    break;
                case Direction.S:
                    newPos.y++;
                    break;
                case Direction.E:
                    newPos.x++;
                    break;
                case Direction.W:
                    newPos.x--;
                    break;
            }
            return newPos;
        }
    }
    
    public class SettingsModel
    {
        public int gridX;
        public int gridY;
        public int startX;
        public int startY;
        public Direction startDirection;
        public int exitX;
        public int exitY;
        public List<int> mineX;
        public List<int> mineY;
        public List<Position> mines;
    }
    
}
