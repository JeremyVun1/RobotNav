============
How to Use
============
> dotnet run <mapName> <method> <options>

Example
> dotnet run largenw BFS

If no method specified, will default to BFS. Map filename is required (see data folder for example maps)

============
Maps
============
See data folder for examples.

[10,10] //width and height
(0,0) //starting position
(9,9)|(8,8) //goal positions
(2,4,4,1) //(x, y, w, h) of wall segment
(4,0,1,4)
(2,5,1,2)
(7,6,1,4)

============
Methods
============
BFS 	(Breadth First Search)
DFS 	(Depth First Search)
GBFS 	(Greedy Best First Search)
AS 		(A Star)
ASFS 	(A Star with fast stack)
GA 		(Genetic Algorithm)
JPS 	(Jump Point Search)

============
Options (Genetic Algorithm)
============
> dotnet run <mapName> GA <Population size> <Mutation Rate> <Fitness Multiplier> <Diversity Selection> <Elitism> <deepening increment>

Example
> dotnet run <mapName> GA 20 0.04 1 f t 1

Population Size: 20
Mutation Rate: 0.04
Fitness Multiplier: 1 (higher is more selective)
Diversity Selection: f (false. do not score individual diversity from mean)
Elitism: t (true. add global best to each generation)
Deepening Increment: 1 (rate at which depth limit increases)

============
GUI
============
Grid numbers represent the relevant metric used to evaluate each node. e.g. GBFS numbers are the heuristic value, A* numbers are the f value, BFS numbers are the g value.

Color code
Yellow: expanded nodes (closed set)
Light Green: Best path found
Light Blue: frontier nodes (open set)
