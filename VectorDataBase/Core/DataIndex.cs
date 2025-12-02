using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class to hold indexed vectors, now relying on HSNWUtils for distance and level calculation.
/// </summary>
public class DataIndex : IDataIndex
{
    public Dictionary<int, HsnwNode> Nodes { get; set; } = new Dictionary<int, HsnwNode>();

    public int EntryPointId { get; private set; } //id of the entry point node
    public int MaxLevel { get; private set; } //highest level in the graph
    public int MaxNeighbours { get; init; } //max number of neighbors per node
    public int EfConstruction { get; init; } //size of candidate list during construction
    public float InverseLogM { get; init; } //Controls the probability of a node being assigned to higher levels
    
    /// <summary>
    /// Insert a new node into the HSNW index
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="random"></param>
    public void Insert(HsnwNode newNode, Random random)
    {
        if (Nodes.Count == 0)
        {
            InitializeFirstNode(newNode);
            return;
        }

        // Initialization: Set level and prepare structure
        int newNodeLevel = InitializeNewNode(newNode, random);
        int currentEntryId = EntryPointId;

        // Search Top Layers: Find the closest node in layers above the new node's level
        currentEntryId = SearchTopLayers(newNode, newNodeLevel, currentEntryId);

        // Layer Connection: Connect the new node in all relevant layers
        ConnectLayers(newNode, newNodeLevel, currentEntryId);

        // Update Global State
        UpdateMaxState(newNode);
    }

    /// <summary>
    /// Initialize the first node in the index
    /// </summary>
    /// <param name="newNode"></param>
    private void InitializeFirstNode(HsnwNode newNode)
    {
        newNode.level = 0;
        newNode.Neighbors = new List<int>[1];
        newNode.Neighbors[0] = new List<int>(MaxNeighbours);
        Nodes.Add(newNode.id, newNode);
        EntryPointId = newNode.id;
        MaxLevel = 0;
    }

    /// <summary>
    /// Initialize new node: assign level and prepare neighbor lists
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    private int InitializeNewNode(HsnwNode newNode, Random random)
    {
        // Calculate and assign level
        int newNodeLevel = HSNWUtils.GetRandomLevel(InverseLogM, random);
        newNode.level = newNodeLevel;

        // Initialize neighbor lists for each level up to the new node's level
        newNode.Neighbors = new List<int>[newNodeLevel + 1];
        for (int level = 0; level <= newNodeLevel; level++)
        {
            newNode.Neighbors[level] = new List<int>(MaxNeighbours);
        }
        Nodes.Add(newNode.id, newNode);
        return newNodeLevel;
    }

    /// <summary>
    /// Search from the top layers down to the new node's level to find the closest entry point
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="newNodeLevel"></param>
    /// <param name="currentEntryId"></param>
    /// <returns></returns>
    private int SearchTopLayers(HsnwNode newNode, int newNodeLevel, int currentEntryId)
    {
        // Search from MaxLevel down to newNodeLevel + 1
        for (int level = MaxLevel; level > newNodeLevel; level--)
        {
            if (level < Nodes[currentEntryId].Neighbors.Length)
            {
                // Search with ef=1 to quickly find the next best entry point
                List<int> candidates = SearchLayer(newNode.Vector, currentEntryId, level, ef: 1);
                
                if (candidates.Count > 0)
                {
                    currentEntryId = candidates[0];
                }
            }
        }
        return currentEntryId;
    }

    /// <summary>
    /// Connect the new node to neighbors in each layer down to level 0
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="newNodeLevel"></param>
    /// <param name="currentEntryId"></param>
    private void ConnectLayers(HsnwNode newNode, int newNodeLevel, int currentEntryId)
    {
        // Connect from min(newNodeLevel, MaxLevel) down to 0
        for (int level = Math.Min(newNodeLevel, MaxLevel); level >= 0; level--)
        {
            // Search the layer to find potential neighbors
            List<int> candidates = SearchLayer(newNode.Vector, currentEntryId, level, EfConstruction);
            
            // Select neighbors using heuristic
            List<int> neighborsToConnect = SelectNeighbors(newNode.Vector, candidates, MaxNeighbours);
            
            // Establish connections
            newNode.Neighbors[level].AddRange(neighborsToConnect);

            // Connect neighbors back to the new node and manage their connections
            foreach (int neighborId in neighborsToConnect)
            {
                HsnwNode neighborNode = Nodes[neighborId];
                neighborNode.Neighbors[level].Add(newNode.id);

                if (neighborNode.Neighbors[level].Count > MaxNeighbours)
                {
                    ShrinkConnections(neighborId, level);
                }
            }

            // Update entry point for next level's search
            if (candidates.Count > 0)
            {
                currentEntryId = candidates[0];
            }
        }
    }

    /// <summary>
    /// Update global index state if the new node has the highest level
    /// </summary>
    /// <param name="newNode"></param>
    private void UpdateMaxState(HsnwNode newNode)
    {
        if (newNode.level > MaxLevel)
        {
            MaxLevel = newNode.level;
            EntryPointId = newNode.id;
        }
    }
    
    /// <summary>
    /// Find nearest neighbors for a given query vector
    /// </summary>
    /// <param name="queryVector"></param>
    /// <param name="k"></param>
    /// <param name="efSearch"></param>
    /// <returns></returns>
    public List<int> FindNearestNeighbors(float[] queryVector, int k, int? efSearch = null)
    {
        if (Nodes.Count == 0)
        {
            return new List<int>();
        }

        int ef = efSearch ?? EfConstruction;

        int currentEntryId = EntryPointId;

        for (int level = MaxLevel; level >= 1; level--)
        {
            List<int> candidates = SearchLayer(queryVector, currentEntryId, level, ef: 1);
            if (candidates.Count > 0)
            {
                currentEntryId = candidates.First();
            }
            // If no candidates found, continue with the currentEntryId
        }

        List<int> finalCandidates = SearchLayer(queryVector, currentEntryId, 0, ef);
        return finalCandidates.Take(k).ToList();
    }
    
    /// <summary>
    /// Select neighbors ensuring diversity using heuristic
    /// </summary>
    /// <param name="queryVector"></param>
    /// <param name="candidates"></param>
    /// <param name="maxConnections"></param>
    /// <returns></returns>
    public List<int> SelectNeighbors(float[] queryVector, List<int> candidates, int maxConnections)
    {
        //Sort candidates by distance to query vector
        var sortedCandidates = candidates.Select(id => new
        {
            Id = id,
            Distance = 1.0f - HSNWUtils.CosineSimilarity(queryVector, Nodes[id].Vector)
        })
        .OrderBy(x => x.Distance)
        .ToList();

        //Select neighbors ensuring diversity
        List<int> selectedNeighbors = new List<int>();
        HashSet<int> selectedSet = new HashSet<int>();

        //First pass: select diverse neighbors
        foreach (var candidate in sortedCandidates)
        {
            if (selectedNeighbors.Count >= maxConnections)
            {
                break;
            }

            bool isDiverse = true;
            //Check diversity against already selected neighbors
            foreach (int selectedId in selectedNeighbors)
            {
                float distanceBetween = 1.0f - HSNWUtils.CosineSimilarity(Nodes[candidate.Id].Vector, Nodes[selectedId].Vector);
                if (distanceBetween < candidate.Distance)
                {
                    isDiverse = false;
                    break;
                }
            }

            if (isDiverse)
            {
                selectedNeighbors.Add(candidate.Id);
                selectedSet.Add(candidate.Id);
            }
        }

        //Second pass: fill remaining slots if needed
        if (selectedNeighbors.Count < maxConnections)
        {
            foreach (var candidate in sortedCandidates)
            {
                if (selectedNeighbors.Count >= maxConnections)
                    break;
                if (selectedSet.Add(candidate.Id))
                {
                    selectedNeighbors.Add(candidate.Id);
                }

            }
        }
        return selectedNeighbors;
    }

    /// <summary>
    /// Shrink connections of a node to maintain max neighbors using selection heuristic
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="layer"></param>
    public void ShrinkConnections(int nodeId, int layer)
    {
        HsnwNode node = Nodes[nodeId];
        List<int> currentNeighbors = node.Neighbors[layer];

        if (currentNeighbors.Count <= MaxNeighbours)
            return;

        float[] nodeVector = node.Vector;
        List<int> neighborids = currentNeighbors;

        List<int> selectedNeighbors = SelectNeighbors(nodeVector, neighborids, MaxNeighbours);
        node.Neighbors[layer] = selectedNeighbors;
    }


    /// <summary>
    /// Helper method to extract candidate IDs from the sorted candidate queue
    /// </summary>
    private List<int> GetCandidatesFromQueue(SortedDictionary<float, List<int>> CandidateQueue, int limit)
    {
        List<int> result = new List<int>();
        foreach (var entry in CandidateQueue)
        {
            foreach (var id in entry.Value)
            {
                if (result.Count < limit)
                {
                    result.Add(id);
                }
                else
                {
                    return result;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Search for nearest neighbors in a given layer
    /// </summary>
    public List<int> SearchLayer(float[] queryVector, int entryId, int layer, int ef)
    {
        //Assuming lower key = higher similarity
        SortedDictionary<float, List<int>> candidateQueue = new SortedDictionary<float, List<int>>();
        
        //Track results separately (nodes we've explored neighbors from)
        SortedDictionary<float, List<int>> resultQueue = new SortedDictionary<float, List<int>>();

        //Set to keep track of visited nodes
        HashSet<int> visitedSet = new HashSet<int>();

        //Initialize with entry point
        HsnwNode entryNode = Nodes[entryId];
        //Convert cosine similarity to distance
        float entryDistance = 1.0f - HSNWUtils.CosineSimilarity(queryVector, entryNode.Vector);

        //Add to candidate queue and visited set
        candidateQueue.Add(entryDistance, new List<int> { entryId });
        visitedSet.Add(entryId);

        while (candidateQueue.Count > 0)
        {
            //Get current best candidate
            float currentBestDistance = candidateQueue.Keys.First();
            int currentBestId = candidateQueue[currentBestDistance].First();
            candidateQueue[currentBestDistance].RemoveAt(0);
            //Remove key if no more IDs
            if (candidateQueue[currentBestDistance].Count == 0)
            {
                candidateQueue.Remove(currentBestDistance);
            }

            //Get current worst result distance (The largest key in the result queue)
            float worstDistanceInResults = resultQueue.Count > 0 ? resultQueue.Keys.Last() : float.MaxValue;

            if (currentBestDistance > worstDistanceInResults && resultQueue.Count >= ef)
            {
                break;
            }

            //Add to result queue
            if (!resultQueue.ContainsKey(currentBestDistance))
            {
                resultQueue[currentBestDistance] = new List<int>();
            }
            resultQueue[currentBestDistance].Add(currentBestId);

            //Explore neighbors of the current best node
            HsnwNode currentNode = Nodes[currentBestId];

            //Get neighbors at the current layer (check if layer exists for this node)
            if (layer < currentNode.Neighbors.Length)
            {
                List<int> neighborsAtLevel = currentNode.Neighbors[layer];

                foreach (int neighborId in neighborsAtLevel)
                {
                    if (visitedSet.Add(neighborId))
                    {
                        //Calculate distance to neighbor
                        HsnwNode neighborNode = Nodes[neighborId];
                        float neighborDistance = 1.0f - HSNWUtils.CosineSimilarity(queryVector, neighborNode.Vector);

                        //Add neighbor to candidate queue if it qualifies
                        if (candidateQueue.Count < ef || neighborDistance < worstDistanceInResults)
                        {
                            //Add to candidate queue
                            if (!candidateQueue.ContainsKey(neighborDistance))
                            {
                                candidateQueue[neighborDistance] = new List<int>();
                            }
                            // Add neighbor ID to the list for this distance
                            candidateQueue[neighborDistance].Add(neighborId);

                            //If candidate queue exceeds ef, remove the worst candidate
                            if (candidateQueue.Count > ef)
                            {
                                float LastKey = candidateQueue.Keys.Last();
                                candidateQueue[LastKey].RemoveAt(candidateQueue[LastKey].Count - 1);
                                if (candidateQueue[LastKey].Count == 0)
                                {
                                    candidateQueue.Remove(LastKey);
                                }
                            }
                        }
                    }
                }
            }
        }
        return GetCandidatesFromQueue(resultQueue, ef);
    }
}