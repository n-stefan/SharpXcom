
namespace SharpXcom;

internal class NativeImports
{
    #region SDL_gfx

    [DllImport("SDL_gfx")]
    internal static extern int lineColor(nint dst, short x1, short y1, short x2, short y2, uint color);

    [DllImport("SDL_gfx")]
	internal static extern int lineRGBA(nint dst, short x1, short y1, short x2, short y2, byte r, byte g, byte b, byte a);

    [DllImport("SDL_gfx")]
	internal static extern int characterRGBA(nint dst, short x, short y, sbyte c, byte r, byte g, byte b, byte a);

    [DllImport("SDL_gfx")]
	internal static extern int stringRGBA(nint dst, short x, short y, string s, byte r, byte g, byte b, byte a);

    #endregion

    #region SDL2

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint SDL_LoadBMP_RW(nint src, int freesrc);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glDeleteProgram(uint program);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glDeleteTextures(int n, ref uint textures);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern uint glGetError();

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, nint pixels);

    #endregion

    #region SDL2_mixer

    [DllImport("SDL2_mixer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern nint Mix_LoadMUS_RW(nint src);

    #endregion
}
