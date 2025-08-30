using System.Diagnostics.CodeAnalysis;

public class ChaoMemoryInstance
{
    private CellState[]? _solution;
    private bool _isShuffling;
    private CellState _movedCard;

    public CellState[] Solution => _solution ?? throw new InvalidOperationException("Solution not available");

    public ChaoMemoryInstance()
    {
        Reset();
    }

    private void Reset()
    {
        _solution = null;
        _isShuffling = false;
        _movedCard = CellState.Empty;
    }

    public bool Proc(CellState[] map, int handPosition)
    {
        // Game hidden, or we're not in Chao Memory, or something else went wrong.
        if(map.All(x => x is CellState.Empty))
        {
            Reset();
            return false;
        }

        // If we see 14 open cards, reset and re-initialize.
        if(map.Count(ChaoMemory.IsCard) == 14)
        {
            _solution = map;
            return false;
        }

        // If we're not initialized, we have nothing to work with.
        if (_solution is null) return false;

        if (!_isShuffling)
        {
            // If we've just initialized, wait until all cards are flipped.
            if (map.Count(x => x is CellState.Flipped) != 14) return false;
            _isShuffling = true;
            return false;
        }

        if(handPosition != -1)
        {
            if (ChaoMemory.IsCard(_movedCard))
            {
                throw new InvalidOperationException("Shuffling ended before all moved cards were placed.");
            }
            return true;
        }

        for (int i = 0; i < 30; i++)
        {
            var solutionCard = _solution[i];
            var currentCard = map[i];

            if (ChaoMemory.IsCard(solutionCard) && currentCard is CellState.Flipped)
            {
                continue;
            }

            if (solutionCard is CellState.Empty && currentCard is CellState.Empty)
            {
                continue;
            }

            if (ChaoMemory.IsCard(solutionCard) && currentCard is CellState.Empty && _movedCard is CellState.Empty)
            {
                _solution[i] = CellState.Empty;
                _movedCard = solutionCard;
                return false;
            }

            if(solutionCard is CellState.Empty && currentCard is CellState.Flipped && ChaoMemory.IsCard(_movedCard))
            {
               _solution[i] = _movedCard;
                _movedCard = CellState.Empty;
                return false;
            }

            throw new InvalidOperationException("Unaccounted state.");
        }
        return false;
    }
}
