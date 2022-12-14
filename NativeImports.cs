
namespace SharpXcom;

internal class NativeImports
{
    #region SDL_gfx

    [DllImport("SDL_gfx")]
    internal static extern int lineColor(IntPtr dst, short x1, short y1, short x2, short y2, uint color);

    #endregion

    #region SDL2

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr SDL_LoadBMP_RW(IntPtr src, int freesrc);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glDeleteProgram(uint program);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glDeleteTextures(int n, ref uint textures);

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern uint glGetError();

    //[DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
    //internal static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);

    #endregion

    #region SDL2_mixer

    [DllImport("SDL2_mixer", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr Mix_LoadMUS_RW(IntPtr src);

    #endregion
}
