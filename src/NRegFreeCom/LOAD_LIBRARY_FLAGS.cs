﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRegFreeCom
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms684179.aspx"/>
    [Flags]
    public enum LOAD_LIBRARY_FLAGS : uint
    {
        DONT_RESOLVE_DLL_REFERENCES= 0x00000001,
        LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
        LOAD_LIBRARY_AS_IMAGE_RESOURCE  =0x00000020,
        LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS=0x00001000,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
        LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
        LOAD_LIBRARY_SEARCH_USER_DIRS =  0x00000400,
        LOAD_WITH_ALTERED_SEARCH_PATH =0x00000008,

    }
}
