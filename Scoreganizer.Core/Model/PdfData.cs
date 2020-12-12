using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    public class PdfToken
    {
        public PdfToken(SongData song, DataModel model)
        {
            Song = song;
            Model = model;
        }
        public SongData Song { get; set; }
        public DataModel Model { get; set; }

    }

    public class PdfReturn
    {

    }

    public class PdfData
    {
    }
}
