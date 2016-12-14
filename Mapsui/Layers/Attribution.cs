namespace Mapsui.Layers
{
    public class Attribution
    {
        public Attribution(string text = null, string url = null)
        {
            Text = text ?? "";
            Url = url ?? "";
        }

        public string Text { get; set; }
        public string Url { get; set; }
    }
}