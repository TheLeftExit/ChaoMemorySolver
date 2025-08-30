public static class ChaoMemoryInput
{
    public static async Task KeyDown(VK key, nint windowHandle)
    {
        Win32.SendMessage(windowHandle, Win32Macros.WM_KEYDOWN, (nuint)key, 0);
        await Task.Delay(50);
        Win32.SendMessage(windowHandle, Win32Macros.WM_KEYUP, (nuint)key, 0);
    }

    private static async Task FlipCard(int handPosition, int cardPosition, nint windowHandle)
    {
        var (handX, handY) = ChaoMemory.IndexToPosition(handPosition);
        var (cardX, cardY) = ChaoMemory.IndexToPosition(cardPosition);

        while (handX != cardX)
        {
            var dx = Math.Sign(cardX - handX);
            var key = dx > 0 ? VK.RIGHT : VK.LEFT;
            await KeyDown(key, windowHandle);
            handX += dx;
            await Task.Delay(250);
        }

        while (handY != cardY)
        {
            var dy = Math.Sign(cardY - handY);
            var key = dy > 0 ? VK.DOWN : VK.UP;
            await KeyDown(key, windowHandle);
            handY += dy;
            await Task.Delay(250);
        }

        await KeyDown((VK)'X', windowHandle);
        await Task.Delay(500);
    }

    public static async Task Run(CellState[] solution, int handPosition, nint windowHandle)
    {
        if(handPosition == -1) throw new ArgumentException();

        var (handX, handY) = ChaoMemory.IndexToPosition(handPosition);
        var cards = solution
            .Select((x, i) => (Card: x, Position: ChaoMemory.IndexToPosition(i)))
            .Where(x => ChaoMemory.IsCard(x.Card))
            .ToList();

        while (cards.Any())
        {
            var targetCard = cards[0];
            cards.Remove(targetCard);
            await FlipCard(handPosition, ChaoMemory.PositionToIndex(targetCard.Position.X, targetCard.Position.Y), windowHandle);
            handPosition = ChaoMemory.PositionToIndex(targetCard.Position.X, targetCard.Position.Y);
            var secondCard = cards.Single(x => x.Card == targetCard.Card);
            cards.Remove(secondCard);
            await FlipCard(handPosition, ChaoMemory.PositionToIndex(secondCard.Position.X, secondCard.Position.Y), windowHandle);
            handPosition = ChaoMemory.PositionToIndex(secondCard.Position.X, secondCard.Position.Y);
            await Task.Delay(1000);
        }
    }
}