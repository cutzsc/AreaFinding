using System.Windows.Media;

namespace IslandSquare
{
    public static class ExtensionMethods
    {
        public static bool EqualsToColor(this Color col, Color color)
        {
            return col.R == color.R &&
                col.G == color.G &&
                col.B == color.B;
        }

        public static bool InRange(this Color col, Color min, Color max)
        {
            return col.R >= min.R && col.R <= max.R &&
                col.G >= min.G && col.G <= max.G &&
                col.B >= min.B && col.B <= max.B;
        }
    }
}
