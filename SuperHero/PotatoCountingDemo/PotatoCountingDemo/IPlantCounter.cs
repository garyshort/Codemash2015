using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoCountingDemo
{
    public interface IPlantCounter
    {
        /// <summary>
        /// Counts the plants in a provided image
        /// </summary>
        /// <returns>The number of plants detected</returns>
        int CountPlants();
        /// <summary>
        /// Classifies each pixel of an image as being foreground
        /// or background
        /// </summary>
        /// <returns> An image whose pixels are either black or white</returns>
        Bitmap Classify();
        /// <summary>
        /// Returns the number of clusters detected
        /// </summary>
        /// <returns>Int number of clusters</returns>
        int GetClusterCount();
    }
}
