using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PotatoCountingDemo
{
    /// <summary>
    /// A class to count plants in a given image
    /// </summary>
    public class PlantCounter : IDisposable, IPlantCounter
    {
        private Bitmap bitmap;
        private Byte greenessThreshold;
        private int distanceThreshold;
        private Dictionary<int, List<Point>> clusters =
            new Dictionary<int, List<Point>>();

        /// <summary>
        /// Construct a PlantCounter
        /// </summary>
        /// <param name="bitmap">The image to count plants in</param>
        /// <param name="greeness">The greenness threshold from classification</param>
        /// <param name="distance">The distance a point can be and still be in a cluster</param>
        public PlantCounter(Bitmap bitmap, Byte greeness, int distance)
        {
            this.bitmap = bitmap;
            this.greenessThreshold = greeness;
            this.distanceThreshold = distance;
        }

        /// <summary>
        /// Construct a PlantCounter, calculate the greeness threshold
        /// </summary>
        /// <param name="bitmap">The image to count plants</param>
        /// <param name="distance">The distance a point can be and still be in a cluster</param>
        public PlantCounter(Bitmap bitmap, int distance)
        {
            this.bitmap = bitmap;
            this.greenessThreshold = this.ThresholdFromAverageHistogramIntensity();
            this.distanceThreshold = distance;
        }

        /// <summary>
        /// Calculatae the 'greeness' threshold from most common intensity
        /// </summary>
        /// <returns>byte representation of the intensity threshold</returns>
        private byte ThresholdFromAverageHistogramIntensity()
        {
            // Create an intensity histogram for "greenness"
            int[] histogram = this.CreateIntensityHistogram();

            // Skip the first value as that's zero intensity
            int[] allButZeroIntensity = histogram.Skip(1).ToArray();
            
            // Find the average frequency
            double average = allButZeroIntensity.Average();

            // Find the closest frequency to the average
            int closest = allButZeroIntensity.Aggregate((x, y) => 
                Math.Abs(x - average) < Math.Abs(y - average) ? x : y);

            // Find the first intensity with the average frequency
            int intensity = 
                Array.FindIndex(allButZeroIntensity, x => x == closest);

            // And return it
            return (byte)intensity;
        }

        /// <summary>
        /// Calculate the 'greenness' threshold using Otsu's Method
        /// http://en.wikipedia.org/wiki/Otsu%27s_method
        /// </summary>
        /// <returns>byte representation of the intensity threshold</returns>
        private Byte ThresholdFromOtsuMethod()
        {
            int totalPixels = this.bitmap.Width * this.bitmap.Height;
            int[] intensities = this.CreateIntensityHistogram();
            double[] intensityProbabilities =
                this.CreateProbabilitiesHistogram(intensities);

            double foregroundProb = 0d;
            double backgroundProb = 0d;
            double foregroundClassMean = 0d;
            double backgroundClassMean = 0d;
            double delta = 0d;
            double max = 0d;
            double t1 = 0d;
            double t2 = 0d;

            for (int i = 1; i < intensities.Length; i++)
            {
                // Update the probablities
                foregroundProb = intensityProbabilities
                    .Take(i)
                    .ToList()
                    .Sum();

                backgroundProb = intensityProbabilities
                    .Skip(i)
                    .ToList()
                    .Sum();

                // Update the class means
                foregroundClassMean =
                    foregroundProb * (intensities[i] / 2) / foregroundProb;

                backgroundClassMean =
                    backgroundProb * (intensities[i] / 2) / backgroundProb;

                // Caluclate the delta
                delta =
                    foregroundProb *
                    backgroundProb *
                    Math.Pow(foregroundClassMean - backgroundClassMean, 2);

                // Set thresholds
                if (delta >= max)
                {
                    t1 = i;
                    if (delta > max)
                    {
                        t2 = i;
                    }
                    max = delta;
                }
            }

            // Return the required threshold
            return (byte)((t1 + t2) / 2d);
        }

        /// <summary>
        /// Create a histogram of the "greenness" in the image
        /// </summary>
        /// <returns>An array of ints being the "greenness" historgram</returns>
        private int[] CreateIntensityHistogram()
        {
            int[] histogram = new int[256];
            for (int x = 0; x < this.bitmap.Width; x++)
            {
                for (int y = 0; y < this.bitmap.Height; y++)
                {
                    histogram[this.bitmap.GetPixel(x, y).G]++;      
                }
            };

            return histogram;
        }

        /// <summary>
        /// Create a histogram of the probability of each intensity
        /// </summary>
        /// <param name="intensities">An int[] histogram of intensities</param>
        /// <returns>A double[] histogram of probabilites</returns>
        private double[] CreateProbabilitiesHistogram(int[] intensities)
        {
            double[] probabilities = new double[256];
            int totalPixels = this.bitmap.Width * this.bitmap.Height;
            for (int i = 0; i < intensities.Length; i++)
            {
                probabilities[i] = (double)intensities[i] / (double)totalPixels;
            }
            return probabilities;
        }

        /// <summary>
        /// Counts the plants in a provided image
        /// </summary>
        /// <returns>The number of plants detected</returns>
        public int CountPlants()
        {
            this.ClusterClassifiedBitmap(this.Classify());
            return this.clusters.Count;
        }

        /// <summary>
        /// Classifies each pixel of an image as being foreground
        /// or background
        /// </summary>
        /// <returns> An image whose pixels are either black or white</returns>
        public Bitmap Classify()
        {
            // Grab our bitmap and call the copy constructor
            Bitmap bm = new Bitmap(this.bitmap);

            // Classify the clone depending on "greeness" threshold
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    Color c = bm.GetPixel(x, y);
                    if (c.G > this.greenessThreshold)
                    {
                        bm.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        bm.SetPixel(x, y, Color.Black);
                    }
                }
            };

            return bm;
        }

        /// <summary>
        /// Detects clusters in an image which as been classified
        /// </summary>
        /// <param name="bm">Bitmap</param>
        private void ClusterClassifiedBitmap(Bitmap bm)
        {
            // Reset cluster from last run
            this.clusters.Clear();

            // Visit each pixel in the image
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    // We are only interested in the foreground pixels
                    if (bm.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        this.ClusterPoint(new Point(x, y));
                    }
                }
            };
        }

        /// <summary>
        /// Add the given point to a cluster, based on it's nearest 
        /// neighbour, or create a new cluster for it if there is no
        /// nearest neighbour
        /// </summary>
        /// <param name="point"></param>
        private void ClusterPoint(Point point)
        {
            // If there are no clusters, then this point 
            // is the seed of the first
            if (this.clusters.Count == 0)
            {
                this.clusters.Add(1, new List<Point> { point });
            }

            else
            {
                // Otherwise find the cluster with the nearest neighbour
                int clusterNumber = this.GetClusterforPoint(point);

                // If we found one, add this point to it
                if (clusterNumber != 0)
                {
                    this.clusters[clusterNumber].Add(point);
                }

                else
                {
                    // Otherwise this is the seed of a new cluster
                    this.clusters.Add(
                        this.clusters.Count + 1,
                        new List<Point> { point });
                }
            }
        }

        /// <summary>
        /// For a given point return the cluster with the mosts 'votes' for
        /// this point, or zero if there is no nearest neighbour
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Integer key of the cluster in this.clusters or zero</returns>
        private int GetClusterforPoint(Point point)
        {
            // Store the current cluster and the current highest vote
            int currentCluster = 0;
            double currentHighVote = 0;

            // Visit each cluster...
            Parallel.ForEach(this.clusters, kvp =>
            {
                // Get a list of the near neighbours
                var neighbours = kvp.Value.Where(clusteredPoint =>
                    clusteredPoint.IsNeighbour(
                    point,
                    this.distanceThreshold));

                // Ask each neighbour to vote for this point
                double votes = 0d;
                neighbours.ToList().ForEach(votingPoint =>
                {
                    votes += votingPoint.VoteForPoint(
                        point,
                        this.distanceThreshold);
                });

                // Did this cluster out vote the others?
                if (votes > currentHighVote)
                {
                    currentHighVote = votes;
                    currentCluster = kvp.Key;
                }
            });

            // Return the winning cluster
            return currentCluster;
        }

        /// <summary>
        /// Colour the clusters on the bitmap for testing purposes
        /// </summary>
        /// <returns>A coloured bitmap showing the clusters</returns>
        public Bitmap ShowClusters()
        {
            // Classify and cluster the bitmap
            Bitmap bm = this.Classify();
            this.ClusterClassifiedBitmap(bm);

            // Cull clusters below threshold
            this.cullClusters();

            // Get all the colours
            KnownColor[] colours = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            var tempColours = colours.ToList();
            tempColours.Remove(KnownColor.Black);
            tempColours.Remove(KnownColor.White);
            colours = tempColours.ToArray();

            // Create a new PRNG
            var rand = new Random(DateTime.Now.Millisecond);

            // Colour the clusters
            foreach (var kvp in this.clusters)
            {
                KnownColor c = (KnownColor)colours.GetValue(rand.Next(colours.Length));
                kvp.Value.ForEach(point =>
                {
                    bm.SetPixel(
                        point.X,
                        point.Y,
                        Color.FromKnownColor(c));
                });
            };

            return bm;
        }

        private void cullClusters()
        {
            // Set a cull threshold
            const int cullThreshold = 25;

            // Collect orphaned points
            List<Point> orphanedPoints = new List<Point>();
            var kvps = this.clusters
                .Where(each => each.Value.Count < cullThreshold)
                .ToList();

            kvps.ForEach(kvp =>
            {
                kvp.Value.ForEach(point => orphanedPoints.Add(point));
            });

            // Remove the clusters
            foreach (var kvp in kvps)
            {
                this.clusters.Remove(kvp.Key);
            }

            // Try to re cluster the orphaned points
            orphanedPoints.ForEach(point =>
            {
                var key = this.GetClusterforPoint(point);
                if (key > 0)
                {
                    this.clusters[key].Add(point);
                }
            });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (this.bitmap != null)
            {
                this.bitmap.Dispose();
                this.bitmap = null;
            }
        }

        public int GetClusterCount()
        {
            return this.clusters.Count;
        }
    }
}
