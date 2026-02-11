namespace BitTorrent
{
    public class FileItem
    {
        public string Path;
        public long Size;
        public long Offset;

        public string FormattedSize { get { return Torrent.BytesToString(Size); }}
    }
}

