
namespace SharpXcom;

internal class NativeImports
{
    #region SDL2_gfx

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int lineColor(nint renderer, short x1, short y1, short x2, short y2, uint color);

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int lineRGBA(nint renderer, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int filledCircleColor(nint renderer, short x, short y, short rad, uint color);

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int characterRGBA(nint renderer, short x, short y, char c, byte r, byte g, byte b, byte a);

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int stringRGBA(nint renderer, short x, short y, string s, byte r, byte g, byte b, byte a);

    [DllImport("SDL2_gfx", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int texturedPolygon(nint renderer, [In] short[] vx, [In] short[] vy, int n, nint texture, int texture_dx, int texture_dy);

    #endregion

    #region OpenGL

    [DllImport("opengl32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void glDeleteProgram(uint program);

    [DllImport("opengl32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void glDeleteTextures(int n, ref uint textures);

    [DllImport("opengl32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint glGetError();

    [DllImport("opengl32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, nint pixels);

    #endregion
}
