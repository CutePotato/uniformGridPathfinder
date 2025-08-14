# Hierarchical Jump Point Search Pathfinding

The motivation of this project is to present a possible solution for turn-based tactical game in regard of Pathfinding.

This project's a modification of the original [Hierarchical Pathfinding](https://github.com/hugoscurti/hierarchical-pathfinding/tree/master) from Hugo Scurti.

In this project, I explore the concept of [JPS](http://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf) presented by Daniel Harabor and Alban Grastien and change the way node connection is done by applying the latter concept. Which significantly improves the velocity at which a destination path is obtained.

Hopefully, it can be used as a start point for Unity developers that try to developer their own solution. 

# Notes

This project is a prototype and could be improved by applying Steve Rabin and Fernando Silva work optimizations through [JPS+](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter14_JPS_Plus_An_Extreme_A_Star_Speed_Optimization_for_Static_Uniform_Cost_Grids.pdf)

# How to test

1. Clone the repo

2. Open Sample Scene to Test <br>
   [![Video Explanation](https://i9.ytimg.com/vi_webp/iVV3njkSY-o/mqdefault.webp?v=689e38d5&sqp=CLjy-MQG&rs=AOn4CLDBKooMNpGQbwN5qStQnCN6sj8wLA)](https://youtu.be/iVV3njkSY-o)


- - -

# Credits

* Original repository this project is based on [Hierarchical Pathfinding](https://github.com/hugoscurti/hierarchical-pathfinding/tree/master) from Hugo Scurti.

* HPA* was introduced in [this paper](http://www.cs.ualberta.ca/~mmueller/ps/hpastar.pdf) written by Botea, Müller, and Schaeffer.

* JPS was introduced in [JPS](http://users.cecs.anu.edu.au/~dharabor/data/papers/harabor-grastien-aaai11.pdf) presented by Daniel Harabor and Alban Grastien.

* The Priority Queue used in this project comes from BlueRaja's [High Speed Priority Queue for C#](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp).  All files in folder **Priority Queue** comes from the repo's **Priority Queue** folder.