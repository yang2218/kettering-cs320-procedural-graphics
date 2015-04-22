# Parameter Types #

## HeightMap ##
**`byte[,]`** where the dimensions are the same length and a power of 2.

## ColorMap ##
**`int[,]`** RGBA; the dimensions are the same length and a power of 2.

## Circles ##
**`IList<Vector3>`**: x,y represent a circle's center position in the interval `[0,1]`, and z represents its radius in the interval `[0,1]`, where 1 is the width of the HeightMap

# Modules #

## Initial Shape ##
Inputs:
(none)
Outputs:
  1. List of points
    * vertices of a simple convex polygon

## Apollonian Gasket ##
Inputs:
  1. List of points
    * must be a simple convex polygon
Outputs:
  1. List of point and radius pairs
    * Perhaps as a list of vector3f?

## Fill Circles ##
Inputs:
  1. List of point and radius pairs (vector3f?)
Outputs
  1. Heigtmap
Fills circles in bitmap with a constant color (based on radius?)

## Radial Circles ##
Inputs:
  1. List of point and radius pairs (vector3f?)
Outputs
  1. Heightmap
Radial gradients from center of each circle (intensity based on radius?)

## Blend Heightmap ##
Input:
  1. Heightmap 1
  1. Heightmap 2
  1. Blending Function
  1. Blending Factor?
Output
  1. Heightmap
Blending functions: Add, multiply, alpha blend, etc

## Color Based on Altitude ##
Inputs:
  1. Heightmap
  1. coloring parameters?
Outputs:
  1. Colormap