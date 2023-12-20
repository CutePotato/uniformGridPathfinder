﻿# Hierarchical Jump Point Search Pathfinding

The motivation of this project is to present a possible solution for turn-based tactical game in regard of Pathfinding.

This project it's a modification of the original [Hierarchical Pathfinding](https://github.com/hugoscurti/hierarchical-pathfinding/tree/master) from Hugo Scurti.

In this project, I explore the concept of [JPS](http://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf) presented by Daniel Harabor and Alban Grastien and change the way nodes connection is done by applying latter concept. Which significantly improve velocity at which destination path is obtained.

Hopefully it can be used as start point for Unity developers that try to developer own solution. 

# Notes

This project is a prototype and could be improved applying Steve Rabin and Fernando Silva work optimizations through [JPS+](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter14_JPS_Plus_An_Extreme_A_Star_Speed_Optimization_for_Static_Uniform_Cost_Grids.pdf)

# How to test

1. Clone the repo

2. Import [Optimize Priority Queue](https://www.nuget.org/packages/OptimizedPriorityQueue) into the project Asset folder

3. Open Sample Scene to Test
   - In MapManager you can add different links that connect the Surfaces(Clusters)
   - You can add obstacles in Surface scripts.
   - Once you press Play,

- - -

# Credits

* HPA* was introduced in [this paper](http://www.cs.ualberta.ca/~mmueller/ps/hpastar.pdf) written by Botea, Müller, and Schaeffer.

* All .map and .scen files used for testing come from Moving AI's [2d Pathfinding benchmark sets](https://movingai.com/benchmarks/grids.html)

* The Priority Queue used in this project comes from BlueRaja's [High Speed Priority Queue for C#](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp).  All files in folder **Priority Queue** comes from the repo's **Priority Queue** folder.