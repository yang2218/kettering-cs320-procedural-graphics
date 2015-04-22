# Inputs #
  * **Circles**: A list of cirlces
    * `IList<Vector3>` where x,y are `[0..1]` normalized position coordinates, and z is a `[0..1]` normalized radius.
    * The first circle will be assumed to be the largest.
  * **Cap**: A CapType (Flat, Cone, Hemisphere)
  * **HeightMap** (`byte[,]`): should be square and a power-of-2 side length
  * **BlendFunc**: A blending function
  * **BlendFuncSrcFactor** and **BlendFuncDstFactor**: blending factors

# Outputs #
  * **HeightMap**: A new HeightMap with the same size as the Input heightmap

# Description #

Rasterizes the circles onto the heigtmap, using the specified cap style, and with height based on the circle's radius (perhaps some parameters to control this will be added).

(TODO: describe the algorithms to be used to perform the rasterization/extrusion)