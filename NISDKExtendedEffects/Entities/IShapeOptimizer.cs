using System.Collections.Generic;
using System.Windows;

namespace NISDKExtendedEffects.Entities
{
    public interface IShapeOptimizer
    {
        /// <summary>
        /// Optimize specified shape.
        /// </summary>
        ///
        /// <param name="shape">Shape to be optimized.</param>
        ///
        /// <returns>Returns final optimized shape, which may have reduced amount of points.</returns>
        ///
        List<Point> OptimizeShape(List<Point> shape);
    }
}