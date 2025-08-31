using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

public static partial class Win32
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessagew
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(
        nint hWnd,
        uint msg,
        nuint wParam,
        nint lParam
    );

    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-findwindowexw
    [LibraryImport("user32.dll")]
    public static partial nint FindWindowExW(
        nint hWndParent,
        nint hWndChildAfter,
        ReadOnlySpan<char> lpszClass,
        ReadOnlySpan<char> lpszWindow
    );

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetClientRect(
        nint hWnd,
        out RECT lpRect
    );

    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-clienttoscreen
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ClientToScreen(
        nint hWnd,
        ref POINT lpPoint
    );

    public static bool ClientToScreen(nint hWnd, ref RECT rect)
    {
        var point = new POINT { x = rect.left, y = rect.top };
        var pointOld = point;
        if (!ClientToScreen(hWnd, ref point)) return false;
        var offsetX = point.x - pointOld.x;
        var offsetY = point.y - pointOld.y;

        rect.left += offsetX;
        rect.top += offsetY;
        rect.right += offsetX;
        rect.bottom += offsetY;
        return true;
    }
}

public record struct RECT(int left, int top, int right, int bottom);
public record struct POINT(int x, int y);
