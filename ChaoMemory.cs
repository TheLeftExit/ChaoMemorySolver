using System.Buffers;
using System.Diagnostics;
using System.Drawing.Imaging;

public enum CellState
{
    Empty = -1,
    Flipped = -2,
    Card0 = 0,
    Card1 = 1,
    Card2 = 2,
    Card3 = 3,
    Card4 = 4,
    Card5 = 5,
    Card6 = 6
}

public static class ChaoMemory
{
    public static Bitmap Hand = (Bitmap)Image.FromFile("Sprites\\hand.png");
    public static Bitmap FlippedCard = (Bitmap)Image.FromFile("Sprites\\card_flipped.png");
    public static Bitmap[] EmptySpaces = Enumerable.Range(0, 3).Select(x => (Bitmap)Image.FromFile($"Sprites\\empty_{x}.png")).ToArray();
    public static Bitmap[] Cards = Enumerable.Range(0, 7).Select(x => (Bitmap)Image.FromFile($"Sprites\\card_{x}.png")).ToArray();

    private static readonly BitmapDataSnapshot _handData = BitmapDataSnapshot.FromBitmap(Hand);
    private static readonly BitmapDataSnapshot _flippedCardData = BitmapDataSnapshot.FromBitmap(FlippedCard);
    private static readonly BitmapDataSnapshot[] _emptySpacesData = EmptySpaces.Select(BitmapDataSnapshot.FromBitmap).ToArray();
    private static readonly BitmapDataSnapshot[] _cardsData = Cards.Select(BitmapDataSnapshot.FromBitmap).ToArray();

    public static nint GetWindowHandle()
    {
        var windowHandle = Process.GetProcessesByName("mGBA").Single().MainWindowHandle;
        windowHandle = Win32.FindWindowExW(windowHandle, 0, "Qt51516QWindowOwnDCIcon", "mGBA");
        return windowHandle;
    }

    private static unsafe Bitmap GetGBABitmap()
    {
        var windowHandle = GetWindowHandle();
        Win32.GetClientRect(windowHandle, out var clientRect);
        Win32.ClientToScreen(windowHandle, ref clientRect);
        var (width, height) = (clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);
        if ((width, height) is not (240, 160))
        {
            throw new InvalidOperationException();
        }

        var bitmap = new Bitmap(width, height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(clientRect.left, clientRect.top, 0, 0, new Size(width, height));
        }
        return bitmap;
    }

    private static double CompareImages(BitmapDataSnapshot surface, int offsetX, int offsetY, BitmapDataSnapshot image)
    {
        var totalPixels = 0;
        var matchingPixels = 0;
        for (int x = 0; x < image.Width; x++)
        {
            for(int y = 0; y < image.Height; y++)
            {
                var c1 = image.GetColor(x, y);
                if (c1.A != 255) continue;

                var c2 = surface.GetColor(offsetX + x, offsetY + y);
                
                totalPixels++;
                if(CompareColors(c1, c2))
                {
                    matchingPixels++;
                }
            }
        }
        return (double)matchingPixels / totalPixels;
    }

    private static bool CompareColors(Color c1, Color c2)
    {
        var dR = c1.R - c2.R;
        var dG = c1.G - c2.G;
        var dB = c1.B - c2.B;
        var distance = Math.Sqrt(dR * dR + dG * dG + dB * dB);
        return distance < 10;
    }

    public static (int X, int Y) IndexToPosition(int index) => (index % 6, index / 6);
    public static int PositionToIndex(int x, int y) => y * 6 + x;
    public static bool IsCard(CellState state) => state
        is CellState.Card0
        or CellState.Card1
        or CellState.Card2
        or CellState.Card3
        or CellState.Card4
        or CellState.Card5
        or CellState.Card6;

    public static unsafe CellState[] ReadImage(out int handPosition)
    {
        using var gameBitmap = GetGBABitmap();
        var data = BitmapDataSnapshot.FromBitmap(gameBitmap);

        var result = new CellState[30];

        Parallel.For(0, 30, i =>
        {
            (int x, int y) = IndexToPosition(i);

            int offsetX = x * 32;
            int offsetY = y * 32;

            foreach (var image in _emptySpacesData)
            {
                if (CompareImages(data, offsetX, offsetY, image) > 0.8)
                {
                    result[i] = CellState.Empty;
                    return;
                }
            }
            if (CompareImages(data, offsetX, offsetY, _flippedCardData) > 0.5)
            {
                result[i] = CellState.Flipped;
                return;
            }
            for (int j = 0; j < _cardsData.Length; j++)
            {
                if (CompareImages(data, offsetX, offsetY, _cardsData[j]) > 0.8)
                {
                    result[i] = (CellState)j;
                    return;
                }
            }
            result[i] = CellState.Empty;
        });

        for(int i = 0; i < 30; i++)
        {
            (int x, int y) = IndexToPosition(i);
            int offsetX = x * 32;
            int offsetY = y * 32;
            if (y > 0) offsetY -= 8;
            if(CompareImages(data, offsetX, offsetY, _handData) > 0.8)
            {
                handPosition = i;
                return result;
            }
        }

        handPosition = -1;
        return result;
    }

    public static Bitmap Render(CellState[] state, int handPosition)
    {
        var bitmap = new Bitmap(32 * 6, 32 * 5);
        using var graphics = Graphics.FromImage(bitmap);
        for (int i = 0; i < state.Length; i++)
        {
            var (x, y) = IndexToPosition(i);
            var image = state[i] switch
            {
                CellState.Flipped => FlippedCard,
                CellState.Card0 => Cards[0],
                CellState.Card1 => Cards[1],
                CellState.Card2 => Cards[2],
                CellState.Card3 => Cards[3],
                CellState.Card4 => Cards[4],
                CellState.Card5 => Cards[5],
                CellState.Card6 => Cards[6],
                _ => null
            };
            if(image is not null)
            {
                graphics.DrawImage(image, x * 32, y * 32);
            }
            if(i == handPosition)
            {
                graphics.DrawImage(Hand, x * 32, y * 32);
            }
        }
        return bitmap;
    }


}

// If we continuously work with BitmapData directly, accessing pixel data will randomly throw AccessViolationException :(
public class BitmapDataSnapshot
{
    public int Width { get; }
    public int Height { get; }
    public Color[] Data { get; }

    public unsafe BitmapDataSnapshot(BitmapData data)
    {
        Width = data.Width;
        Height = data.Height;
        Data = new Color[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var argb = *(uint*)(data.Scan0 + y * data.Stride + x * 4);
                Data[y * Width + x] = Color.FromArgb((int)argb);
            }
        }
    }

    public static BitmapDataSnapshot FromBitmap(Bitmap bitmap)
    {
        var unit = GraphicsUnit.Pixel;
        var data = bitmap.LockBits(
            Rectangle.Round(bitmap.GetBounds(ref unit)),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb
        );
        try
        {
            return new BitmapDataSnapshot(data);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    public Color GetColor(int x, int y) => Data[y * Width + x];
}