using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    /// <summary>
    /// Higher value means higher priority.
    /// </summary>
    [Flags]
    internal enum Priority
    {
        /// <summary>
        /// Once per chapter.
        /// </summary>
        Pages = 1,

        /// <summary>
        /// For html pages and images.
        /// </summary>
        Image = 1, 

        Series = 2, 
        Chapters = 2
        
    }
}
