// This file was copied from the bsnes project.

// This is the license info, from ruby.hpp:

/*
  ruby
  version: 0.08 (2011-11-25)
  license: public domain
 */

namespace SharpXcom.Engine;

unsafe internal class OpenGL
{
    const int GL_UNSIGNED_INT_8_8_8_8_REV = 0x8367;
    const int GL_CLAMP_TO_BORDER = 0x812D;
    const int GL_BGRA = 0x80E1;
    const int GL_RGB16_EXT = 0x8054;
    const int GL_LINK_STATUS = 0x8B82;
    const int GL_COMPILE_STATUS = 0x8B81;
    const int GL_INFO_LOG_LENGTH = 0x8B84;
    const int GL_FRAGMENT_SHADER = 0x8B30;
    const int GL_VERTEX_SHADER = 0x8B31;

    /* DataType */
    internal const int GL_UNSIGNED_BYTE = 0x1401;

    /* PixelFormat */
    internal const int GL_RGB = 0x1907;

    /* AttribMask */
    const int GL_COLOR_BUFFER_BIT = 0x00004000;

    /* TextureMagFilter */
    const int GL_NEAREST = 0x2600;
    const int GL_LINEAR = 0x2601;

    /* MatrixMode */
    const int GL_MODELVIEW = 0x1700;
    const int GL_PROJECTION = 0x1701;

    /* BeginMode */
    const int GL_TRIANGLES = 0x0004;
    const int GL_TRIANGLE_STRIP = 0x0005;

    /* Boolean */
    const int GL_TRUE = 1;
    const int GL_FALSE = 0;

    /* TextureParameterName */
    const int GL_TEXTURE_MAG_FILTER = 0x2800;
    const int GL_TEXTURE_MIN_FILTER = 0x2801;
    const int GL_TEXTURE_WRAP_S = 0x2802;
    const int GL_TEXTURE_WRAP_T = 0x2803;

    /* GetTarget */
    const int GL_ALPHA_TEST = 0x0BC0;
    const int GL_BLEND = 0x0BE2;
    const int GL_DEPTH_TEST = 0x0B71;
    const int GL_POLYGON_SMOOTH = 0x0B41;
    const int GL_STENCIL_TEST = 0x0B90;
    const int GL_DITHER = 0x0BD0;
    const int GL_TEXTURE_2D = 0x0DE1;
    const int GL_UNPACK_ROW_LENGTH = 0x0CF2;

    /* ErrorCode */
    const int GL_NO_ERROR = 0;
    const int GL_INVALID_ENUM = 0x0500;
    const int GL_INVALID_VALUE = 0x0501;
    const int GL_INVALID_OPERATION = 0x0502;
    const int GL_STACK_OVERFLOW = 0x0503;
    const int GL_STACK_UNDERFLOW = 0x0504;
    const int GL_OUT_OF_MEMORY = 0x0505;

    uint[] gltexture;
    uint glprogram;
    internal bool linear;
    bool shader_support;
    nint buffer;
    internal Surface buffer_surface;
    internal uint iwidth, iheight, iformat, ibpp;

    internal static bool checkErrors;

    internal static delegate* unmanaged<int, int, int, int, uint, uint, nint, void> glReadPixels;
    static delegate* unmanaged<uint> glGetError;
    static delegate* unmanaged<uint, uint, out int, void> glGetShaderiv;
    static delegate* unmanaged<uint, int, int[], out string, void> glGetShaderInfoLog;
    static delegate* unmanaged<uint, void> glDeleteShader;
    static delegate* unmanaged<uint, void> glCompileShader;
    static delegate* unmanaged<uint, int, string, int, void> glShaderSource;
    static delegate* unmanaged<uint, void> glDeleteProgram;
    static delegate* unmanaged<int, uint[], void> glDeleteTextures;
    static delegate* unmanaged<uint> glCreateProgram;
    static delegate* unmanaged<uint, void> glUseProgram;
    static delegate* unmanaged<uint, byte> glIsProgram;
    static delegate* unmanaged<uint, byte> glIsShader;
    static delegate* unmanaged<uint, uint> glCreateShader;
    static delegate* unmanaged<uint, uint, void> glAttachShader;
    static delegate* unmanaged<uint, uint, void> glDetachShader;
    static delegate* unmanaged<uint, int, int[], uint[], void> glGetAttachedShaders;
    static delegate* unmanaged<uint, uint, out int, void> glGetProgramiv;
    static delegate* unmanaged<uint, int, int[], out string, void> glGetProgramInfoLog;
    static delegate* unmanaged<uint, void> glLinkProgram;
    static delegate* unmanaged<uint, string, int> glGetUniformLocation;
    static delegate* unmanaged<int, int, void> glUniform1i;
    static delegate* unmanaged<int, int, float[], void> glUniform2fv;
    static delegate* unmanaged<int, int, float[], void> glUniform4fv;
    static delegate* unmanaged<nint> glXGetCurrentDisplay;
    static delegate* unmanaged<uint> glXGetCurrentDrawable;
    static delegate* unmanaged<nint, uint, int, void> glXSwapIntervalEXT;
    static delegate* unmanaged<int, uint> wglSwapIntervalEXT;
    static delegate* unmanaged<uint, void> glDisable;
    static delegate* unmanaged<uint, void> glEnable;
    static delegate* unmanaged<float, float, void> glTexCoord2f;
    static delegate* unmanaged<int, int, int, void> glVertex3i;
    static delegate* unmanaged<uint, uint, int, void> glTexParameteri;
    static delegate* unmanaged<uint, void> glBegin;
    static delegate* unmanaged<void> glEnd;
    static delegate* unmanaged<void> glLoadIdentity;
    static delegate* unmanaged<uint, void> glMatrixMode;
    static delegate* unmanaged<double, double, double, double, double, double, void> glOrtho;
    static delegate* unmanaged<int, int, int, int, void> glViewport;
    static delegate* unmanaged<uint, int, void> glPixelStorei;
    static delegate* unmanaged<uint, int, int, int, int, int, uint, uint, nint, void> glTexSubImage2D;
    static delegate* unmanaged<uint, void> glClear;
    static delegate* unmanaged<float, float, float, float, void> glClearColor;
    static delegate* unmanaged<int, uint[], void> glGenTextures;
    static delegate* unmanaged<uint, uint[], void> glBindTexture;
    static delegate* unmanaged<uint, int, int, int, int, int, uint, uint, nint, void> glTexImage2D;

    OpenGL()
    {
        gltexture = null;
        glprogram = 0;
        linear = false;
        shader_support = false;
        buffer = nint.Zero;
        buffer_surface = null;
        iwidth = 0;
        iheight = 0;
        iformat = GL_UNSIGNED_INT_8_8_8_8_REV; // this didn't seem to be set anywhere before...
        ibpp = 32;                             // ...nor this
    }

    ~OpenGL() =>
        term();

    void term()
    {
        if (gltexture != null)
        {
            glDeleteTextures(1, gltexture);
            glErrorCheck();
            gltexture = null;
        }

        if (glprogram != 0)
        {
            glDeleteProgram(glprogram);
            glprogram = 0;
        }

        if (buffer != nint.Zero)
        {
            buffer = nint.Zero;
            iwidth = 0;
            iheight = 0;
        }

        buffer_surface = null;
    }

    static bool reported = false;
    internal static void glErrorCheck([CallerFilePath] string file = default, [CallerLineNumber] int line = default)
    {
        uint glErr;
	    if (checkErrors && !reported && (glErr = glGetError()) != GL_NO_ERROR)
	    {
		    reported = true;

		    do
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {file}:{line}: glGetError() complaint: {strGLError(glErr)}");
		    } while ((glErr = glGetError()) != GL_NO_ERROR);
	    }
    }

    static string strGLError(uint glErr)
    {
        string err;

        switch (glErr)
        {
            case GL_INVALID_ENUM:
                err = "GL_INVALID_ENUM";
                break;
            case GL_INVALID_VALUE:
                err = "GL_INVALID_VALUE";
                break;
            case GL_INVALID_OPERATION:
                err = "GL_INVALID_OPERATION";
                break;
            case GL_STACK_OVERFLOW:
                err = "GL_STACK_OVERFLOW";
                break;
            case GL_STACK_UNDERFLOW:
                err = "GL_STACK_UNDERFLOW";
                break;
            case GL_OUT_OF_MEMORY:
                err = "GL_OUT_OF_MEMORY";
                break;
            case GL_NO_ERROR:
                err = "No error! How did you even reach this code?";
                break;
            default:
                err = "Unknown error code!";
                break;
        }

        return err;
    }

    internal void setVSync(bool sync)
    {
        int interval = sync ? 1 : 0;
        if (glXGetCurrentDisplay != null && glXGetCurrentDrawable != null && glXSwapIntervalEXT != null)
        {
            var dpy = glXGetCurrentDisplay();
            glErrorCheck();
            uint drawable = glXGetCurrentDrawable();
            glErrorCheck();

            if (drawable != 0)
            {
                glXSwapIntervalEXT(dpy, drawable, interval);
                glErrorCheck();
                // Log(LOG_INFO) << "Made an attempt to set vsync via GLX.";
            }
        }
        else if (wglSwapIntervalEXT != null)
        {
            wglSwapIntervalEXT(interval);
            glErrorCheck();
            // Log(LOG_INFO) << "Made an attempt to set vsync via WGL.";
        }
    }

    internal void init(int w, int h)
    {
        //disable unused features
        glDisable(GL_ALPHA_TEST);
        glDisable(GL_BLEND);
        glDisable(GL_DEPTH_TEST);
        glDisable(GL_POLYGON_SMOOTH);
        glDisable(GL_STENCIL_TEST);
        glErrorCheck();

        //enable useful and required features
        glEnable(GL_DITHER);
        glEnable(GL_TEXTURE_2D);
        glErrorCheck();

        glDisable = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glDisable");
        glEnable = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glEnable");
        glGetError = (delegate* unmanaged<uint>)SDL_GL_GetProcAddress("glGetError");
        glDeleteTextures = (delegate* unmanaged<int, uint[], void>)SDL_GL_GetProcAddress("glDeleteTextures");
        glReadPixels = (delegate* unmanaged<int, int, int, int, uint, uint, nint, void>)SDL_GL_GetProcAddress("glReadPixels");
        glTexCoord2f = (delegate* unmanaged<float, float, void>)SDL_GL_GetProcAddress("glTexCoord2f");
        glVertex3i = (delegate* unmanaged<int, int, int, void>)SDL_GL_GetProcAddress("glVertex3i");
        glTexParameteri = (delegate* unmanaged<uint, uint, int, void>)SDL_GL_GetProcAddress("glTexParameteri");
        glBegin = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glBegin");
        glEnd = (delegate* unmanaged<void>)SDL_GL_GetProcAddress("glEnd");
        glLoadIdentity = (delegate* unmanaged<void>)SDL_GL_GetProcAddress("glLoadIdentity");
        glMatrixMode = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glMatrixMode");
        glOrtho = (delegate* unmanaged<double, double, double, double, double, double, void>)SDL_GL_GetProcAddress("glOrtho");
        glViewport = (delegate* unmanaged<int, int, int, int, void>)SDL_GL_GetProcAddress("glViewport");
        glPixelStorei = (delegate* unmanaged<uint, int, void>)SDL_GL_GetProcAddress("glPixelStorei");
        glTexSubImage2D = (delegate* unmanaged<uint, int, int, int, int, int, uint, uint, nint, void>)SDL_GL_GetProcAddress("glTexSubImage2D");
        glClear = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glClear");
        glClearColor = (delegate* unmanaged<float, float, float, float, void>)SDL_GL_GetProcAddress("glClearColor");
        glGenTextures = (delegate* unmanaged<int, uint[], void>)SDL_GL_GetProcAddress("glGenTextures");
        glBindTexture = (delegate* unmanaged<uint, uint[], void>)SDL_GL_GetProcAddress("glBindTexture");
        glTexImage2D = (delegate* unmanaged<uint, int, int, int, int, int, uint, uint, nint, void>)SDL_GL_GetProcAddress("glTexImage2D");

        //bind shader functions
        glCreateProgram = (delegate* unmanaged<uint>)SDL_GL_GetProcAddress("glCreateProgram");
        glDeleteProgram = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glDeleteProgram");
        glUseProgram = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glUseProgram");
        glIsProgram = (delegate* unmanaged<uint, byte>)SDL_GL_GetProcAddress("glIsProgram");
        glIsShader = (delegate* unmanaged<uint, byte>)SDL_GL_GetProcAddress("glIsShader");
        glCreateShader = (delegate* unmanaged<uint, uint>)SDL_GL_GetProcAddress("glCreateShader");
        glDeleteShader = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glDeleteShader");
        glShaderSource = (delegate* unmanaged<uint, int, string, int, void>)SDL_GL_GetProcAddress("glShaderSource");
        glCompileShader = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glCompileShader");
        glAttachShader = (delegate* unmanaged<uint, uint, void>)SDL_GL_GetProcAddress("glAttachShader");
        glDetachShader = (delegate* unmanaged<uint, uint, void>)SDL_GL_GetProcAddress("glDetachShader");
        glGetAttachedShaders = (delegate* unmanaged<uint, int, int[], uint[], void>)SDL_GL_GetProcAddress("glGetAttachedShaders");
        glGetProgramiv = (delegate* unmanaged<uint, uint, out int, void>)SDL_GL_GetProcAddress("glGetProgramiv");
        glGetProgramInfoLog = (delegate* unmanaged<uint, int, int[], out string, void>)SDL_GL_GetProcAddress("glGetProgramInfoLog");
        glGetShaderiv = (delegate* unmanaged<uint, uint, out int, void>)SDL_GL_GetProcAddress("glGetShaderiv");
        glGetShaderInfoLog = (delegate* unmanaged<uint, int, int[], out string, void>)SDL_GL_GetProcAddress("glGetShaderInfoLog");
        glLinkProgram = (delegate* unmanaged<uint, void>)SDL_GL_GetProcAddress("glLinkProgram");
        glGetUniformLocation = (delegate* unmanaged<uint, string, int>)SDL_GL_GetProcAddress("glGetUniformLocation");
        glUniform1i = (delegate* unmanaged<int, int, void>)SDL_GL_GetProcAddress("glUniform1i");
        glUniform2fv = (delegate* unmanaged<int, int, float[], void>)SDL_GL_GetProcAddress("glUniform2fv");
        glUniform4fv = (delegate* unmanaged<int, int, float[], void>)SDL_GL_GetProcAddress("glUniform4fv");

        shader_support = glCreateProgram != null && glDeleteProgram != null && glUseProgram != null && glCreateShader != null
        && glDeleteShader != null && glShaderSource != null && glCompileShader != null && glAttachShader != null
        && glDetachShader != null && glLinkProgram != null && glGetUniformLocation != null && glIsProgram != null && glIsShader != null
        && glUniform1i != null && glUniform2fv != null && glUniform4fv != null && glGetAttachedShaders != null
        && glGetShaderiv != null && glGetShaderInfoLog != null && glGetProgramiv != null && glGetProgramInfoLog != null;

        glXGetCurrentDisplay = (delegate* unmanaged<nint>)SDL_GL_GetProcAddress("glXGetCurrentDisplay");
        glXGetCurrentDrawable = (delegate* unmanaged<uint>)SDL_GL_GetProcAddress("glXGetCurrentDrawable");
        glXSwapIntervalEXT = (delegate* unmanaged<nint, uint, int, void>)SDL_GL_GetProcAddress("glXSwapIntervalEXT");

        wglSwapIntervalEXT = (delegate* unmanaged<int, uint>)SDL_GL_GetProcAddress("wglSwapIntervalEXT");

        if (shader_support)
        {
            if (glprogram != 0)
            {
                if (glIsProgram(glprogram) != 0)
                {
                    glDeleteProgram(glprogram);
                    glErrorCheck();
                }
            }
            glprogram = glCreateProgram();
            glErrorCheck();
        }

        //create surface texture
        resize((uint)w, (uint)h);
    }

    internal void refresh(bool smooth, uint inwidth, uint inheight, uint outwidth, uint outheight, int topBlackBand, int bottomBlackBand, int leftBlackBand, int rightBlackBand)
    {
        while (glGetError() != GL_NO_ERROR); // clear possible error from who knows where
        clear();
        if (shader_support && glprogram != 0)
        {
            glUseProgram(glprogram);
            glErrorCheck();
            int location;

            float[] inputSize = { (float)inwidth, (float)inheight };
            location = glGetUniformLocation(glprogram, "rubyInputSize");
            glUniform2fv(location, 1, inputSize);

            float[] outputSize = { (float)outwidth, (float)outheight };
            location = glGetUniformLocation(glprogram, "rubyOutputSize");
            glUniform2fv(location, 1, outputSize);

            float[] textureSize = { (float)iwidth, (float)iheight };
            location = glGetUniformLocation(glprogram, "rubyTextureSize");
            glUniform2fv(location, 1, textureSize);
            glErrorCheck();
        }

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, smooth ? GL_LINEAR : GL_NEAREST);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, smooth ? GL_LINEAR : GL_NEAREST);

        glErrorCheck();

        glMatrixMode(GL_PROJECTION);
        glLoadIdentity();
        glOrtho(0, outwidth, 0, outheight, -1.0, 1.0);
        glViewport(0, 0, (int)outwidth, (int)outheight);

        glMatrixMode(GL_MODELVIEW);
        glLoadIdentity();

        glErrorCheck();

        var bpp = Surface.getFormat(buffer_surface.getSurface()).BytesPerPixel;
        glPixelStorei(GL_UNPACK_ROW_LENGTH, buffer_surface.getSurface().pitch / bpp);

        glErrorCheck();

        glTexSubImage2D(GL_TEXTURE_2D,
            /* mip-map level = */ 0, /* x = */ 0, /* y = */ 0,
            (int)iwidth, (int)iheight, GL_BGRA, iformat, buffer);

        //OpenGL projection sets 0,0 as *bottom-left* of screen.
        //therefore, below vertices flip image to support top-left source.
        //texture range = x1:0.0, y1:0.0, x2:1.0, y2:1.0
        //vertex range = x1:0, y1:0, x2:width, y2:height
        if (leftBlackBand + rightBlackBand + topBlackBand + bottomBlackBand == 0)
        {
            double w = (double)inwidth / (double)iwidth * 2;
            double h = (double)inheight / (double)iheight * 2;
            int u1 = 0;
            int u2 = (int)(outwidth * 2);
            int v1 = (int)outheight;
            int v2 = (int)-outheight;

            glBegin(GL_TRIANGLES);
            glTexCoord2f(0, 0); glVertex3i(u1, v1, 0);
            glTexCoord2f((float)w, 0); glVertex3i(u2, v1, 0);
            glTexCoord2f(0, (float)h); glVertex3i(u1, v2, 0);
            glEnd();
        }
        else
        {
            double w = (double)inwidth / (double)iwidth;
            double h = (double)inheight / (double)iheight;
            int u1 = leftBlackBand;
            int u2 = (int)(outwidth - rightBlackBand);
            int v1 = (int)(outheight - topBlackBand);
            int v2 = bottomBlackBand;

            glBegin(GL_TRIANGLE_STRIP);
            glTexCoord2f(0, 0); glVertex3i(u1, v1, 0);
            glTexCoord2f((float)w, 0); glVertex3i(u2, v1, 0);
            glTexCoord2f(0, (float)h); glVertex3i(u1, v2, 0);
            glTexCoord2f((float)w, (float)h); glVertex3i(u2, v2, 0);
            glEnd();
        }
        glErrorCheck();

        if (shader_support)
        {
            glUseProgram(0);
            glErrorCheck();
        }
    }

    void clear()
    {
        //memset(buffer, 0, iwidth * iheight * ibpp);
        glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        glErrorCheck();
        glClear(GL_COLOR_BUFFER_BIT);
        glErrorCheck();
    }

    void resize(uint width, uint height)
    {
        if (gltexture == null)
        {
            glGenTextures(1, gltexture);
            glErrorCheck();
        }

        iwidth = width;
        iheight = height;
        if (buffer_surface != null) buffer_surface = null;
        buffer_surface = new Surface((int)iwidth, (int)iheight, 0, 0, (int)ibpp); // use OpenXcom's Surface class to get an aligned buffer with bonus SDL_Surface
        buffer = buffer_surface.getSurface().pixels;

        glBindTexture(GL_TEXTURE_2D, gltexture);
        glErrorCheck();
        glPixelStorei(GL_UNPACK_ROW_LENGTH, (int)iwidth);
        glErrorCheck();
        glTexImage2D(GL_TEXTURE_2D,
            /* mip-map level = */ 0, /* internal format = */ GL_RGB16_EXT,
            (int)width, (int)height, /* border = */ 0, /* format = */ GL_BGRA,
            iformat, buffer);
        glErrorCheck();
    }

    internal bool set_shader(string source_yaml_filename)
    {
	    if (!shader_support) return false;

	    if (glprogram != 0)
	    {
		    glDeleteProgram(glprogram);
		    glprogram = 0;
	    }

	    if (!string.IsNullOrEmpty(source_yaml_filename) && source_yaml_filename[0] != '\0')
	    {
		    glprogram = glCreateProgram();
		    if (glprogram == 0)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to create GLSL shader program");
			    return false;
		    }
		    try
		    {
                using var input = new StreamReader(source_yaml_filename);
                var yaml = new YamlStream();
                yaml.Load(input);
                var document = (YamlMappingNode)yaml.Documents[0].RootNode; //YAML.Node document = YAML.LoadFile(source_yaml_filename);

                bool is_glsl;
			    string language = document.Children["language"].ToString();
			    is_glsl = (language == "GLSL");

			    linear = bool.Parse(document.Children["linear"].ToString()); // some shaders want texture linear interpolation and some don't
			    string fragment_source = document.Children["fragment"] != null ? document.Children["fragment"].ToString() : string.Empty;
			    string vertex_source = document.Children["vertex"] != null ? document.Children["vertex"].ToString() : string.Empty;

			    if (is_glsl)
			    {
				    if (!string.IsNullOrEmpty(fragment_source)) set_fragment_shader(fragment_source);
				    if (!string.IsNullOrEmpty(vertex_source)) set_vertex_shader(vertex_source);
			    }
			    else
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Unexpected shader language \"{document.Children["language"]}\"");
			    }
		    }
		    catch (YamlException e)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {source_yaml_filename}: {e.Message}");
			    glDeleteProgram(glprogram);
			    glprogram = 0;
			    return false;
		    }

            glLinkProgram(glprogram);
            glErrorCheck();
            glGetProgramiv(glprogram, GL_LINK_STATUS, out int linkStatus);
		    glErrorCheck();
		    if (linkStatus != GL_TRUE)
		    {
                glGetProgramiv(glprogram, GL_INFO_LOG_LENGTH, out int infoLogLength);
			    glErrorCheck();
			    if (infoLogLength == 0)
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} OpenGL shader link failed: No log returned from driver");
			    }
			    else
			    {
				    glGetProgramInfoLog(glprogram, infoLogLength, null, out string infoLog);
				    glErrorCheck();

                    Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} OpenGL shader link failed \"{infoLog}\"");

				    infoLog = null;
			    }
			    glDeleteProgram(glprogram);
			    glErrorCheck();
			    glprogram = 0;
		    }
	    }
	    return glprogram != 0;
    }

    void set_fragment_shader(string source)
    {
        int fragmentshader = (int)createShader(GL_FRAGMENT_SHADER, source);
	    if (fragmentshader != 0)
	    {
		    glAttachShader(glprogram, (uint)fragmentshader);
		    glErrorCheck();
		    glDeleteShader((uint)fragmentshader);
	    }
    }

    void set_vertex_shader(string source)
    {
        int vertexshader = (int)createShader(GL_VERTEX_SHADER, source);
	    if (vertexshader != 0)
	    {
		    glAttachShader(glprogram, (uint)vertexshader);
		    glErrorCheck();
		    glDeleteShader((uint)vertexshader);
	    }
    }

    static uint createShader(uint type, string source)
    {
        uint shader = glCreateShader(type);
	    glErrorCheck();
        glShaderSource(shader, 1, source, 0);
	    glErrorCheck();
	    glCompileShader(shader);
	    glErrorCheck();

	    glGetShaderiv(shader, GL_COMPILE_STATUS, out int compileSuccess);
	    glErrorCheck();
	    if (compileSuccess != GL_TRUE)
	    {
		    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, out int infoLogLength);
		    glErrorCheck();
		    if (infoLogLength == 0)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} OpenGL shader compilation failed: No log returned from driver");
		    }
		    else
		    {
                glGetShaderInfoLog(shader, infoLogLength, null, out string infoLog);
			    glErrorCheck();

                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} OpenGL shader compilation failed: \"{infoLog}\"");

			    infoLog = null;
		    }
		    glDeleteShader(shader);
		    glErrorCheck();
		    shader = 0;
	    }

        return shader;
    }

    void @lock(ref nint data, ref uint pitch)
    {
	    pitch = iwidth * ibpp;
	    data = buffer;
    }
}
