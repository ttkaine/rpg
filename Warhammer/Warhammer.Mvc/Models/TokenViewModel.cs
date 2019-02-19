using System;
using System.Drawing;

namespace Warhammer.Mvc.Models
{

    public enum TokenColor
    {
        Green,
        Red,
        Blue,
        Black,
        White,
        Pink,
        Orange,
        Yellow,
        Purple,
        Grey
    }

    public class TokenViewModel
    {
        public Color DrawingColor
        {
            get
            {
                switch (Colour)
                {
                    case TokenColor.Green:
                        return Color.Green;
                    case TokenColor.Red:
                        return Color.Red;
                    case TokenColor.Blue:
                        return Color.Blue;
                    case TokenColor.Black:
                        return Color.Black;
                    case TokenColor.White:
                        return Color.White;
                    case TokenColor.Pink:
                        return Color.Pink;
                    case TokenColor.Orange:
                        return Color.Orange;
                    case TokenColor.Yellow:
                        return Color.Yellow;
                    case TokenColor.Purple:
                        return Color.Purple;
                    case TokenColor.Grey:
                        return Color.Gray;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int Id { get; set; }
        public TokenColor Colour { get; set; }
    }
}