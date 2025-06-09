using MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MCTS
{
    public class MCTSParallel
    {
        private readonly GameRules _rules;
        private readonly int _maxIterations;
        private readonly double _explorationParameter;
        private readonly int _numThreads;
        private readonly Random _random = new Random();
        private readonly object _rootLock = new object();

        public MCTSParallel(GameRules rules, int maxIterations = 10000, double explorationParameter = 1.414, int numThreads = 4)
        {
            _rules = rules;
            _maxIterations = maxIterations;
            _explorationParameter = explorationParameter;
            _numThreads = numThreads;
        }

        public GameMove FindBestMove(GameState state, int playerId)
        {
            var root = new MCTSNode(state.Clone(), null, null, playerId);
            var iterationsPerThread = _maxIterations / _numThreads;
            var tasks = new Task[_numThreads];

            for (int i = 0; i < _numThreads; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() => ParallelSearch(root, iterationsPerThread, threadId));
            }

            Task.WaitAll(tasks);

            var bestChild = root.SelectChild();
            return bestChild?.Move;
        }

        private void ParallelSearch(MCTSNode root, int iterations, int threadId)
        {
            for (int i = 0; i < iterations; i++)
            {
                var selectedNode = Selection(root);

                if (!selectedNode.IsTerminal())
                {
                    MCTSNode nodeToExplore;

                    lock (_rootLock)
                    {
                        if (!selectedNode.IsFullyExpanded(_rules))
                        {
                            nodeToExplore = selectedNode.Expand(_rules);
                        }
                        else
                        {
                            nodeToExplore = selectedNode.BestChild(_explorationParameter);
                        }
                    }

                    if (nodeToExplore != null)
                    {
                        var simulationResult = Simulation(nodeToExplore);

                        Backpropagation(nodeToExplore, simulationResult);
                    }
                }
            }
        }

        private MCTSNode Selection(MCTSNode node)
        {
            while (!node.IsTerminal() && node.IsFullyExpanded(_rules))
            {
                var bestChild = node.BestChild(_explorationParameter);
                if (bestChild == null)
                    break;

                node = bestChild;
            }
            return node;
        }

        private double Simulation(MCTSNode node)
        {
            var simulationState = node.State.Clone();
            int playerId = node.PlayerId;

            while (!simulationState.IsGameOver())
            {
                var validMoves = _rules.GetValidMoves(simulationState);
                if (validMoves.Count == 0)
                {
                    simulationState.TransitionToNextPhase();
                    continue;
                }

                var randomMove = validMoves[_random.Next(validMoves.Count)];
                _rules.ApplyMove(simulationState, randomMove);
            }

            return EvaluateState(simulationState, playerId);
        }

        private void Backpropagation(MCTSNode node, double score)
        {
            while (node != null)
            {
                node.Update(score);
                node = node.Parent;
            }
        }

        private double EvaluateState(GameState state, int playerId)
        {
            if (playerId < 0 || playerId >= state.Players.Count)
                return 0;

            var player = state.Players[playerId];

            return player.Score / 99.0;
        }
    }
}