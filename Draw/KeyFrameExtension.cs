using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using StorybrewCommon.Animations;
using Microsoft.CSharp;

namespace StorybrewScripts
{
    public static class KeyframedValueExtensions
    {
        // Extension method for KeyframedValue<T>
        public static void Simplify<T>(this KeyframedValue<T> keyframedValue, double tolerance)
        {
            if (keyframedValue == null)
                throw new ArgumentNullException(nameof(keyframedValue));

            if (typeof(T) == typeof(Vector2))
            {
                var castedValue = keyframedValue as KeyframedValue<Vector2>;
                Simplify2D(castedValue, tolerance);
            }
            else if (typeof(T) == typeof(float))
            {
                var castedValue = keyframedValue as KeyframedValue<float>;
                Simplify1D(castedValue, tolerance);
            }
            else if (typeof(T) == typeof(double))
            {
                var castedValue = keyframedValue as KeyframedValue<double>;
                Simplify1D(castedValue, tolerance);
            }
            else
            {
                throw new InvalidOperationException("Unsupported type for SimplifyMethod");
            }
        }

        private static void Simplify2D(KeyframedValue<Vector2> keyframedValue, double tolerance)
        {
            if (tolerance != 0)
            {
                keyframedValue.Simplify2dKeyframes(tolerance, v => v);
            }
            else
            {
                keyframedValue.SimplifyEqualKeyframes();
            }
        }

        private static void Simplify1D(KeyframedValue<float> keyframedValue, double tolerance)
        {
            if (tolerance != 0)
            {
                keyframedValue.Simplify1dKeyframes(tolerance, v => v);
            }
            else
            {
                keyframedValue.SimplifyEqualKeyframes();
            }
        }

        private static void Simplify1D(KeyframedValue<double> keyframedValue, double tolerance)
        {
            if (tolerance != 0)
            {
                keyframedValue.Simplify1dKeyframes(tolerance, v => (float)v);
            }
            else
            {
                keyframedValue.SimplifyEqualKeyframes();
            }
        }

    }
}