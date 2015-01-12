using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoCountingDemo
{
    public static class Extensions
    {
        /// <summary>
        /// Return if the two points are with threshold distance of each other
        /// </summary>
        /// <param name="clustered"> A point in a cluster</param>
        /// <param name="test"> A candidate point for neighbour 
        /// to clustered</param>
        /// <param name="threshold">How close the two points must be to be 
        /// neighbours</param>
        /// <returns>bool</returns>
        public static bool IsNeighbour(
            this Point clustered,
            Point test,
            int threshold)
        {
            // Calculate how close the points are using Euclidean Distance
            return DistanceFrom(clustered, test) <= threshold;
        }

        public static double VoteForPoint(
            this Point votingPoint,
            Point test,
            int threshold)
        {
            return 1d / votingPoint.DistanceFrom(test);
        }

        public static double DistanceFrom(this Point clustered, Point test)
        {
            return Math.Sqrt(
                Math.Pow(clustered.X - test.X, 2) +
                Math.Pow(clustered.Y - test.Y, 2));
        }
    }
}
