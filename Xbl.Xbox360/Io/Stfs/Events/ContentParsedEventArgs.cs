namespace Xbl.Xbox360.Io.Stfs.Events
{
    public class ContentParsedEventArgs : EventArgs
    {
        public object Content { get; private set; }

        public ContentParsedEventArgs(object content)
        {
            Content = content;
        }
    }
}