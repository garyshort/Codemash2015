using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace PotatoCountingDemo
{
    /// <summary>
    /// A class for detecting and highlighting BlackGrass in an image of 
    /// cereal crops
    /// </summary>
    public class BlackGrassDetector : IDisposable
    {
        private int[] blackGrassProbabilityDistribution;
        private int[] cropProbabilityDistribution;
        private Bitmap sample;
        private Bitmap cropSample;
        private Bitmap blackGrassSample;


        /// <summary>
        /// Initializes a new instance of the BlackGrassDetector class.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="cropSample"></param>
        /// <param name="blackGrassSample"></param>
        public BlackGrassDetector(Bitmap sample, Bitmap cropSample, Bitmap blackGrassSample)
        {
            this.sample = sample;
            this.cropSample = cropSample;
            this.blackGrassSample = blackGrassSample;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (sample != null)
            {
                sample.Dispose();
                sample = null;
            }
            if (cropSample != null)
            {
                cropSample.Dispose();
                cropSample = null;
            }
            if (blackGrassSample != null)
            {
                blackGrassSample.Dispose();
                blackGrassSample = null;
            }
        }

        /// <summary>
        /// Detects Blackgrass, based on crop versus Blackgrass samples
        /// </summary>
        /// <returns>Returns an image highlighting areas of Blackgrass</returns>
        public Bitmap DetectBlackGrass()
        {
            this.CreateCropProbabilityDistribution();
            this.CreateBlackGrassProbabilityDistibution();

            // Create a copy of the sample image for updating
            Bitmap copySample = new Bitmap(this.sample);

            // Walk each pixel...
            for (int x = 0; x < copySample.Width; x++)
            {
                for (int y = 0; y < copySample.Height; y++)
                {

                    // Get the green intensity of this pixel...
                    int intensity = copySample.GetPixel(x, y).G;

                    // If it is any sort of green...
                    if (intensity > 0)
                    {
                        // And of it being Blackgrass...
                        double isBlackgrass =
                            this.ProbabilityIntensityIsBlackGrass(intensity);

                        // It it's more likely to be Blackgrass colour it black
                        if (isBlackgrass > 0.5d)
                        {
                            copySample.SetPixel(x, y, Color.Black);
                        }
                    }
                }
            }

            // Return the coloured bitmap
            return copySample;
        }

        /// <summary>
        /// Calculate the probablity of a given intensity being Blackgrass
        /// using Bayes theorum P(A|B) = P(B|A) * P(A) / P(B)
        /// </summary>
        /// <param name="intensity"></param>
        /// <returns>The probablity of Blackgrass</returns>
        private double ProbabilityIntensityIsBlackGrass(int intensity)
        {
            // Calculate the constituent parts
            double probOfIntensityGivenBG =
                ((double)this.blackGrassProbabilityDistribution[intensity] /
                (double)this.blackGrassProbabilityDistribution.Sum());

            double probOfBG = 
                (double)this.blackGrassProbabilityDistribution.Sum() /
                ((double)this.blackGrassProbabilityDistribution.Sum() +
                (double)this.cropProbabilityDistribution.Sum());

            double probOfIntensity =
                ((double)this.cropProbabilityDistribution[intensity] +
                (double)this.blackGrassProbabilityDistribution[intensity]) /
                ((double)this.blackGrassProbabilityDistribution.Sum() +
                (double)this.cropProbabilityDistribution.Sum());

            // Defend against devide by zero error
            if (probOfIntensity == 0) { return 0d; }


            // Calculate and return the probability
            return probOfIntensityGivenBG * probOfBG / probOfIntensity;
        }

        /// <summary>
        /// Create a histogram for Blackgrass intensities
        /// </summary>
        private void CreateBlackGrassProbabilityDistibution()
        {
            int[] histogram = new int[256];

            for (int x = 0; x < this.blackGrassSample.Width; x++)
            {
                for (int y = 0; y < this.blackGrassSample.Height; y++)
                {
                    histogram[this.blackGrassSample.GetPixel(x, y).G]++;
                }
            }

            this.blackGrassProbabilityDistribution = histogram;
        }

        /// <summary>
        /// Calculate a histogram for crop intensities
        /// </summary>
        private void CreateCropProbabilityDistribution()
        {
            int[] histogram = new int[256];

            for (int x = 0; x < this.cropSample.Width; x++)
            {
                for (int y = 0; y < this.cropSample.Height; y++)
                {
                    histogram[this.cropSample.GetPixel(x, y).G]++;
                }
            }

            this.cropProbabilityDistribution = histogram;
        }
    }
}
