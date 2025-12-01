using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

/// <summary>
/// Class to hold indexed vectors
/// </summary>
public class DataIndex
{
    public Dictionary<int, HsnwNode> Nodes { get; set; } = new Dictionary<int, HsnwNode>();

    public int EntryPointId { get; private set; } //id of the entry point node
    public int MaxLevel { get; private set; } //highest level in the graph
    public int MaxNeighbours { get; init; } //max number of neighbors per node
    public int EfConstruction { get; init; } //size of candidate list during construction
    public float InverseLogM { get; init; } //Controls the probability of a node being assigned to higher levels


    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// </summary>
    /// <param name="vectorA"></param>
    /// <param name="vectorB"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be of the same length");

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;
        
        // Calculate dot product and magnitudes
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        // Avoid division by zero
        if (magnitudeA == 0 || magnitudeB == 0)
            return 0f;

        // Calculate and return cosine similarity
        return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    /// <summary>
    /// Get random level for a new node based on inverseLogM
    /// </summary>
    /// <param name="inverseLogM"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    public static int GetRandomLevel(float inverseLogM, Random random)
    {
        // Using negative logarithm to determine level
        return (int)(-Math.Log(random.NextDouble()) * inverseLogM);
    }

    public void Insert(HsnwNode newNode, Random random)
    {
        if (Nodes.Count == 0)
        {
            newNode.level = 0;
            //Initialize neighbor lists for the first node
            newNode.Neighbors = new List<int>[1];
            newNode.Neighbors[0] = new List<int>(MaxNeighbours);
            Nodes.Add(newNode.id, newNode);
            EntryPointId = newNode.id;
            MaxLevel = 0;
            return;
        }

        //Assign level to new node
        int newNodeLevel = GetRandomLevel(InverseLogM, random);
        newNode.level = newNodeLevel;

        //Initialize neighbor lists for each level
        newNode.Neighbors = new List<int>[newNodeLevel + 1];
        for (int level = 0; level <= newNodeLevel; level++)
        {
            newNode.Neighbors[level] = new List<int>(MaxNeighbours);
        }

        int currentEntryId = EntryPointId;
        Nodes.Add(newNode.id, newNode);

        //Search from top level down to the level of the new node
        for (int level = MaxLevel; level > newNodeLevel; level--)
        {
            if(level < Nodes[currentEntryId].Neighbors.Length)
            {
                //Search only if the current entry node has neighbors at this level
                List<int> candidates = SearchLayer(newNode.Vector, currentEntryId, level, ef: 1);
                //Update entry point for next level
                if(candidates.Count > 0)
                {
                currentEntryId = candidates[0];
                }
            }
        }

        //Connect the new node to neighbors at each level up to its level
        for (int level = Math.Min(newNode.level, MaxLevel); level >= 0; level--)
        {
            List<int> candidates = SearchLayer(newNode.Vector, currentEntryId, level, EfConstruction);
            System.Console.WriteLine($"[Insert] Level {level}: Found {candidates.Count} candidates from entry point {currentEntryId}");
            //Select neighbors to connect using heuristic
            List<int> neigborsToConnect = SelectNeighbors(newNode.Vector, candidates, MaxNeighbours);
            System.Console.WriteLine($"[Insert] Level {level}: Selected {neigborsToConnect.Count} neighbors to connect");
            //Establish connections to new node
            newNode.Neighbors[level].AddRange(neigborsToConnect);

            foreach (int neighborId in neigborsToConnect)
            {
                HsnwNode neighborNode = Nodes[neighborId];
                neighborNode.Neighbors[level].Add(newNode.id);

                if(neighborNode.Neighbors[level].Count > MaxNeighbours)
                {
                    ShrinkConnections(neighborId, level);
                }
            }
            // Update current entry point for next level
            if(candidates.Count > 0)
            {
                currentEntryId = candidates[0];
            }  
        }
        //Update entry point and max level if new node's level is highest
        if(newNode.level > MaxLevel)
        {
            MaxLevel = newNode.level;
            EntryPointId = newNode.id;
        }

    }
    
    public List<int> FindNearestNeighbors(float[] queryVector, int k, int? efSearch = null)
    {
        if (Nodes.Count == 0)
        {
            return new List<int>();
        }

        int ef = efSearch ?? EfConstruction;

        int currentEntryId = EntryPointId;

        for(int level = MaxLevel; level >= 1; level--)
        {
            List<int> candidates = SearchLayer(queryVector, currentEntryId, level, ef: 1);
            if(candidates.Count > 0)
            {
                currentEntryId = candidates.First();
            }
            else
            {
                //NO ACTIONS NEEDED
                continue;
            }
        }

        List<int> finalCandidates = SearchLayer(queryVector, currentEntryId, 0, ef);
        return finalCandidates.Take(k).ToList();
    }

    /// <summary>
    /// Select neighbors using heuristic to ensure diversity
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
            Distance = 1.0f - CosineSimilarity(queryVector, Nodes[id].Vector)
        })
        .OrderBy(x => x.Distance)
        .ToList();

        //Select neighbors ensuring diversity
        List<int> selectedNeighbors = new List<int>();
        HashSet<int> selectedSet = new HashSet<int>();

        //First pass: select diverse neighbors
        foreach(var candidate in sortedCandidates)
        {
            if(selectedNeighbors.Count >= maxConnections)
            {
                break;
            }

            bool isDiverse = true;
            //Check diversity against already selected neighbors
            foreach(int selectedId in selectedNeighbors)
            {
                float distanceBetween = 1.0f - CosineSimilarity(Nodes[candidate.Id].Vector, Nodes[selectedId].Vector);
                if(distanceBetween < candidate.Distance)
                {
                    isDiverse = false;
                    break;
                }
            }

            if(isDiverse)
            {
                selectedNeighbors.Add(candidate.Id);
                selectedSet.Add(candidate.Id);
            }
        }

        //Second pass: fill remaining slots if needed
        if(selectedNeighbors.Count < maxConnections)
        {
            foreach(var candidate in sortedCandidates)
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
    /// <param name="CandidateQueue"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    private static List<int> GetCandidatesFromQueue(SortedDictionary<float, List<int>> CandidateQueue, int limit)
    {
        List<int> result = new List<int>();
        foreach (var entry in CandidateQueue)
        {
            foreach (var id in entry.Value)
            {
                if(result.Count < limit)
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
    /// <param name="queryVector"></param>
    /// <param name="entryId"></param>
    /// <param name="layer"></param>
    /// <param name="ef"></param>
    /// <returns></returns>
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
        float entryDistance = 1.0f - CosineSimilarity(queryVector, entryNode.Vector);

        //Add to candidate queue and visited set
        candidateQueue.Add(entryDistance, new List<int> { entryId });
        visitedSet.Add(entryId);

        while(candidateQueue.Count > 0)
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
            if(!resultQueue.ContainsKey(currentBestDistance))
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

                foreach(int neighborId in neighborsAtLevel)
                {
                    if (visitedSet.Add(neighborId))
                    {
                        //Calculate distance to neighbor
                        HsnwNode neighborNode = Nodes[neighborId];
                        float neighborDistance = 1.0f - CosineSimilarity(queryVector, neighborNode.Vector);
                        
                        //Add neighbor to candidate queue if it qualifies
                        if(candidateQueue.Count < ef || neighborDistance < worstDistanceInResults)
                        {
                            //Add to candidate queue
                            if(!candidateQueue.ContainsKey(neighborDistance))
                            {
                                candidateQueue[neighborDistance] = new List<int>();
                            }
                            // Add neighbor ID to the list for this distance
                            candidateQueue[neighborDistance].Add(neighborId);

                            //If candidate queue exceeds ef, remove the worst candidate
                            if(candidateQueue.Count > ef)
                            {
                                float LastKey = candidateQueue.Keys.Last();
                                candidateQueue.Remove(LastKey);
                            }
                        }
                    }
                }
            }
        }
        return GetCandidatesFromQueue(resultQueue, ef);
    }


}