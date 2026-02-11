using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace BitTorrent
{
    public class Torrent
    {
        public string Name { get; private set; }
        public bool? IsPrivate { get; private set; }
        public List<FileItem> Files { get; private set; } = new List<FileItem>();
        public string FileDirectory { get { return (Files.Count > 1 ? Name + Path.DirectorySeparatorChar : ""); } }
        public string DownloadDirectory { get; private set; }

        public List<Tracker> Trackers { get; } = new List<Tracker>();
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public Encoding Encoding { get; set; }

        public int BlockSize { get; private set; }
        public int PieceSize { get; private set; }
        public long TotalSize { get { return Files.Sum(x => x.Size);  } }

        public string FormattedPieceSize { get { return BytesToString(PieceSize);  } }
        public string FormattedTotalSize { get { return BytesToString(TotalSize);  } }

        public int PieceCount { get; { return PieceHashes.Length;  } }

        public byte[][] PieceHashes { get; private set; }
        public bool[] IsPieceVerified { get; private set; }
        public bool[][] IsBlockAcquired { get; private set; }

        

        public byte[] InfoHash { get; private set; } = new byte[20];
        public string HexStringInfoHash { get { return String.Join("", this.InfoHash.Select(x => x.ToString("x2"))); } }
        public string UrlSafeStringInfoHash { get { return Encoding.UTF8.GetString(WedUtility.UrlEncodeToBytes(this.InfoHash, 0, 20)); } }
        

    }
}