using System;
using System.Collections.Generic;
using System.Text;

namespace Lomont.Scoreganizer.Core.Model
{
    /// <summary>
    /// Media player interface for control
    /// </summary>
    public interface IMediaPlayer
    {
        public void Start();
        public void Stop();
    }
}
