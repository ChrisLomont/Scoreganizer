using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    public class PdfToken
    {
        public PdfToken(SongData song)
        {
            Song = song;
        }
        public SongData Song { get; set; }

    }

    public class PdfReturn
    {

    }

    public class PdfData
    {
    }
}
