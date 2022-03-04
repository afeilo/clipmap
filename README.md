
- 地形
    - clipmap：渲染地形的一种lod技术，伴随视角的移动而更新网格。渲染中为了处理平滑移动、接缝等不得不渲染一些额外的填充网格。参考：
        - [Geometry clipmaps: Terrain rendering using nested regular grids](https://hhoppe.com/proj/geomclipmap/) 
        - [Chapter 2. Terrain Rendering Using GPU-Based Geometry Clipmaps](https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry)
        - [Geometry clipmaps: simple terrain rendering with level of detail](https://mikejsavage.co.uk/blog/geometry-clipmaps.html)
    - cdlod：通过顶点x、z的lerp达到一种更加平滑的lod过渡，无需拼接网格进行接缝的处理。参考
        - [cdlod](https://github.com/fstrugar/CDLOD)

    - VirtualTexture
        - [svt](http://www.silverspaceship.com/src/svt/)
