using System.Windows.Media;

namespace IslandSquare
{
    public class ColorRange
    {
        public Color Color { get; private set; }
        
        public SolidColorBrush ColorBrush
        {
            get { return new SolidColorBrush(Color); }
        }

        public ColorRange(Color color)
        {
            Color = color;
        }
    }
}
