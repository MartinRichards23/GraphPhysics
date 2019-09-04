using GraphPhysics.Resources;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using SystemPlus.Windows.Media;

namespace GraphPhysics.Model
{
    public sealed class Node : NodeBase
    {
        Brush brush;
        FormattedText formattedText;
        ImageSource image;

        public Node(Point p, double mass, Color colour)
            : base(p, mass, colour)
        {
            //image = MyResources.Resources.GetImage("Resources\\ .jpg");

            UpdateText();
        }

        protected override void OnColourChanged()
        {
            brush = BrushCache.GetBrush(Colour);
        }

        private void UpdateText()
        {
            formattedText = new FormattedText("Business cat!", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Helvetica"), 12, BrushCache.GetBrush(Colors.White));
        }

        public override void Draw(DrawingContext dc, DrawParameters parameters)
        {
            double radius = this.Radius;

            if (IsSelected)
            {
                // draw highlight
                dc.DrawEllipse(highlightBrush, null, Position, radius + 5, radius + 5);
            }

            dc.DrawEllipse(brush, null, Position, radius, radius);

            if (parameters.ZoomLevel > DetailThreshold)
            {
                // draw text
                Point textPosition = new Point(Position.X - formattedText.Width / 2, Position.Y + radius);
                dc.DrawText(formattedText, textPosition);

                // draw image
                if (image != null)
                {
                    double width = radius * 1.4;
                    double height = radius * 1.4;

                    Rect imageRect = new Rect(Position.X - width / 2, Position.Y - height / 2, width, height);

                    dc.DrawImage(image, imageRect);
                }
            }
        }
    }
}