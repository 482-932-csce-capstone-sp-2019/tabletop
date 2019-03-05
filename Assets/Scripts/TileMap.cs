using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMap : MonoBehaviour 
{

	public GameObject playerPrefab;
	public GameObject selectedUnit;

	public TileType[] tileTypes;

	int[,] tiles;
	int[,] playerLocations;
	List<GameObject> players;
	Node[,] graph;


	int mapSizeX = 20;
	int mapSizeY = 20;

	// Start is called when the object becomes active
	void Start()
	{
		// set up player by getting position
		GameObject player = (GameObject)Instantiate( playerPrefab, new Vector3(0, 0, 0), Quaternion.identity );
		player.GetComponent<Unit>().tileX = (int)player.transform.position.x;
		player.GetComponent<Unit>().tileY = (int)player.transform.position.y;
		player.GetComponent<Unit>().map = this;
		players = new List<GameObject>();
		players.Add(player);

		GenerateMapData();
		GeneratePathfindingGraph();
		GenerateMapVisual();

		addPlayerToTile(player.GetComponent<Unit>().tileX, player.GetComponent<Unit>().tileY);
	}

	void GenerateMapData()
	{
		// Allocate our map tiles
		tiles = new int[mapSizeX,mapSizeY];
		playerLocations = new int[mapSizeX,mapSizeY];
		
		int x,y;
		
		// Initialize our map tiles to be grass
		for(x=0; x < mapSizeX; x++)
		{
			for(y=0; y < mapSizeY; y++)
			{
				// value of 0 represents grass
				tiles[x,y] = 0;
				playerLocations[x,y] = 0;
			}
		}

		// Impassable area
		for(x=3; x <= 5; x++)
		{
			for(y=0; y < 4; y++)
			{
				// value of 1 represents impassable terrain
				tiles[x,y] = 1;
			}
		}
	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
	{

		TileType tt = tileTypes[ tiles[targetX,targetY] ];

		if(UnitCanEnterTile(targetX, targetY) == false)
			return Mathf.Infinity;

		float cost = tt.movementCost;

		if( sourceX!=targetX && sourceY!=targetY)
		{
			// We are moving diagonally!  Fudge the cost for tie-breaking
			// Purely a cosmetic thing!
			cost += 0.001f;
		}

		return cost;

	}

	void GeneratePathfindingGraph()
	{
		// Initialize the array
		graph = new Node[mapSizeX,mapSizeY];

		// Initialize a Node for each spot in the array
		for(int x=0; x < mapSizeX; x++)
		{
			for(int y=0; y < mapSizeY; y++)
			{
				graph[x,y] = new Node();
				graph[x,y].x = x;
				graph[x,y].y = y;
			}
		}

		// Now that all the nodes exist, calculate their neighbours
		for(int x=0; x < mapSizeX; x++)
		{
			for(int y=0; y < mapSizeY; y++)
			{

				// This is the 4-way connection version:
/*				if(x > 0)
					graph[x,y].neighbours.Add( graph[x-1, y] );
				if(x < mapSizeX-1)
					graph[x,y].neighbours.Add( graph[x+1, y] );
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );
*/

				// This is the 8-way connection version (allows diagonal movement)
				// Try left
				if(x > 0)
				{
					graph[x,y].neighbours.Add( graph[x-1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x-1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x-1, y+1] );
				}

				// Try Right
				if(x < mapSizeX-1)
				{
					graph[x,y].neighbours.Add( graph[x+1, y] );
					if(y > 0)
						graph[x,y].neighbours.Add( graph[x+1, y-1] );
					if(y < mapSizeY-1)
						graph[x,y].neighbours.Add( graph[x+1, y+1] );
				}

				// Try straight up and down
				if(y > 0)
					graph[x,y].neighbours.Add( graph[x, y-1] );
				if(y < mapSizeY-1)
					graph[x,y].neighbours.Add( graph[x, y+1] );

				// This also works with 6-way hexes and n-way variable areas
			}
		}
	}

	// create GameObjects based on array of tiles
	void GenerateMapVisual()
	{
		for(int x=0; x < mapSizeX; x++)
		{
			for(int y=0; y < mapSizeY; y++)
			{
				TileType tt = tileTypes[ tiles[x,y] ];
				GameObject go = (GameObject)Instantiate( tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity );

				ClickableTile ct = go.GetComponent<ClickableTile>();
				ct.tileX = x;
				ct.tileY = y;
				ct.map = this;
			}
		}
	}

	public Vector3 TileCoordToWorldCoord(int x, int y)
	{
		return new Vector3(x, y, 0);
	}

	public bool UnitCanEnterTile(int x, int y)
	{

		// We could test the unit's walk/hover/fly type against various
		// terrain flags here to see if they are allowed to enter the tile.

		return tileTypes[ tiles[x,y] ].isWalkable;
	}

	public bool isUserOnTile(int x, int y)
	{
		if (playerLocations[x,y] == 1)
		{
			Debug.Log("User is on tile");
			return true;
		}
		else
		{
			Debug.Log("User is NOT on tile");
			return false;
		}
	}

	public void unselectUnit()
	{
		if (selectedUnit != null)
		{
			Debug.Log("unit deselected");
			selectedUnit.GetComponent<Unit>().selected = Color.white;
			selectedUnit = null;
		}
	}

	public void selectUnit(int x, int y)
	{
		if (playerLocations[x,y] == 1 && selectedUnit == null)
		{
			Debug.Log("unit selected");
			foreach(GameObject player in players)
			{
				if (player.GetComponent<Unit>().tileX == x && player.GetComponent<Unit>().tileY == y)
				{
					selectedUnit = player;
				}
			}
			selectedUnit.GetComponent<Unit>().selected = Color.red;
		}
	}

	public void removePlayerFromTile(int x, int y)
	{
		playerLocations[x,y] = 0;
	}

	public void addPlayerToTile(int x, int y)
	{
		if(playerLocations[x,y] == 0)
		{
			Debug.Log("added to tile");
			playerLocations[x,y] = 1;
			if (selectedUnit != null)
			{
				selectedUnit.GetComponent<Unit>().tileX = x;
				selectedUnit.GetComponent<Unit>().tileY = y;
			}
		}
	}

	public void GeneratePathTo(int x, int y)
	{
		if(selectedUnit == null)
		{
			return;
		}

		// Clear out our unit's old path.
		selectedUnit.GetComponent<Unit>().currentPath = null;

		if( UnitCanEnterTile(x,y) == false )
		{
			// We probably clicked on a mountain or something, so just quit out.
			return;
		}

		Dictionary<Node, float> dist = new Dictionary<Node, float>();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

		// Setup the "Q" -- the list of nodes we haven't checked yet.
		List<Node> unvisited = new List<Node>();
		
		Node source = graph[selectedUnit.GetComponent<Unit>().tileX, 
		                    selectedUnit.GetComponent<Unit>().tileY];
		
		Node target = graph[x,y];
		
		dist[source] = 0;
		prev[source] = null;

		// Initialize everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes CAN'T be reached from the source,
		// which would make INFINITY a reasonable value
		foreach(Node v in graph)
		{
			if(v != source)
			{
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);
		}

		while(unvisited.Count > 0)
		{
			// "u" is going to be the unvisited node with the smallest distance.
			Node u = null;

			foreach(Node possibleU in unvisited)
			{
				if(u == null || dist[possibleU] < dist[u])
				{
					u = possibleU;
				}
			}

			if(u == target)
			{
				break;	// Exit the while loop!
			}

			unvisited.Remove(u);

			foreach(Node v in u.neighbours)
			{
				//float alt = dist[u] + u.DistanceTo(v);
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if( alt < dist[v] )
				{
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		// If we get there, the either we found the shortest route
		// to our target, or there is no route at ALL to our target.

		if(prev[target] == null)
		{
			// No route between our target and the source
			return;
		}

		List<Node> currentPath = new List<Node>();

		Node curr = target;

		// Step through the "prev" chain and add it to our path
		while(curr != null)
		{
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// Right now, currentPath describes a route from out target to our source
		// So we need to invert it!

		currentPath.Reverse();

		selectedUnit.GetComponent<Unit>().currentPath = currentPath;
	}

}
